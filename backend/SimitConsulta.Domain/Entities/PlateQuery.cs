using SimitConsulta.Domain.Enums;
using SimitConsulta.Domain.Exceptions;
using SimitConsulta.Domain.Models;
using SimitConsulta.Domain.ValueObjects;

namespace SimitConsulta.Domain.Entities;

/// <summary>
/// Aggregate Root del dominio.
/// Representa una consulta de multas realizada sobre una placa
/// vehicular en el SIMIT.
///
/// Invariantes:
/// - Siempre inicia en estado Procesando.
/// - Las colecciones son privadas — nadie agrega hijas desde afuera.
/// - El aggregate construye sus propias entidades hijas.
/// - Todo cambio de estado pasa por MarkSuccessful() o MarkFailed().
/// </summary>
public class PlateQuery
{
    public int Id { get; private set; }
    public string Plate { get; private set; } = default!;
    public DateTime ConsultedAt { get; private set; }
    public QueryType QueryType { get; private set; }
    public QueryStatus Status { get; private set; }
    public string? RawResponse { get; private set; }
    public int FinesCount { get; private set; }
    public int SummonsCount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string? ErrorMessage { get; private set; }

    private readonly List<FineDetail> _fines = new();
    private readonly List<SummonsDetail> _summons = new();

    public IReadOnlyCollection<FineDetail> Fines
        => _fines.AsReadOnly();
    public IReadOnlyCollection<SummonsDetail> Summons
        => _summons.AsReadOnly();

    private PlateQuery() { }

    /// <summary>
    /// Único punto de creación del aggregate.
    /// Garantiza estado inicial Procesando y fecha UTC.
    /// </summary>
    public static PlateQuery Create(string plate, QueryType queryType) => new()
    {
        Plate = plate,
        ConsultedAt = DateTime.UtcNow,
        QueryType = queryType,
        Status = QueryStatus.Procesando
    };

    /// <summary>
    /// Registra el resultado exitoso de la consulta.
    /// El aggregate construye sus propias hijas internamente.
    /// Regla de negocio Exitoso vs SinMultas vive aquí, no en el handler.
    /// </summary>
    public void MarkSuccessful(
        string rawResponse,
        decimal totalAmount,
        IEnumerable<SimitFine> finesData,
        IEnumerable<SimitSummons> summonsData)
    {
        var fines = finesData.Select(f =>
            FineDetail.Create(
                f.Number, f.Amount, f.Status,
                f.InfractionDate, f.Agency, f.Description))
            .ToList();

        var summons = summonsData.Select(s =>
            SummonsDetail.Create(
                s.Number, s.Amount, s.Status,
                s.InfractionDate, s.Infraction))
            .ToList();

        RawResponse = rawResponse;
        TotalAmount = totalAmount;
        FinesCount = fines.Count;
        SummonsCount = summons.Count;

        Status = (FinesCount + SummonsCount) > 0
            ? QueryStatus.Exitoso
            : QueryStatus.SinMultas;

        _fines.AddRange(fines);
        _summons.AddRange(summons);
    }

    /// <summary>
    /// Registra que la consulta falló.
    /// Siempre se persiste aunque sea con error — garantiza trazabilidad.
    /// </summary>
    public void MarkFailed(string errorMessage)
    {
        Status = QueryStatus.Error;
        ErrorMessage = errorMessage;
    }
}

