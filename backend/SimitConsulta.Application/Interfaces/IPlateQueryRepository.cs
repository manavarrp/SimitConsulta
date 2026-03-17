using SimitConsulta.Domain.Entities;

namespace SimitConsulta.Application.Interfaces;

/// <summary>
/// Contrato de persistencia para el aggregate PlateQuery.
/// Vive en Application porque sus métodos evolucionan
/// junto a los casos de uso.
/// Infrastructure lo implementa con EF Core.
/// </summary>
public interface IPlateQueryRepository
{
    /// <summary>
    /// Persiste una nueva PlateQuery en BD con estado Procesando.
    /// Se llama antes de consultar el SIMIT para garantizar
    /// trazabilidad aunque el proceso falle.
    /// </summary>
    Task<PlateQuery> AddAsync(
        PlateQuery query, CancellationToken ct = default);

    /// <summary>
    /// Actualiza una PlateQuery con el resultado final.
    /// Se llama después de recibir respuesta del SIMIT.
    /// </summary>
    Task UpdateAsync(
        PlateQuery query, CancellationToken ct = default);

    /// <summary>
    /// Recupera una PlateQuery por ID con sus hijas incluidas.
    /// Retorna null si no existe.
    /// </summary>
    Task<PlateQuery?> GetByIdAsync(
        int id, CancellationToken ct = default);

    /// <summary>
    /// Historial paginado ordenado por fecha descendente.
    /// Filtra por placa si se provee.
    /// </summary>
    Task<IReadOnlyList<PlateQuery>> GetHistoryAsync(
        string? plate, int page, int pageSize,
        CancellationToken ct = default);

    /// <summary>
    /// Cuenta el total para calcular páginas en el cliente.
    /// Aplica el mismo filtro que GetHistoryAsync.
    /// </summary>
    Task<int> CountAsync(
        string? plate, CancellationToken ct = default);
}