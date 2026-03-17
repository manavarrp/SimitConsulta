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
/// Command para consultar multas de una placa.
/// El frontend resuelve el captcha PoW y envía el token
/// junto con la placa — el backend no necesita contactar
/// el servidor del captcha directamente.
/// </summary>
public record QueryPlateCommand(
    string Plate,
    string CaptchaToken,
    QueryType QueryType = QueryType.Individual)
    : IRequest<Result<PlateQueryDto>>;

// ── Validator ─────────────────────────────────────────────

/// <summary>
/// Valida placa y token antes de llegar al handler.
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

        RuleFor(x => x.CaptchaToken)
            .NotEmpty()
                .WithMessage("El token del captcha es obligatorio.");
    }

    private static bool BeValidPlate(string input) =>
        Plate.TryCreate(input, out _, out _);
}

// ── Handler ───────────────────────────────────────────────

/// <summary>
/// Orquesta el caso de uso "consultar placa en el SIMIT".
/// El token captcha ya viene resuelto desde el frontend.
/// </summary>
public class QueryPlateHandler
    : IRequestHandler<QueryPlateCommand, Result<PlateQueryDto>>
{
    private readonly IPlateQueryRepository _repo;
    private readonly ISimitGateway _gateway;
    private readonly ILogger<QueryPlateHandler> _logger;

    public QueryPlateHandler(
        IPlateQueryRepository repo,
        ISimitGateway gateway,
        ILogger<QueryPlateHandler> logger)
    {
        _repo = repo;
        _gateway = gateway;
        _logger = logger;
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
            // 3. Consultar SIMIT con el token que envió el frontend
            var response = await _gateway.QueryPlateAsync(
                plate.Value,
                request.CaptchaToken,   // ← token del frontend
                ct);

            // 4. Aggregate construye hijas y determina estado
            query.MarkSuccessful(
                response.RawResponse,
                response.TotalAmount,
                response.Fines,
                response.Summons);

            // 5. Persistir resultado
            await _repo.UpdateAsync(query, ct);

            _logger.LogInformation(
                "Query successful plate {Plate}: {Fines} fines, " +
                "{Summons} summons",
                plate.Value, query.FinesCount, query.SummonsCount);

            return Result.Ok(query.ToDto());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error querying plate {Plate}", plate.Value);

            query.MarkFailed(ex.Message);
            await _repo.UpdateAsync(query, ct);

            return Result.Fail<PlateQueryDto>(
                $"Error querying plate: {ex.Message}");
        }
    }
}