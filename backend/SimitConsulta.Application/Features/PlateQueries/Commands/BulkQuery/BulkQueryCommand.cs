using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using SimitConsulta.Application.Common.Results;
using SimitConsulta.Application.Features.PlateQueries.Commands.QueryPlate;
using SimitConsulta.Application.Features.PlateQueries.DTOs;
using SimitConsulta.Domain.Enums;
using SimitConsulta.Domain.ValueObjects;

namespace SimitConsulta.Application.Features.PlateQueries.Commands.BulkQuery;

// ── Command ───────────────────────────────────────────────

/// <summary>
/// Command para consultar un lote de placas.
/// Acepta hasta 100 placas por solicitud.
/// </summary>
public record BulkQueryCommand(List<string> Plates)
    : IRequest<Result<BulkQueryDto>>;

// ── Validator ─────────────────────────────────────────────

/// <summary>
/// Valida que el lote tenga entre 1 y 100 placas
/// y que cada una tenga formato colombiano válido.
/// </summary>
public class BulkQueryValidator : AbstractValidator<BulkQueryCommand>
{
    public BulkQueryValidator()
    {
        RuleFor(x => x.Plates)
            .NotEmpty()
                .WithMessage("Debe enviar al menos una placa.")
            .Must(p => p.Count <= 100)
                .WithMessage("Máximo 100 placas por consulta masiva.");

        RuleForEach(x => x.Plates)
            .NotEmpty()
                .WithMessage("Ninguna placa puede estar vacía.")
            .Must(p => Plate.TryCreate(p, out _, out _))
                .WithMessage((_, p) =>
                    $"Formato inválido: '{p}'. Ejemplo: ABC123.");
    }
}

// ── Handler ───────────────────────────────────────────────

/// <summary>
/// Consulta un lote de placas en paralelo reutilizando QueryPlateCommand.
/// Concurrencia limitada a 3 simultáneas con SemaphoreSlim
/// para evitar rate limiting del SIMIT.
/// Una placa fallida no detiene el lote — cada error queda en BD.
/// </summary>
public class BulkQueryHandler
    : IRequestHandler<BulkQueryCommand, Result<BulkQueryDto>>
{
    private readonly IMediator _mediator;
    private readonly ILogger<BulkQueryHandler> _logger;

    private const int MaxConcurrency = 3;

    public BulkQueryHandler(
        IMediator mediator,
        ILogger<BulkQueryHandler> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<Result<BulkQueryDto>> Handle(
        BulkQueryCommand request, CancellationToken ct)
    {
        _logger.LogInformation(
            "Starting bulk query for {Total} plates",
            request.Plates.Count);

        var semaphore = new SemaphoreSlim(MaxConcurrency);

        var tasks = request.Plates.Select(async plate =>
        {
            await semaphore.WaitAsync(ct);
            try
            {
                return await _mediator.Send(
                    new QueryPlateCommand(plate, QueryType.Masiva), ct);
            }
            finally
            {
                semaphore.Release();
            }
        });

        var results = await Task.WhenAll(tasks);

        var dtos = results
            .Select(r => r.IsSuccess
                ? r.Value!
                : new PlateQueryDto(
                    0, string.Empty, DateTime.UtcNow,
                    "Error", "Masiva", 0, 0, 0, [], [], r.Error))
            .ToList();

        var successful = results.Count(r =>
            r.IsSuccess && r.Value!.Status != "Error");

        _logger.LogInformation(
            "Bulk query completed: {Ok}/{Total} successful",
            successful, request.Plates.Count);

        return Result.Ok(new BulkQueryDto(
            TotalProcessed: request.Plates.Count,
            Successful: successful,
            Failed: request.Plates.Count - successful,
            Results: dtos
        ));
    }
}