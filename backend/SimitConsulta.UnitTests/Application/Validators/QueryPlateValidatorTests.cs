using FluentValidation.TestHelper;
using SimitConsulta.Application.Features.PlateQueries.Commands.QueryPlate;
using Xunit;

namespace SimitConsulta.UnitTests.Application.Validators;

/// <summary>
/// Tests del validator del QueryPlateCommand.
/// Verifica validación de placa y del token captcha.
/// </summary>
public class QueryPlateValidatorTests
{
    private readonly QueryPlateValidator _validator = new();

    // ── Placa válida ──────────────────────────────────────

    [Theory]
    [InlineData("ABC123")]
    [InlineData("XYZ986")]
    [InlineData("LMN45D")]
    public void Validate_WithValidPlateAndToken_ShouldHaveNoErrors(string plate)
    {
        var result = _validator.TestValidate(
            new QueryPlateCommand(plate, "mock-token"));  // ← token

        result.ShouldNotHaveAnyValidationErrors();
    }

    // ── Placa inválida ────────────────────────────────────

    [Fact]
    public void Validate_WithEmptyPlate_ShouldReturnRequiredError()
    {
        var result = _validator.TestValidate(
            new QueryPlateCommand("", "mock-token"));  // ← token

        result.ShouldHaveValidationErrorFor(x => x.Plate)
              .WithErrorMessage("La placa es obligatoria.");
    }

    [Theory]
    [InlineData("AB1234")]
    [InlineData("1BC123")]
    [InlineData("ABCDEFG")]
    [InlineData("ABC12!")]
    public void Validate_WithInvalidFormat_ShouldReturnError(string plate)
    {
        var result = _validator.TestValidate(
            new QueryPlateCommand(plate, "mock-token"));  // ← token

        result.ShouldHaveValidationErrorFor(x => x.Plate);
    }

    // ── Token inválido ────────────────────────────────────

    [Fact]
    public void Validate_WithEmptyToken_ShouldReturnTokenError()
    {
        var result = _validator.TestValidate(
            new QueryPlateCommand("ABC123", ""));  // ← token vacío

        result.ShouldHaveValidationErrorFor(x => x.CaptchaToken)
              .WithErrorMessage("El token del captcha es obligatorio.");
    }

    [Fact]
    public void Validate_WithNullToken_ShouldReturnTokenError()
    {
        var result = _validator.TestValidate(
            new QueryPlateCommand("ABC123", null!));  // ← token null

        result.ShouldHaveValidationErrorFor(x => x.CaptchaToken);
    }
}