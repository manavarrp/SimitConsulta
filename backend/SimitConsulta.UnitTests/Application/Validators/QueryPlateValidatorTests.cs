using FluentValidation.TestHelper;
using SimitConsulta.Application.Features.PlateQueries.Commands.QueryPlate;

namespace SimitConsulta.UnitTests.Application.Validators;

/// <summary>
/// Tests del validator del QueryPlateCommand.
/// </summary>
public class QueryPlateValidatorTests
{
    private readonly QueryPlateValidator _validator = new();

    [Theory]
    [InlineData("ABC123")]
    [InlineData("XYZ986")]
    [InlineData("LMN45D")]
    public void Validate_WithValidPlate_ShouldHaveNoErrors(string plate)
    {
        var result = _validator.TestValidate(
            new QueryPlateCommand(plate));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyPlate_ShouldReturnRequiredError()
    {
        var result = _validator.TestValidate(
            new QueryPlateCommand(""));

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
            new QueryPlateCommand(plate));

        result.ShouldHaveValidationErrorFor(x => x.Plate);
    }
}