using MediatR;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SimitConsulta.Application.Common.Results;
using SimitConsulta.Application.Features.PlateQueries.Commands.QueryPlate;
using SimitConsulta.Application.Features.PlateQueries.Commands.BulkQuery;
using SimitConsulta.Application.Features.PlateQueries.DTOs;

namespace SimitConsulta.UnitTests.Application.Handlers;

public class BulkQueryHandlerTests
{
    private readonly Mock<IMediator> _mediatorMock = new();
    private readonly BulkQueryHandler _handler;

    public BulkQueryHandlerTests()
    {
        _handler = new BulkQueryHandler(
            _mediatorMock.Object,
            NullLogger<BulkQueryHandler>.Instance);
    }

    [Fact]
    public async Task Handle_AllPlatesSuccessful_ShouldReturnCorrectCounts()
    {
        _mediatorMock
            .Setup(m => m.Send(
                It.IsAny<QueryPlateCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((QueryPlateCommand cmd, CancellationToken _) =>
                Result.Ok(new PlateQueryDto(
                    1, cmd.Plate, DateTime.UtcNow,
                    "SinMultas", "Masiva",
                    0, 0, 0, [], [], null)));

        var result = await _handler.Handle(
            new BulkQueryCommand(           // ← agregar token
                ["ABC123", "XYZ986", "LMN45D"],
                "mock-token"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value!.TotalProcessed);
        Assert.Equal(3, result.Value.Successful);
        Assert.Equal(0, result.Value.Failed);
    }

    [Fact]
    public async Task Handle_OnePlateFails_OthersContinue()
    {
        var callCount = 0;
        _mediatorMock
            .Setup(m => m.Send(
                It.IsAny<QueryPlateCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((QueryPlateCommand cmd, CancellationToken _) =>
            {
                callCount++;
                return callCount == 1
                    ? Result.Ok(new PlateQueryDto(
                        1, cmd.Plate, DateTime.UtcNow,
                        "Exitoso", "Masiva",
                        1, 0, 100_000, [], [], null))
                    : Result<PlateQueryDto>.Fail("Error de conexión");
            });

        var result = await _handler.Handle(
            new BulkQueryCommand(           // ← agregar token
                ["ABC123", "XYZ986"],
                "mock-token"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.TotalProcessed);
        Assert.Equal(1, result.Value.Successful);
        Assert.Equal(1, result.Value.Failed);
    }

    [Fact]
    public async Task Handle_EmptyList_ShouldReturnZeroResults()
    {
        var result = await _handler.Handle(
            new BulkQueryCommand([], "mock-token"),  // ← agregar token
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value!.TotalProcessed);
    }
}