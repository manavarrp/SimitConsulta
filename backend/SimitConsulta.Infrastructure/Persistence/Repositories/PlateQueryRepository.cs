using Microsoft.EntityFrameworkCore;
using SimitConsulta.Application.Interfaces;
using SimitConsulta.Domain.Entities;

namespace SimitConsulta.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementación de IPlateQueryRepository con EF Core.
///
/// Reglas de rendimiento:
/// - AsNoTracking() en lecturas — EF no rastrea objetos en memoria.
/// - Include() con hijas solo cuando se necesitan.
/// - Skip/Take antes de ToListAsync — paginación en SQL no en memoria.
/// </summary>
public class PlateQueryRepository : IPlateQueryRepository
{
    private readonly AppDbContext _db;

    public PlateQueryRepository(AppDbContext db) => _db = db;

    /// <summary>
    /// Persiste PlateQuery en estado Procesando.
    /// Se llama antes del SIMIT para garantizar trazabilidad.
    /// </summary>
    public async Task<PlateQuery> AddAsync(
        PlateQuery query, CancellationToken ct = default)
    {
        _db.PlateQueries.Add(query);
        await _db.SaveChangesAsync(ct);
        return query;
    }

    /// <summary>
    /// Actualiza con resultado final (éxito o error).
    /// EF detecta cambios del aggregate y genera el UPDATE.
    /// </summary>
    public async Task UpdateAsync(
        PlateQuery query, CancellationToken ct = default)
    {
        _db.PlateQueries.Update(query);
        await _db.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Recupera por ID incluyendo hijas.
    /// Usa tracking — podría modificarse después.
    /// </summary>
    public async Task<PlateQuery?> GetByIdAsync(
        int id, CancellationToken ct = default) =>
        await _db.PlateQueries
            .Include(q => q.Fines)
            .Include(q => q.Summons)
            .FirstOrDefaultAsync(q => q.Id == id, ct);

    /// <summary>
    /// Historial paginado con AsNoTracking — solo lectura.
    /// Ordena por fecha descendente (más reciente primero).
    /// </summary>
    public async Task<IReadOnlyList<PlateQuery>> GetHistoryAsync(
        string? plate, int page, int pageSize,
        CancellationToken ct = default)
    {
        var query = _db.PlateQueries.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(plate))
            query = query.Where(q =>
                q.Plate == plate.ToUpperInvariant());

        return await query
            .OrderByDescending(q => q.ConsultedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    /// <summary>
    /// Cuenta total con el mismo filtro que GetHistoryAsync.
    /// Necesario para calcular páginas en el cliente.
    /// </summary>
    public async Task<int> CountAsync(
        string? plate, CancellationToken ct = default)
    {
        var query = _db.PlateQueries.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(plate))
            query = query.Where(q =>
                q.Plate == plate.ToUpperInvariant());

        return await query.CountAsync(ct);
    }
}