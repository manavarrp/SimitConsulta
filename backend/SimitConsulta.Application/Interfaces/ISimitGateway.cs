using SimitConsulta.Domain.Models;

namespace SimitConsulta.Application.Interfaces;

/// <summary>
/// Contrato del cliente externo al SIMIT.
/// Vive en Application porque es un servicio orquestado
/// por los casos de uso.
/// Infrastructure lo implementa con HttpClient y captcha PoW.
/// Permite mockear en tests sin llamadas HTTP reales.
/// </summary>
public interface ISimitGateway
{
    /// <summary>
    /// Consulta multas y comparendos de una placa en el SIMIT.
    /// Resuelve el captcha PoW y traduce la respuesta a tipos de dominio.
    /// </summary>
    /// <param name="plate">Placa normalizada formato ABC123 o ABC12D.</param>
    /// <exception cref="Domain.Exceptions.SimitGatewayException">
    /// Si el SIMIT no responde o devuelve error HTTP.
    /// </exception>
    Task<SimitResponse> QueryPlateAsync(
        string plate, CancellationToken ct = default);
}