using SimitConsulta.Application.Common.Results;

namespace SimitConsulta.UnitTests.Application;

/// <summary>
/// Tests del Result monad.
/// Verifica Ok, Fail, Map y BindAsync.
/// </summary>
public class ResultTests
{
    [Fact]
    public void Ok_ShouldCreateSuccessfulResult()
    {
        var result = Result.Ok(42);

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(42, result.Value);
        Assert.Null(result.Error);
    }

    [Fact]
    public void Fail_ShouldCreateFailedResult()
    {
        var result = Result<int>.Fail("algo salió mal");

        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("algo salió mal", result.Error);
    }

    [Fact]
    public void Map_OnSuccess_ShouldTransformValue()
    {
        var result = Result.Ok(10).Map(x => x * 2);

        Assert.True(result.IsSuccess);
        Assert.Equal(20, result.Value);
    }

    [Fact]
    public void Map_OnFailure_ShouldPropagateError()
    {
        var mapperExecuted = false;

        var result = Result<int>.Fail("error")
            .Map(x => { mapperExecuted = true; return x * 2; });

        Assert.True(result.IsFailure);
        Assert.Equal("error", result.Error);
        Assert.False(mapperExecuted);
    }

    [Fact]
    public async Task BindAsync_OnSuccess_ShouldChainOperation()
    {
        var result = await Result.Ok(5)
            .BindAsync(x => Task.FromResult(Result.Ok(x + 1)));

        Assert.True(result.IsSuccess);
        Assert.Equal(6, result.Value);
    }

    [Fact]
    public async Task BindAsync_OnFailure_ShouldShortCircuit()
    {
        var nextCalled = false;

        var result = await Result<int>.Fail("fallo")
            .BindAsync(x =>
            {
                nextCalled = true;
                return Task.FromResult(Result.Ok(x + 1));
            });

        Assert.True(result.IsFailure);
        Assert.False(nextCalled);
    }

    [Fact]
    public void ToString_ShouldReflectState()
    {
        Assert.StartsWith("Ok", Result.Ok("valor").ToString());
        Assert.StartsWith("Fail", Result<string>.Fail("err").ToString());
    }
}