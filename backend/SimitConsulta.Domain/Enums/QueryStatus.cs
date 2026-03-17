namespace SimitConsulta.Domain.Enums
{
    /// <summary>
    /// Define los posibles estados del ciclo de vida de una Consulta.
    /// Se persiste como string en BD con HasConversion para que sea
    /// legible sin conocer los valores numéricos del enum.
    /// </summary>
    public enum QueryStatus
    {
        /// <summary>Registro creado. Aún no se ha llamado al SIMIT.</summary>
        Procesando,

        /// <summary>SIMIT respondió y encontró multas o comparendos.</summary>
        Exitoso,

        /// <summary>SIMIT respondió correctamente pero no hay registros.</summary>
        SinMultas,

        /// <summary>Falló la llamada al SIMIT o la placa era inválida.</summary>
        Error
    }
}
