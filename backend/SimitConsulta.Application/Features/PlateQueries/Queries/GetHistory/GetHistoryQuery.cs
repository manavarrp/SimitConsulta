using FluentValidation;
using MediatR;
using SimitConsulta.Application.Common.Mappings;
using SimitConsulta.Application.Common.Results;
using SimitConsulta.Application.Features.PlateQueries.DTOs;
using SimitConsulta.Application.Interfaces;

namespace SimitConsulta.Application.Features.PlateQueries.Queries.GetHistory;

// ── Query ─────────────────────────────────────────────────

/// <summary>
/// Query CQRS para el historial paginado de consultas.
/// Las queries nunca modifican estado — solo leen.
/// </summary>
public record GetHistoryQuery(
    string? Plate = null,
    int Page = 1,
    int PageSize = 20)
    : IRequest<Result<HistoryDto>>;

// ── Validator ─────────────────────────────────────────────

/// <summary>Valida los parámetros de paginación.</summary>
public class GetHistoryValidator : AbstractValidator<GetHistoryQuery>
{
    public GetHistoryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0)
                .WithMessage("La página debe ser mayor a 0.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
                .WithMessage(
                    "El tamaño de página debe estar entre 1 y 100.");
    }
}

// ── Handler ───────────────────────────────────────────────

/// <summary>
/// Recupera el historial paginado de consultas.
/// AsNoTracking en el repositorio para mejor rendimiento
/// — el historial no se modifica.
/// </summary>
public class GetHistoryHandler
    : IRequestHandler<GetHistoryQuery, Result<HistoryDto>>
{
    private readonly IPlateQueryRepository _repo;

    public GetHistoryHandler(IPlateQueryRepository repo)
        => _repo = repo;

    public async Task<Result<HistoryDto>> Handle(
        GetHistoryQuery request, CancellationToken ct)
    {
        var total = await _repo.CountAsync(request.Plate, ct);

        var items = await _repo.GetHistoryAsync(
            request.Plate, request.Page, request.PageSize, ct);

        return Result.Ok(new HistoryDto(
            Total: total,
            Page: request.Page,
            PageSize: request.PageSize,
            Items: items.Select(q => q.ToHistoryItem()).ToList()
        ));
    }
}