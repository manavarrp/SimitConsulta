using SimitConsulta.Domain.Exceptions;
using SimitConsulta.Domain.ValueObjects;

namespace SimitConsulta.UnitTests.Domain;

/// <summary>
/// Tests del Value Object Plate.
/// Verifica creación, validación e igualdad estructural.
/// </summary>
public class PlateTests
{
    // ── Create() válidos ──────────────────────────────────

    [Theory]
    [InlineData("ABC123")]       // carro estándar
    [InlineData("XYZ986")]       // carro
    [InlineData("LMN45D")]       // moto
    [InlineData("abc123")]       // minúsculas → se normalizan
    [InlineData("  ABC123  ")]   // espacios → se eliminan
    public void Create_WithValidFormat_ShouldReturnInstance(string input)
    {
        var plate = Plate.Create(input);

        Assert.NotNull(plate);
        Assert.Equal(input.Trim().ToUpperInvariant(), plate.Value);
    }

    // ── Create() inválidos ────────────────────────────────

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("AB123")]
    [InlineData("1BC123")]
    [InlineData("ABCD123")]
    [InlineData("ABC12!")]
    public void Create_WithInvalidFormat_ShouldThrowDomainException(
        string input)
    {
        Assert.Throws<DomainException>(() => Plate.Create(input));
    }

    // ── TryCreate() ───────────────────────────────────────

    [Fact]
    public void TryCreate_WithValidPlate_ShouldReturnTrue()
    {
        var success = Plate.TryCreate("ABC123", out var plate, out var error);

        Assert.True(success);
        Assert.NotNull(plate);
        Assert.Equal(string.Empty, error);
    }

    [Fact]
    public void TryCreate_WithInvalidPlate_ShouldReturnFalse()
    {
        var success = Plate.TryCreate("INVALIDA", out var plate, out var error);

        Assert.False(success);
        Assert.Null(plate);
        Assert.NotEmpty(error);
    }

    // ── Igualdad estructural ──────────────────────────────

    [Fact]
    public void Equality_SamePlateValue_ShouldBeEqual()
    {
        var p1 = Plate.Create("ABC123");
        var p2 = Plate.Create("abc123");

        Assert.Equal(p1, p2);
        Assert.Equal(p1.GetHashCode(), p2.GetHashCode());
    }

    // ── Conversión implícita ──────────────────────────────

    [Fact]
    public void ImplicitConversion_ShouldReturnStringValue()
    {
        var plate = Plate.Create("ABC123");
        string value = plate;

        Assert.Equal("ABC123", value);
    }
}