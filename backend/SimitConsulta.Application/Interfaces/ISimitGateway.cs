using SimitConsulta.Domain.Models;

namespace SimitConsulta.Application.Interfaces;

/// <summary>
/// Contrato del cliente externo al SIMIT.
/// El captchaToken es resuelto por el frontend (navegador real)
/// y enviado al backend para evitar el TLS fingerprinting del servidor
/// del captcha qxcaptcha.fcm.org.co.
/// </summary>
public interface ISimitGateway
{
    /// <summary>
    /// Consulta multas y comparendos de una placa en el SIMIT.
    /// </summary>
    /// <param name="plate">Placa normalizada formato ABC123 o ABC12D.</param>
    /// <param name="captchaToken">
    /// Token PoW resuelto por el frontend.
    /// Formato: JSON serializado como string — igual que reCaptchaDTO.response.
    /// Ejemplo: "[{\"question\":\"abc...\",\"time\":1234,\"nonce\":7}]"
    /// </param>
    Task<SimitResponse> QueryPlateAsync(
        string plate,
        string captchaToken,
        CancellationToken ct = default);
}