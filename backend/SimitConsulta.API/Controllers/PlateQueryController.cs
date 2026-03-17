using MediatR;
using Microsoft.AspNetCore.Mvc;
using SimitConsulta.Application.Features.PlateQueries.Commands.BulkQuery;
using SimitConsulta.Application.Features.PlateQueries.Commands.QueryPlate;
using SimitConsulta.Application.Features.PlateQueries.DTOs;
using SimitConsulta.Application.Features.PlateQueries.Queries.GetHistory;

namespace SimitConsulta.API.Controllers;

/// <summary>
/// Controller para consulta de multas vehiculares en el SIMIT.
///
/// Principio Thin Controller:
/// 1. Recibe el request HTTP.
/// 2. Construye el Command o Query.
/// 3. Envía a MediatR.
/// 4. Mapea Result&lt;T&gt; al HTTP status correcto.
///
/// Sin lógica de negocio — toda vive en los handlers.
/// </summary>
[ApiController]
[Route("api/v1")]
[Produces("application/json")]
public class PlateQueryController : ControllerBase
{
    private readonly IMediator _mediator;

    public PlateQueryController(IMediator mediator)
        => _mediator = mediator;

    /// <summary>
    /// Consulta multas y comparendos de una placa en el SIMIT.
    /// Persiste la consulta en BD con resultado o error.
    /// </summary>
    /// <param name="request">Body con la placa a consultar.</param>
    [HttpPost("query")]
    [ProducesResponseType(typeof(PlateQueryDto), 200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 502)]
    public async Task<IActionResult> QueryPlate(
        [FromBody] QueryPlateRequest request,
        CancellationToken ct)
    {
        var result = await _mediator.Send(
            new QueryPlateCommand(request.Plate), ct);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(new
            {
                error = true,
                message = result.Error
            });
    }

    /// <summary>
    /// Consulta un lote de hasta 100 placas en paralelo.
    /// Una placa fallida no detiene el lote.
    /// </summary>
    [HttpPost("query/bulk")]
    [ProducesResponseType(typeof(BulkQueryDto), 200)]
    [ProducesResponseType(typeof(object), 400)]
    public async Task<IActionResult> BulkQuery(
        [FromBody] BulkQueryRequest request,
        CancellationToken ct)
    {
        var result = await _mediator.Send(
            new BulkQueryCommand(request.Plates), ct);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(new
            {
                error = true,
                message = result.Error
            });
    }

    /// <summary>
    /// Historial paginado de consultas realizadas.
    /// Filtro opcional por placa.
    /// </summary>
    /// <param name="plate">Filtro opcional por placa (query string).</param>
    /// <param name="page">Número de página base 1. Default: 1.</param>
    /// <param name="pageSize">Registros por página máx 100. Default: 20.</param>
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
            : BadRequest(new
            {
                error = true,
                message = result.Error
            });
    }

    /// <summary>
    /// Health check — confirma que la API está en línea.
    /// Útil para load balancers y monitoreo.
    /// </summary>
    [HttpGet("health")]
    public IActionResult Health() => Ok(new
    {
        status = "ok",
        timestamp = DateTime.UtcNow,
        version = "1.0"
    });
}

// ── Request models ────────────────────────────────────────
// Records simples para recibir el body del request.
// Se mantienen aquí — son solo contenedores de entrada HTTP.

/// <summary>Body del endpoint de consulta individual.</summary>
public record QueryPlateRequest(string Plate);

/// <summary>Body del endpoint de consulta masiva.</summary>
public record BulkQueryRequest(List<string> Plates);