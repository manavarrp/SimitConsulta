namespace SimitConsulta.Domain.Models
{
    /// <summary>
    /// Datos de una multa devuelta por el SIMIT.
    /// Término conservado en español — término legal colombiano.
    /// </summary>
    public record SimitFine(
        string? Number,
        decimal Amount,
        string? Status,
        string? InfractionDate,
        string? Agency,
        string? Description
    );
}
