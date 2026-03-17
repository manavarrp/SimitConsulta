using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using SimitConsulta.Infrastructure.Utils;

namespace SimitConsulta.Infrastructure.Simit.Captcha;

public interface ICaptchaSolver
{
    Task<string> GetTokenAsync(CancellationToken ct = default);
}

/// <summary>
/// Resuelve el captcha Proof-of-Work de qxcaptcha.fcm.org.co.
/// El servidor espera FormData con endpoint=question.
/// Devuelve { error: false/0, data: { question, recommended_difficulty } }
/// </summary>
public class CaptchaSolver : ICaptchaSolver
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly ILogger<CaptchaSolver> _logger;

    private const string CaptchaUrl = "https://qxcaptcha.fcm.org.co/api.php";
    private const int DefaultDifficulty = 2;

    public CaptchaSolver(
        IHttpClientFactory httpFactory,
        ILogger<CaptchaSolver> logger)
    {
        _httpFactory = httpFactory;
        _logger = logger;
    }

    public async Task<string> GetTokenAsync(
     CancellationToken ct = default)
    {
        var challenge = await GetChallengeAsync(ct);

        var difficulty = challenge.RecommendedDifficulty > 0
            ? challenge.RecommendedDifficulty
            : 2;

        // Tiempo fijo antes de resolver — igual que el navegador
        var time = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        _logger.LogDebug(
            "Challenge: {Question}, difficulty: {Difficulty}, time: {Time}",
            challenge.Question, difficulty, time);

        // Resolver en C# — mucho más rápido que JavaScript/PowerShell
        var token = HashHelper.SolvePoWAndBuildToken(
            challenge.Question, time, difficulty);

        _logger.LogDebug("Token built: {Token}", token);

        return token;
    }
    private async Task<CaptchaChallenge> GetChallengeAsync(
      CancellationToken ct)
    {
        var client = _httpFactory.CreateClient("simit");

        // application/x-www-form-urlencoded — igual que Invoke-WebRequest
        // MultipartFormDataContent no funciona con este servidor
        var formData = new FormUrlEncodedContent(new[]
        {
        new KeyValuePair<string, string>("endpoint", "question")
    });

        var response = await client.PostAsync(CaptchaUrl, formData, ct);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(ct);

        _logger.LogDebug("Captcha raw response: {Json}", json);

        var apiResponse = JsonHelper.Deserialize<CaptchaApiResponse>(json)
            ?? throw new InvalidOperationException(
                "Empty or invalid captcha server response.");

        if (apiResponse.HasError || apiResponse.Data is null)
            throw new InvalidOperationException(
                $"Captcha API error: {json}");

        return apiResponse.Data;
    }
    // ── DTOs internos ─────────────────────────────────────

    private sealed class CaptchaApiResponse
    {
        /// <summary>
        /// El servidor puede devolver bool (false/true) o int (0/1).
        /// JsonElement maneja ambos formatos.
        /// </summary>
        [JsonPropertyName("error")]
        public JsonElement Error { get; set; }

        [JsonPropertyName("data")]
        public CaptchaChallenge? Data { get; set; }

        public bool HasError =>
            Error.ValueKind == JsonValueKind.True ||
            (Error.ValueKind == JsonValueKind.Number &&
             Error.GetInt32() != 0);
    }

    private sealed class CaptchaChallenge
    {
        [JsonPropertyName("question")]
        public string Question { get; set; } = string.Empty;

        [JsonPropertyName("recommended_difficulty")]
        public int RecommendedDifficulty { get; set; }
    }
}