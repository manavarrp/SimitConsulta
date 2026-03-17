using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using SimitConsulta.Application.Common.Mappings;
using SimitConsulta.Application.Common.Results;
using SimitConsulta.Application.Features.PlateQueries.DTOs;
using SimitConsulta.Application.Interfaces;
using SimitConsulta.Domain.Entities;
using SimitConsulta.Domain.Enums;
using SimitConsulta.Domain.ValueObjects;

namespace SimitConsulta.Application.Features.PlateQueries.Commands.QueryPlate;

// ── Command ───────────────────────────────────────────────

/// <summary>
/// Command CQRS para consultar las multas de una placa en el SIMIT.
/// Record inmutable — se crea y envía, no se modifica.
/// MediatR lo enruta automáticamente al QueryPlateHandler.
/// </summary>
public record QueryPlateCommand(
    string    Plate,
    QueryType QueryType = QueryType.Individual)
    : IRequest<Result<PlateQueryDto>>;

// ── Validator ─────────────────────────────────────────────

/// <summary>
/// Valida los datos de entrada del QueryPlateCommand.
/// Se ejecuta automáticamente via ValidationBehavior.
/// Delega la validación del formato al Value Object Plate.TryCreate().
/// </summary>
public class QueryPlateValidator : AbstractValidator<QueryPlateCommand>
{
    public QueryPlateValidator()
    {
        RuleFor(x => x.Plate)
            .NotEmpty()
                .WithMessage("La placa es obligatoria.")
            .Must(BeValidPlate)
                .WithMessage(cmd =>
                    $"Formato de placa inválido: '{cmd.Plate}'. " +
                    "Ejemplos: ABC123 (carro), ABC12D (moto).");
    }

    private static bool BeValidPlate(string input) =>
        Plate.TryCreate(input, out _, out _);
}

// ── Handler ───────────────────────────────────────────────

/// <summary>
/// Orquesta el caso de uso "consultar placa en el SIMIT".
///
/// Flujo:
/// 1. Crear Value Object Plate — garantiza formato válido.
/// 2. Crear aggregate PlateQuery y persistir (estado: Procesando).
///    Garantiza trazabilidad aunque el proceso falle después.
/// 3. Llamar ISimitGateway — resuelve captcha PoW y consulta SIMIT.
/// 4. Aggregate construye hijas y determina estado final.
/// 5. Persistir resultado final.
/// 6. Retornar Result.Ok(dto) o Result.Fail(error).
/// </summary>
public class QueryPlateHandler
    : IRequestHandler<QueryPlateCommand, Result<PlateQueryDto>>
{
    private readonly IPlateQueryRepository _repo;
    private readonly ISimitGateway         _gateway;
    private readonly ILogger<QueryPlateHandler> _logger;

    public QueryPlateHandler(
        IPlateQueryRepository      repo,
        ISimitGateway              gateway,
        ILogger<QueryPlateHandler> logger)
    {
        _repo    = repo;
        _gateway = gateway;
        _logger  = logger;
    }

    public async Task<Result<PlateQueryDto>> Handle(
        QueryPlateCommand request, CancellationToken ct)
    {
        // 1. Validar formato y crear Value Object
        if (!Plate.TryCreate(request.Plate, out var plate, out var error))
            return Result.Fail<PlateQueryDto>(error);

        // 2. Crear aggregate y persistir antes de llamar al SIMIT
        var query = PlateQuery.Create(plate!.Value, request.QueryType);
        await _repo.AddAsync(query, ct);

        try
        {
            // 3. Consultar SIMIT
            var response = await _gateway.QueryPlateAsync(plate.Value, ct);

            // 4. Aggregate construye hijas y determina estado
            query.MarkSuccessful(
                response.RawResponse,
                response.TotalAmount,
                response.Fines,
                response.Summons);

            // 5. Persistir resultado
            await _repo.UpdateAsync(query, ct);

            _logger.LogInformation(
                "Query successful plate {Plate}: {Fines} fines, {Summons} summons",
                plate.Value, query.FinesCount, query.SummonsCount);

            return Result.Ok(query.ToDto());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying plate {Plate}", plate.Value);

            query.MarkFailed(ex.Message);
            await _repo.UpdateAsync(query, ct);

            return Result.Fail<PlateQueryDto>(
                $"Error querying plate: {ex.Message}");
        }
    }
}