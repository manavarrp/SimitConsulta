using System.Security.Cryptography;
using System.Text;

namespace SimitConsulta.Infrastructure.Utils;

/// <summary>
/// Utilidades de hashing para el captcha Proof-of-Work del SIMIT.
/// Clase estática — sin estado, sin DI, resultados deterministas.
/// Testeable directamente sin mocks.
/// </summary>
public static class HashHelper
{
    /// <summary>
    /// Calcula MD5 de un string UTF-8 y lo retorna como hex lowercase.
    /// Ejemplo: Md5Hex("abc") → "900150983cd24fb0d6963f7d28e17f72"
    /// </summary>
    public static string Md5Hex(string input)
    {
        var bytes = MD5.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    /// <summary>
    /// Resuelve el captcha Proof-of-Work del SIMIT.
    /// Busca el nonce mínimo tal que MD5(question + nonce)
    /// empiece con N ceros hexadecimales (difficulty).
    /// Típicamente resuelve en menos de 5.000 iteraciones.
    /// </summary>
    /// <param name="question">Hash recibido del servidor de captcha.</param>
    /// <param name="difficulty">Ceros requeridos al inicio del hash.</param>
    /// <param name="maxIterations">Límite para evitar bucles infinitos.</param>
    /// <exception cref="ArgumentException">Si question es vacío.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Si difficulty fuera de 1–8.</exception>
    /// <exception cref="InvalidOperationException">Si no hay solución en maxIterations.</exception>
    public static long SolvePoW(
        string question,
        int difficulty,
        long maxIterations = 10_000_000)
    {
        if (string.IsNullOrWhiteSpace(question))
            throw new ArgumentException(
                "Question cannot be empty.", nameof(question));

        if (difficulty < 1 || difficulty > 8)
            throw new ArgumentOutOfRangeException(
                nameof(difficulty), "Difficulty must be between 1 and 8.");

        var prefix = new string('0', difficulty);

        for (long nonce = 0; nonce < maxIterations; nonce++)
            if (Md5Hex($"{question}{nonce}").StartsWith(prefix))
                return nonce;

        throw new InvalidOperationException(
            $"Could not solve PoW for question='{question}' " +
            $"difficulty={difficulty} in {maxIterations:N0} iterations.");
    }
}