namespace SimitConsulta.Domain.Models
{
    /// <summary>
    /// Datos de un comparendo devuelto por el SIMIT.
    /// Término jurídico colombiano sin equivalente exacto en inglés.
    /// </summary>
    public record SimitSummons(
        string? Number,
        decimal Amount,
        string? Status,
        string? InfractionDate,
        string? Infraction
    );
}
