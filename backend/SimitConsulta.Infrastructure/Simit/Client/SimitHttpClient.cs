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
/// Implementación de ISimitGateway con llamadas HTTP al SIMIT.
///
/// Nota importante sobre la estructura del SIMIT:
/// El API devuelve TODOS los registros en el array "multas".
/// El campo comparendo:true indica que es un comparendo de tránsito.
/// El array "comparendos" siempre llega vacío.
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

        var captchaToken = await _captcha.GetTokenAsync(ct);

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

        var json = await response.Content.ReadAsStringAsync(ct);

        _logger.LogDebug(
            "SIMIT response for {Plate}: {Length} chars",
            plate, json.Length);

        if (!JsonHelper.TryDeserialize<SimitResponseDto>(json, out var dto)
            || dto is null)
            throw new SimitGatewayException(
                "Could not deserialize SIMIT response.");

        return MapToDomain(dto, json);
    }

    /// <summary>
    /// Traduce la respuesta del SIMIT a tipos de dominio.
    /// El SIMIT mezcla multas y comparendos en el array "multas".
    /// Los separamos por el campo comparendo:true/false.
    /// </summary>
    private SimitResponse MapToDomain(
        SimitResponseDto dto, string rawJson)
    {
        var allRecords = dto.Multas ?? [];

        // Separar por tipo — comparendo:false → multa, comparendo:true → comparendo
        var fines = allRecords
            .Where(m => !m.EsComparendo)
            .Select(m => new SimitFine(
                Number: m.NumeroComparendo,
                Amount: m.ValorPagar,
                Status: m.EstadoComparendo,
                InfractionDate: m.FechaComparendo,
                Agency: m.OrganismoTransito,
                Description: m.Infracciones?
                    .FirstOrDefault()?.DescripcionInfraccion))
            .ToList();

        var summons = allRecords
            .Where(m => m.EsComparendo)
            .Select(m => new SimitSummons(
                Number: m.NumeroComparendo,
                Amount: m.ValorPagar,
                Status: m.EstadoComparendo,
                InfractionDate: m.FechaComparendo,
                Infraction: m.Infracciones?
                    .FirstOrDefault()?.CodigoInfraccion))
            .ToList();

        // Total real = suma de valorPagar de todos los registros
        var total = allRecords.Sum(m => m.ValorPagar);

       _logger.LogInformation(
            "SIMIT parsed: {Fines} fines, {Summons} summons, total {Total}",
            fines.Count, summons.Count, total);

        return new SimitResponse(
            Fines: fines,
            Summons: summons,
            TotalAmount: total,
            ClearedOfDebts: dto.PazSalvo,
            Cancelled: dto.Cancelada,
            Suspended: dto.Suspendida,
            RawResponse: rawJson
        );
    }

    // ── DTOs privados ─────────────────────────────────────

    private sealed class SimitResponseDto
    {
        [JsonPropertyName("multas")]
        public List<SimitMultaDto>? Multas { get; set; }

        [JsonPropertyName("pazSalvo")]
        public bool PazSalvo { get; set; }

        [JsonPropertyName("cancelada")]
        public bool Cancelada { get; set; }

        [JsonPropertyName("suspendida")]
        public bool Suspendida { get; set; }
    }

    private sealed class SimitMultaDto
    {
        /// <summary>true = comparendo de tránsito, false = multa.</summary>
        [JsonPropertyName("comparendo")]
        public bool EsComparendo { get; set; }

        [JsonPropertyName("numeroComparendo")]
        public string? NumeroComparendo { get; set; }

        /// <summary>Valor total a pagar incluyendo intereses.</summary>
        [JsonPropertyName("valorPagar")]
        public decimal ValorPagar { get; set; }

        [JsonPropertyName("estadoComparendo")]
        public string? EstadoComparendo { get; set; }

        [JsonPropertyName("fechaComparendo")]
        public string? FechaComparendo { get; set; }

        [JsonPropertyName("organismoTransito")]
        public string? OrganismoTransito { get; set; }

        [JsonPropertyName("infracciones")]
        public List<InfraccionDto>? Infracciones { get; set; }
    }

    private sealed class InfraccionDto
    {
        [JsonPropertyName("codigoInfraccion")]
        public string? CodigoInfraccion { get; set; }

        [JsonPropertyName("descripcionInfraccion")]
        public string? DescripcionInfraccion { get; set; }

        [JsonPropertyName("valorInfraccion")]
        public decimal ValorInfraccion { get; set; }
    }
}