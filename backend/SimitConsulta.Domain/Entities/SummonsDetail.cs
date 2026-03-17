namespace SimitConsulta.Domain.Entities;

/// <summary>
/// Entidad hija del aggregate PlateQuery.
/// Representa el detalle de un comparendo encontrado en el SIMIT.
/// "Summons": orden oficial de comparecencia ante autoridad de tránsito.
/// Solo se crea desde PlateQuery.MarkSuccessful().
/// </summary>
public class SummonsDetail
{
    public int Id { get; private set; }
    public int PlateQueryId { get; private set; }
    public string? Number { get; private set; }
    public decimal Amount { get; private set; }
    public string? Status { get; private set; }
    public string? InfractionDate { get; private set; }
    public string? Infraction { get; private set; }

    private SummonsDetail() { }

    public static SummonsDetail Create(
        string? number,
        decimal amount,
        string? status,
        string? infractionDate,
        string? infraction) => new()
        {
            Number = number,
            Amount = amount,
            Status = status,
            InfractionDate = infractionDate,
            Infraction = infraction
        };
}