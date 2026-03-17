using System.Text;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using SimitConsulta.Infrastructure.Utils;

namespace SimitConsulta.Infrastructure.Simit.Captcha;

/// <summary>
/// Contrato interno del solver de captcha.
/// Permite mockearlo en tests unitarios de SimitHttpClient.
/// </summary>
public interface ICaptchaSolver
{
    /// <summary>
    /// Obtiene un token PoW válido para el body del SIMIT.
    /// Obtiene el challenge y resuelve el Proof-of-Work.
    /// </summary>
    Task<string> GetTokenAsync(CancellationToken ct = default);
}

/// <summary>
/// Implementación del solver del captcha Proof-of-Work
/// de qxcaptcha.fcm.org.co.
///
/// El captcha del SIMIT NO es visual — es computacional:
/// el servidor emite un question (hash MD5) y el cliente
/// debe encontrar un nonce tal que MD5(question+nonce)
/// empiece con N ceros. Se resuelve en el servidor sin
/// intervención humana.
///
/// Flujo:
/// 1. POST qxcaptcha.fcm.org.co/api.php
///    → recibe { question, recommended_difficulty }
/// 2. HashHelper.SolvePoW() → encuentra el nonce
/// 3. Serializa [{question, time, nonce}] como string JSON
/// </summary>
public class CaptchaSolver : ICaptchaSolver
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly ILogger<CaptchaSolver> _logger;

    private const string CaptchaUrl =
        "https://qxcaptcha.fcm.org.co/api.php";

    public CaptchaSolver(
        IHttpClientFactory httpFactory,
        ILogger<CaptchaSolver> logger)
    {
        _httpFactory = httpFactory;
        _logger = logger;
    }

    public async Task<string> GetTokenAsync(CancellationToken ct = default)
    {
        // Paso 1: obtener challenge del servidor de captcha
        var challenge = await GetChallengeAsync(ct);

        _logger.LogDebug(
            "Challenge received: {Question}, difficulty: {Difficulty}",
            challenge.Question, challenge.RecommendedDifficulty);

        // Paso 2: resolver Proof-of-Work
        var nonce = HashHelper.SolvePoW(
            challenge.Question,
            challenge.RecommendedDifficulty);

        _logger.LogDebug("PoW solved, nonce: {Nonce}", nonce);

        // Paso 3: construir token en el formato exacto del SIMIT
        var time = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var tokenArray = new[]
        {
            new { question = challenge.Question, time, nonce }
        };

        return JsonHelper.Serialize(tokenArray);
    }

    private async Task<CaptchaChallenge> GetChallengeAsync(
        CancellationToken ct)
    {
        var client = _httpFactory.CreateClient("simit");
        var body = JsonHelper.Serialize(new { consumidor = "1" });
        var content = new StringContent(body, Encoding.UTF8, "application/json");

        var response = await client.PostAsync(CaptchaUrl, content, ct);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(ct);

        var apiResponse = JsonHelper.Deserialize<CaptchaApiResponse>(json)
            ?? throw new InvalidOperationException(
                "Empty or invalid captcha server response.");

        if (apiResponse.HasError || apiResponse.Data is null)
            throw new InvalidOperationException(
                $"Captcha API error: {json}");

        return apiResponse.Data;
    }

    // ── DTOs internos — no salen de esta clase ────────────

    private sealed class CaptchaApiResponse
    {
        [JsonPropertyName("error")]
        public bool HasError { get; set; }

        [JsonPropertyName("data")]
        public CaptchaChallenge? Data { get; set; }
    }

    private sealed class CaptchaChallenge
    {
        [JsonPropertyName("question")]
        public string Question { get; set; } = string.Empty;

        [JsonPropertyName("recommended_difficulty")]
        public int RecommendedDifficulty { get; set; }
    }
}