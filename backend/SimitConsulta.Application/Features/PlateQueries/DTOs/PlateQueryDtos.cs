namespace SimitConsulta.Application.Features.PlateQueries.DTOs;

/// <summary>
/// Respuesta completa de una consulta individual.
/// Incluye detalle de multas y comparendos encontrados.
/// Es el único tipo que cruza la frontera Application → API.
/// </summary>
public record PlateQueryDto(
    int Id,
    string Plate,
    DateTime ConsultedAt,
    string Status,
    string QueryType,
    int FinesCount,
    int SummonsCount,
    decimal TotalAmount,
    List<FineDto> Fines,
    List<SummonsDto> Summons,
    string? ErrorMessage
);

/// <summary>Datos de una multa en la respuesta de la API.</summary>
public record FineDto(
    string? Number,
    decimal Amount,
    string? Status,
    string? InfractionDate,
    string? Agency,
    string? Description
);

/// <summary>Datos de un comparendo en la respuesta de la API.</summary>
public record SummonsDto(
    string? Number,
    decimal Amount,
    string? Status,
    string? InfractionDate,
    string? Infraction
);

/// <summary>
/// Ítem del historial paginado.
/// No incluye detalle de multas para aligerar la respuesta.
/// </summary>
public record HistoryItemDto(
    int Id,
    string Plate,
    DateTime ConsultedAt,
    string QueryType,
    string Status,
    int FinesCount,
    string? ErrorMessage
);

/// <summary>
/// Respuesta del historial con metadatos de paginación.
/// Total permite al cliente calcular el número de páginas.
/// </summary>
public record HistoryDto(
    int Total,
    int Page,
    int PageSize,
    List<HistoryItemDto> Items
);

/// <summary>
/// Respuesta de una consulta masiva con resumen del lote
/// y detalle de cada placa procesada.
/// </summary>
public record BulkQueryDto(
    int TotalProcessed,
    int Successful,
    int Failed,
    List<PlateQueryDto> Results
);