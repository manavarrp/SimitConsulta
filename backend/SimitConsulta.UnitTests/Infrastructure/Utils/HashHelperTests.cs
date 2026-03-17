using SimitConsulta.Infrastructure.Utils;
using Xunit;

namespace SimitConsulta.UnitTests.Infrastructure.Utils;

/// <summary>
/// Tests del HashHelper — SHA256 y algoritmo PoW del SIMIT.
/// </summary>
public class HashHelperTests
{
    [Fact]
    public void IsPrime_WithPrimeNumbers_ShouldReturnTrue()
    {
        Assert.True(HashHelper.IsPrime(2));
        Assert.True(HashHelper.IsPrime(3));
        Assert.True(HashHelper.IsPrime(7));
        Assert.True(HashHelper.IsPrime(11));
        Assert.True(HashHelper.IsPrime(219677));
    }

    [Fact]
    public void IsPrime_WithNonPrimeNumbers_ShouldReturnFalse()
    {
        Assert.False(HashHelper.IsPrime(1));
        Assert.False(HashHelper.IsPrime(4));
        Assert.False(HashHelper.IsPrime(100));
        Assert.False(HashHelper.IsPrime(1000));  // ← reemplaza 256483
    }

    [Fact]
    public void BuildVerifyJson_ShouldUseCorrectPropertyOrder()
    {
        // El orden question, time, nonce es crítico para el hash
        var json = HashHelper.BuildVerifyJson("abc123", 1000, 7);

        Assert.Equal(
            "{\"question\":\"abc123\",\"time\":1000,\"nonce\":7}",
            json);
    }

    [Fact]
    public void SolvePoWSingle_ShouldFindValidNonce()
    {
        // Usar un challenge conocido para verificar el algoritmo
        var question = "564ba4ff6e61e786de3345c88c80b241";
        var time = 1773777819L;

        var nonce = HashHelper.SolvePoWSingle(question, time, 1);
        var json = HashHelper.BuildVerifyJson(question, time, nonce);
        var hash = HashHelper.Sha256Hex(json);

        // Verificar que el hash empieza con 0000
        Assert.StartsWith("0000", hash);

        // Verificar que el nonce es primo
        Assert.True(HashHelper.IsPrime(nonce));
    }

    [Fact]
    public void SolvePoWAndBuildToken_ShouldReturnValidJsonArray()
    {
        var question = "564ba4ff6e61e786de3345c88c80b241";
        var time = 1773777819L;

        var token = HashHelper.SolvePoWAndBuildToken(question, time, 1);

        // Debe ser un array JSON válido
        Assert.StartsWith("[", token);
        Assert.EndsWith("]", token);
        Assert.Contains("question", token);
        Assert.Contains("time", token);
        Assert.Contains("nonce", token);
    }

    [Fact]
    public void SolvePoWSingle_WithEmptyQuestion_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(
            () => HashHelper.SolvePoWSingle("", 1000));
    }
}