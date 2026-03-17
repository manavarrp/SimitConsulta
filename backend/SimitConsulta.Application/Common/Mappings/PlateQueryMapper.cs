using SimitConsulta.Application.Features.PlateQueries.DTOs;
using SimitConsulta.Domain.Entities;

namespace SimitConsulta.Application.Common.Mappings;

/// <summary>
/// Mapper manual Domain → DTO usando extension methods.
/// Sin AutoMapper — cada campo es explícito y verificado
/// por el compilador en tiempo de compilación.
/// Sintaxis natural: plateQuery.ToDto()
/// </summary>
public static class PlateQueryMapper
{
    /// <summary>
    /// Convierte el aggregate PlateQuery al DTO completo de la API.
    /// Incluye colecciones de multas y comparendos mapeadas.
    /// </summary>
    public static PlateQueryDto ToDto(this PlateQuery q) => new(
        Id: q.Id,
        Plate: q.Plate,
        ConsultedAt: q.ConsultedAt,
        Status: q.Status.ToString(),
        QueryType: q.QueryType.ToString(),
        FinesCount: q.FinesCount,
        SummonsCount: q.SummonsCount,
        TotalAmount: q.TotalAmount,
        Fines: q.Fines.Select(f => f.ToDto()).ToList(),
        Summons: q.Summons.Select(s => s.ToDto()).ToList(),
        ErrorMessage: q.ErrorMessage
    );

    /// <summary>Convierte FineDetail al DTO de multa.</summary>
    public static FineDto ToDto(this FineDetail f) => new(
        Number: f.Number,
        Amount: f.Amount,
        Status: f.Status,
        InfractionDate: f.InfractionDate,
        Agency: f.Agency,
        Description: f.Description
    );

    /// <summary>Convierte SummonsDetail al DTO de comparendo.</summary>
    public static SummonsDto ToDto(this SummonsDetail s) => new(
        Number: s.Number,
        Amount: s.Amount,
        Status: s.Status,
        InfractionDate: s.InfractionDate,
        Infraction: s.Infraction
    );

    /// <summary>
    /// Convierte a DTO simplificado para el historial.
    /// No incluye detalle de multas para aligerar la respuesta.
    /// </summary>
    public static HistoryItemDto ToHistoryItem(this PlateQuery q) => new(
        Id: q.Id,
        Plate: q.Plate,
        ConsultedAt: q.ConsultedAt,
        QueryType: q.QueryType.ToString(),
        Status: q.Status.ToString(),
        FinesCount: q.FinesCount,
        ErrorMessage: q.ErrorMessage
    );
}