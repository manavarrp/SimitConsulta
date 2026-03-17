using System.Security.Cryptography;
using System.Text;

namespace SimitConsulta.Infrastructure.Utils;

public static class HashHelper
{
    /// <summary>
    /// SHA256 hex lowercase de un string UTF-8.
    /// </summary>
    public static string Sha256Hex(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    /// <summary>
    /// Verifica si un número es primo.
    /// Usa sqrt para eficiencia — O(sqrt(n)) en lugar de O(n).
    /// </summary>
    public static bool IsPrime(long value)
    {
        if (value < 2) return false;
        if (value == 2) return true;
        if (value % 2 == 0) return false;

        var sqrt = (long)Math.Sqrt(value);
        for (long i = 3; i <= sqrt; i += 2)
            if (value % i == 0) return false;

        return true;
    }

    /// <summary>
    /// Construye el JSON exactamente como JSON.stringify del navegador.
    /// Orden crítico: question, time, nonce — si cambia el orden
    /// el hash es diferente y el SIMIT rechaza el token.
    /// </summary>
    public static string BuildVerifyJson(
        string question, long time, long nonce) =>
        $"{{\"question\":\"{question}\"," +
        $"\"time\":{time}," +
        $"\"nonce\":{nonce}}}";

    /// <summary>
    /// Resuelve una iteración del PoW del SIMIT.
    /// Busca el siguiente nonce primo tal que
    /// SHA256({"question":q,"time":t,"nonce":n}) empiece con "0000".
    /// </summary>
    public static long SolvePoWSingle(
      string question,
      long time,
      long startNonce = 1,
      long maxIterations = 10_000_000)
    {
        // Validación que el test espera
        if (string.IsNullOrWhiteSpace(question))
            throw new ArgumentException(
                "Question cannot be empty.", nameof(question));

        for (long nonce = startNonce + 1; nonce < maxIterations; nonce++)
        {
            if (!IsPrime(nonce)) continue;
            var json = BuildVerifyJson(question, time, nonce);
            var hash = Sha256Hex(json);
            if (hash.StartsWith("0000"))
                return nonce;
        }

        throw new InvalidOperationException(
            $"Could not solve PoW in {maxIterations:N0} iterations.");
    }

    /// <summary>
    /// Resuelve el PoW N veces (difficulty) y retorna el array
    /// de objetos de verificación exactamente como el captcha-worker.js.
    /// Orden de propiedades: question, time, nonce.
    /// </summary>
    public static string SolvePoWAndBuildToken(
        string question,
        long time,
        int difficulty)
    {
        var parts = new List<string>(difficulty);
        long lastNonce = 1;

        for (int i = 0; i < difficulty; i++)
        {
            lastNonce = SolvePoWSingle(question, time, lastNonce);
            // JSON manual — orden exacto igual que el worker
            parts.Add(BuildVerifyJson(question, time, lastNonce));
        }

        // Array de objetos serializado como string
        return "[" + string.Join(",", parts) + "]";
    }
}