using MediatR;
using Microsoft.AspNetCore.Mvc;
using SimitConsulta.Application.Features.PlateQueries.Commands.BulkQuery;
using SimitConsulta.Application.Features.PlateQueries.Commands.QueryPlate;
using SimitConsulta.Application.Features.PlateQueries.DTOs;
using SimitConsulta.Application.Features.PlateQueries.Queries.GetHistory;

namespace SimitConsulta.API.Controllers;

/// <summary>
/// Controller para consulta de multas vehiculares en el SIMIT.
/// Thin controller — solo delega a MediatR.
/// </summary>
[ApiController]
[Route("api/v1")]
[Produces("application/json")]
public class PlateQueryController : ControllerBase
{
    private readonly IMediator _mediator;

    public PlateQueryController(IMediator mediator)
        => _mediator = mediator;

    [HttpPost("query")]
    [ProducesResponseType(typeof(PlateQueryDto), 200)]
    [ProducesResponseType(typeof(object), 400)]
    public async Task<IActionResult> QueryPlate(
        [FromBody] QueryPlateRequest request,
        CancellationToken ct)
    {
        var result = await _mediator.Send(
            new QueryPlateCommand(
                request.Plate,
                request.CaptchaToken), ct);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(new { error = true, message = result.Error });
    }

    [HttpPost("query/bulk")]
    [ProducesResponseType(typeof(BulkQueryDto), 200)]
    [ProducesResponseType(typeof(object), 400)]
    public async Task<IActionResult> BulkQuery(
        [FromBody] BulkQueryRequest request,
        CancellationToken ct)
    {
        var result = await _mediator.Send(
            new BulkQueryCommand(
                request.Plates,
                request.CaptchaToken), ct);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(new { error = true, message = result.Error });
    }

    [HttpGet("history")]
    [ProducesResponseType(typeof(HistoryDto), 200)]
    [ProducesResponseType(typeof(object), 400)]
    public async Task<IActionResult> GetHistory(
        [FromQuery] string? plate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        if (pageSize > 100) pageSize = 100;

        var result = await _mediator.Send(
            new GetHistoryQuery(plate, page, pageSize), ct);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(new { error = true, message = result.Error });
    }

    [HttpGet("health")]
    public IActionResult Health() => Ok(new
    {
        status = "ok",
        timestamp = DateTime.UtcNow,
        version = "1.0"
    });
}

// ── Request models ────────────────────────────────────────
// Records simples para deserializar el body del request HTTP.
// Se mantienen en este archivo — son contenedores de entrada,
// no lógica de negocio.

/// <summary>
/// Body del endpoint POST /api/v1/query.
/// El frontend resuelve el captcha PoW y envía el token
/// junto con la placa a consultar.
/// </summary>
public record QueryPlateRequest(
    string Plate,
    string CaptchaToken);

/// <summary>
/// Body del endpoint POST /api/v1/query/bulk.
/// Un solo token captcha para todo el lote.
/// </summary>
public record BulkQueryRequest(
    List<string> Plates,
    string CaptchaToken);