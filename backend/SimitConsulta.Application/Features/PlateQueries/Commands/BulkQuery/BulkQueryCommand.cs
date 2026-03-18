// ── Command ───────────────────────────────────────────────

using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using SimitConsulta.Application.Common.Results;
using SimitConsulta.Application.Features.PlateQueries.Commands.QueryPlate;
using SimitConsulta.Application.Features.PlateQueries.DTOs;
using SimitConsulta.Domain.Enums;
using SimitConsulta.Domain.ValueObjects;

namespace SimitConsulta.Application.Features.PlateQueries.Commands.BulkQuery;
/// <summary>
/// Command para consultar un lote de placas.
/// El frontend resuelve un único token captcha que se usa
/// para todas las placas del lote.
/// </summary>
public record BulkQueryCommand(
    List<string> Plates,
    string CaptchaToken)
    : IRequest<Result<BulkQueryDto>>;

// ── Validator ─────────────────────────────────────────────

public class BulkQueryValidator : AbstractValidator<BulkQueryCommand>
{
    public BulkQueryValidator()
    {
        RuleFor(x => x.Plates)
            .NotEmpty()
                .WithMessage("Debe enviar al menos una placa.")
            .Must(p => p.Count <= 100)
                .WithMessage("Máximo 100 placas por consulta masiva.");

        RuleFor(x => x.CaptchaToken)
            .NotEmpty()
                .WithMessage("El token del captcha es obligatorio.");

        RuleForEach(x => x.Plates)
            .NotEmpty()
                .WithMessage("Ninguna placa puede estar vacía.")
            .Must(p => Plate.TryCreate(p, out _, out _))
                .WithMessage((_, p) =>
                    $"Formato inválido: '{p}'. Ejemplo: ABC123.");
    }
}

// ── Handler ───────────────────────────────────────────────

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
                // Reutiliza el mismo token para todas las placas del lote
                return await _mediator.Send(
                    new QueryPlateCommand(
                        plate,
                        request.CaptchaToken,   // ← mismo token
                        QueryType.Masiva), ct);
            }
            finally
            {
                semaphore.Release();
            }
        });

        var results = new List<Result<PlateQueryDto>>();
        foreach (var task in tasks)
        {
            results.Add(await task);
        }
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