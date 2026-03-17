namespace SimitConsulta.Domain.Entities;

/// <summary>
/// Entidad hija del aggregate PlateQuery.
/// Representa el detalle de una multa encontrada en el SIMIT.
/// Solo se crea desde PlateQuery.MarkSuccessful().
/// Constructor privado garantiza que no existe un FineDetail
/// huérfano fuera del aggregate.
/// </summary>
public class FineDetail
{
    public int Id { get; private set; }
    public int PlateQueryId { get; private set; }
    public string? Number { get; private set; }
    public decimal Amount { get; private set; }
    public string? Status { get; private set; }
    public string? InfractionDate { get; private set; }
    public string? Agency { get; private set; }
    public string? Description { get; private set; }

    private FineDetail() { }

    public static FineDetail Create(
        string? number,
        decimal amount,
        string? status,
        string? infractionDate,
        string? agency,
        string? description) => new()
        {
            Number = number,
            Amount = amount,
            Status = status,
            InfractionDate = infractionDate,
            Agency = agency,
            Description = description
        };
}