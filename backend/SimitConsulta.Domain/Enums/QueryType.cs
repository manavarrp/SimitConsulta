namespace SimitConsulta.Domain.Enums
{
    /// <summary>
    /// Distingue si la Consulta fue iniciada individualmente
    /// o como parte de un lote masivo. Se guarda para trazabilidad.
    /// </summary>
    public enum QueryType
    {
        /// <summary>Una sola placa via POST /api/v1/consulta.</summary>
        Individual,

        /// <summary>Dentro de un lote via POST /api/v1/consulta-masiva.</summary>
        Masiva
    }
}
