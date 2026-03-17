namespace SimitConsulta.Domain.Models
{
    /// <summary>
    /// Respuesta del SIMIT traducida a tipos de dominio puros.
    /// Infrastructure la crea. Application la consume.
    /// Sin atributos de serialización — son datos del negocio.
    /// RawResponse se persiste para auditoría.
    /// </summary>
    public record SimitResponse(
        List<SimitFine> Fines,
        List<SimitSummons> Summons,
        decimal TotalAmount,
        bool ClearedOfDebts,
        bool Cancelled,
        bool Suspended,
        string RawResponse
    );
}
