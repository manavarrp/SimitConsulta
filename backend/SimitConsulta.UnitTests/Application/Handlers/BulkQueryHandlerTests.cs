using MediatR;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SimitConsulta.Application.Common.Results;
using SimitConsulta.Application.Features.PlateQueries.Commands.BulkQuery;
using SimitConsulta.Application.Features.PlateQueries.Commands.QueryPlate;
using SimitConsulta.Application.Features.PlateQueries.DTOs;

namespace SimitConsulta.UnitTests.Application.Handlers;

/// <summary>
/// Tests del BulkQueryHandler.
/// Mockea IMediator para simular respuestas del comando individual.
/// </summary>
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
        // Arrange — todas las placas retornan SinMultas
        _mediatorMock
            .Setup(m => m.Send(
                It.IsAny<QueryPlateCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((QueryPlateCommand cmd, CancellationToken _) =>
                Result.Ok(new PlateQueryDto(
                    1, cmd.Plate, DateTime.UtcNow,
                    "SinMultas", "Masiva",
                    0, 0, 0, [], [], null)));

        // Act
        var result = await _handler.Handle(
            new BulkQueryCommand(["ABC123", "XYZ986", "LMN45D"]),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value!.TotalProcessed);
        Assert.Equal(3, result.Value.Successful);
        Assert.Equal(0, result.Value.Failed);
    }

    [Fact]
    public async Task Handle_OnePlateFails_OthersContinue()
    {
        // Arrange — primera exitosa, segunda falla
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

        // Act
        var result = await _handler.Handle(
            new BulkQueryCommand(["ABC123", "XYZ986"]),
            CancellationToken.None);

        // Assert — el lote no se detiene por un error
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.TotalProcessed);
        Assert.Equal(1, result.Value.Successful);
        Assert.Equal(1, result.Value.Failed);
    }

    [Fact]
    public async Task Handle_EmptyList_ShouldReturnZeroResults()
    {
        var result = await _handler.Handle(
            new BulkQueryCommand([]),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value!.TotalProcessed);
    }
}