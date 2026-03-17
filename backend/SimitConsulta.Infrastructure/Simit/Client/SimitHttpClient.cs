using System.Text;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using SimitConsulta.Application.Interfaces;
using SimitConsulta.Domain.Exceptions;
using SimitConsulta.Domain.Models;
using SimitConsulta.Infrastructure.Simit.Captcha;
using SimitConsulta.Infrastructure.Utils;

namespace SimitConsulta.Infrastructure.Simit.Client;

/// <summary>
/// Implementación de ISimitGateway que realiza llamadas HTTP al SIMIT.
///
/// Flujo interno:
/// 1. Obtener token captcha PoW via ICaptchaSolver.
/// 2. Construir body con placa y token.
/// 3. POST consultasimit.fcm.org.co/.../estadocuenta/consulta.
/// 4. Deserializar respuesta a DTOs privados.
/// 5. MapToDomain() → tipos de dominio puros (SimitResponse).
///
/// Los DTOs HTTP son private sealed — no se filtran fuera.
/// Application solo ve SimitResponse, nunca los detalles del JSON.
/// </summary>
public class SimitHttpClient : ISimitGateway
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly ICaptchaSolver _captcha;
    private readonly ILogger<SimitHttpClient> _logger;

    private const string QueryUrl =
        "https://consultasimit.fcm.org.co/simit/microservices/" +
        "estado-cuenta-simit/estadocuenta/consulta";

    public SimitHttpClient(
        IHttpClientFactory httpFactory,
        ICaptchaSolver captcha,
        ILogger<SimitHttpClient> logger)
    {
        _httpFactory = httpFactory;
        _captcha = captcha;
        _logger = logger;
    }

    public async Task<SimitResponse> QueryPlateAsync(
        string plate, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "Querying SIMIT for plate {Plate}", plate);

        // Paso 1: obtener token PoW — prerrequisito obligatorio
        var captchaToken = await _captcha.GetTokenAsync(ct);

        // Paso 2: construir body exacto que espera el SIMIT
        var body = JsonHelper.Serialize(new
        {
            filtro = plate,
            reCaptchaDTO = new
            {
                consumidor = "1",
                response = captchaToken
            }
        });

        var client = _httpFactory.CreateClient("simit");
        var content = new StringContent(
            body, Encoding.UTF8, "application/json");

        // Paso 3: llamar al SIMIT
        HttpResponseMessage response;
        try
        {
            response = await client.PostAsync(QueryUrl, content, ct);
        }
        catch (HttpRequestException ex)
        {
            throw new SimitGatewayException(
                $"Could not connect to SIMIT: {ex.Message}");
        }

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(ct);
            _logger.LogWarning(
                "SIMIT responded {Status}: {Body}",
                response.StatusCode, errorBody);

            throw new SimitGatewayException(
                $"SIMIT returned {(int)response.StatusCode}: {errorBody}",
                (int)response.StatusCode);
        }

        // Paso 4: deserializar a DTOs privados
        var json = await response.Content.ReadAsStringAsync(ct);
        _logger.LogDebug(
            "SIMIT response for {Plate}: {Json}", plate, json);

        if (!JsonHelper.TryDeserialize<SimitResponseDto>(json, out var dto)
            || dto is null)
            throw new SimitGatewayException(
                "Could not deserialize SIMIT response.");

        // Paso 5: traducir a tipos de dominio puros
        return MapToDomain(dto, json);
    }

    /// <summary>
    /// Traduce el DTO HTTP privado a SimitResponse de dominio.
    /// Después de este método el resto de la app no sabe nada de JSON.
    /// rawJson se guarda en RawResponse para auditoría.
    /// </summary>
    private static SimitResponse MapToDomain(
        SimitResponseDto dto, string rawJson) => new(
        Fines: (dto.Fines ?? []).Select(f => new SimitFine(
            f.FormNumber, f.AmountPaid ?? 0, f.Status,
            f.InfractionDate, f.Agency, f.Description)).ToList(),
        Summons: (dto.Summons ?? []).Select(s => new SimitSummons(
            s.SummonsNumber, s.AmountPaid ?? 0, s.Status,
            s.InfractionDate, s.Infraction)).ToList(),
        TotalAmount: dto.TotalGeneral,
        ClearedOfDebts: dto.PazSalvo,
        Cancelled: dto.Cancelada,
        Suspended: dto.Suspendida,
        RawResponse: rawJson
    );

    // ── DTOs privados del SIMIT ───────────────────────────
    // JsonPropertyName usa los nombres exactos del JSON del SIMIT.
    // Son private sealed — detalles de implementación que no salen.

    private sealed class SimitResponseDto
    {
        [JsonPropertyName("multas")]
        public List<SimitFineDto>? Fines { get; set; }

        [JsonPropertyName("comparendos")]
        public List<SimitSummonsDto>? Summons { get; set; }

        [JsonPropertyName("totalGeneral")]
        public decimal TotalGeneral { get; set; }

        [JsonPropertyName("pazSalvo")]
        public bool PazSalvo { get; set; }

        [JsonPropertyName("cancelada")]
        public bool Cancelada { get; set; }

        [JsonPropertyName("suspendida")]
        public bool Suspendida { get; set; }
    }

    private sealed class SimitFineDto
    {
        [JsonPropertyName("numeroFormulario")]
        public string? FormNumber { get; set; }

        [JsonPropertyName("valorCancelado")]
        public decimal? AmountPaid { get; set; }

        [JsonPropertyName("estado")]
        public string? Status { get; set; }

        [JsonPropertyName("fechaInfraccion")]
        public string? InfractionDate { get; set; }

        [JsonPropertyName("organismoTransito")]
        public string? Agency { get; set; }

        [JsonPropertyName("descripcion")]
        public string? Description { get; set; }
    }

    private sealed class SimitSummonsDto
    {
        [JsonPropertyName("numeroComparendo")]
        public string? SummonsNumber { get; set; }

        [JsonPropertyName("valorCancelado")]
        public decimal? AmountPaid { get; set; }

        [JsonPropertyName("estado")]
        public string? Status { get; set; }

        [JsonPropertyName("fechaInfraccion")]
        public string? InfractionDate { get; set; }

        [JsonPropertyName("infraccion")]
        public string? Infraction { get; set; }
    }
}