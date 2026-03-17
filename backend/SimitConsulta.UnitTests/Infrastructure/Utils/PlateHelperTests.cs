using SimitConsulta.Infrastructure.Utils;
using Xunit;

namespace SimitConsulta.UnitTests.Infrastructure.Utils;

/// <summary>
/// Tests del PlateHelper — normalización, filtrado y extracción.
/// </summary>
public class PlateHelperTests
{
    [Theory]
    [InlineData("ABC123", true)]
    [InlineData("XYZ98D", true)]
    [InlineData("abc123", true)]
    [InlineData("AB123", false)]
    [InlineData("1BC123", false)]
    [InlineData("", false)]
    public void IsValid_ShouldReturnExpectedResult(
        string plate, bool expected)
    {
        Assert.Equal(expected, PlateHelper.IsValid(plate));
    }

    [Fact]
    public void Filter_WithMixedList_ShouldSeparateCorrectly()
    {
        var list = new[] { "ABC123", "INVALIDA", "XYZ986", "LMN45D" };

        var (valid, invalid) = PlateHelper.Filter(list);

        Assert.Equal(3, valid.Count);
        Assert.Equal(1, invalid.Count);
        Assert.Contains("ABC123", valid);
        Assert.Contains("INVALIDA", invalid);
    }

    [Fact]
    public void ExtractFrom_WithMixedDelimiters_ShouldExtractPlates()
    {
        var text = "ABC123, XYZ986\nLMN45D; INVALIDA";

        var plates = PlateHelper.ExtractFrom(text);

        Assert.Equal(3, plates.Count);
        Assert.Contains("ABC123", plates);
        Assert.Contains("XYZ986", plates);
        Assert.Contains("LMN45D", plates);
    }

    [Fact]
    public void ExtractFrom_WithDuplicates_ShouldDeduplicate()
    {
        var text = "ABC123\nABC123\nabc123";

        var plates = PlateHelper.ExtractFrom(text);

        Assert.Single(plates);
    }

    [Fact]
    public void Normalize_ShouldConvertToUppercaseAndTrim()
    {
        Assert.Equal("ABC123", PlateHelper.Normalize("  abc123  "));
    }
}