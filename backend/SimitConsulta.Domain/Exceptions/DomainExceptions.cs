namespace SimitConsulta.Domain.Exceptions
{
    /// <summary>
    /// Excepción base para cualquier violación de reglas de negocio.
    /// El middleware la captura y la convierte en HTTP 400.
    /// </summary>
    public class DomainException : Exception
    {
        public DomainException(string message) : base(message) { }
    }

    /// <summary>
    /// Se lanza cuando se busca una Consulta por ID y no existe.
    /// </summary>
    public class QueryNotFoundException : DomainException
    {
        public QueryNotFoundException(int id) : base($"No se encontró la consulta con id {id}.") { }
        
    }

    /// <summary>
    /// Se lanza cuando el SIMIT no responde o devuelve error.
    /// El middleware la convierte en HTTP 502.
    /// Guarda el HTTP status original para diagnóstico.
    /// </summary>
    public class SimitGatewayException : DomainException
    {
        /// <summary>Código HTTP devuelto por el SIMIT, si aplica.</summary>
        public int? StatusCode { get; }

        public SimitGatewayException(string message, int? statusCode = null)
            : base(message) => StatusCode = statusCode;
    }

}
