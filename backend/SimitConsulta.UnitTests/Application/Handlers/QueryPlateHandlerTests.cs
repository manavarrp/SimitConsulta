using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SimitConsulta.Application.Features.PlateQueries.Commands.QueryPlate;
using SimitConsulta.Application.Interfaces;
using SimitConsulta.Domain.Entities;
using SimitConsulta.Domain.Enums;
using SimitConsulta.Domain.Models;

namespace SimitConsulta.UnitTests.Application.Handlers;

/// <summary>
/// Tests del QueryPlateHandler.
/// Mocks de IPlateQueryRepository e ISimitGateway aíslan
/// el handler de BD y del SIMIT real.
/// </summary>
public class QueryPlateHandlerTests
{
    private readonly Mock<IPlateQueryRepository> _repoMock = new();
    private readonly Mock<ISimitGateway> _gatewayMock = new();
    private readonly QueryPlateHandler _handler;

    public QueryPlateHandlerTests()
    {
        _handler = new QueryPlateHandler(
            _repoMock.Object,
            _gatewayMock.Object,
            NullLogger<QueryPlateHandler>.Instance);

        // Setup base — Add y Update no hacen nada por defecto
        _repoMock
            .Setup(r => r.AddAsync(
                It.IsAny<PlateQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((PlateQuery q, CancellationToken _) => q);

        _repoMock
            .Setup(r => r.UpdateAsync(
                It.IsAny<PlateQuery>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task Handle_PlateWithNoRecords_ShouldReturnSinMultas()
    {
        // Arrange — SIMIT no devuelve registros
        _gatewayMock
            .Setup(s => s.QueryPlateAsync(
                "ABC123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SimitResponse(
                Fines: [],
                Summons: [],
                TotalAmount: 0,
                ClearedOfDebts: true,
                Cancelled: false,
                Suspended: false,
                RawResponse: "{}"));

        // Act
        var result = await _handler.Handle(
            new QueryPlateCommand("ABC123"),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(
            QueryStatus.SinMultas.ToString(),
            result.Value!.Status);
    }

    [Fact]
    public async Task Handle_PlateWithFines_ShouldReturnExitoso()
    {
        // Arrange — SIMIT devuelve una multa
        _gatewayMock
            .Setup(s => s.QueryPlateAsync(
                "XYZ986", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SimitResponse(
                Fines: [new SimitFine(
                    "F-001", 650_000, "Pendiente",
                    "2025-10-10", "Sec. Movilidad", "C.02")],
                Summons: [],
                TotalAmount: 650_000,
                ClearedOfDebts: false,
                Cancelled: false,
                Suspended: false,
                RawResponse: "{}"));

        // Act
        var result = await _handler.Handle(
            new QueryPlateCommand("XYZ986"),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(
            QueryStatus.Exitoso.ToString(),
            result.Value!.Status);
        Assert.Equal(1, result.Value.FinesCount);
        Assert.Equal(650_000, result.Value.TotalAmount);
    }

    [Fact]
    public async Task Handle_SimitFails_ShouldReturnFailAndPersistError()
    {
        // Arrange — SIMIT no responde
        _gatewayMock
            .Setup(s => s.QueryPlateAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Timeout"));

        // Act
        var result = await _handler.Handle(
            new QueryPlateCommand("ABC123"),
            CancellationToken.None);

        // Assert — retorna Fail sin propagar excepción
        Assert.True(result.IsFailure);
        Assert.Contains("Timeout", result.Error);

        // Verifica que se persistió el error en BD
        _repoMock.Verify(r => r.UpdateAsync(
            It.Is<PlateQuery>(q => q.Status == QueryStatus.Error),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_InvalidPlate_ShouldReturnFail_WithoutCallingGateway()
    {
        // Act — placa inválida
        var result = await _handler.Handle(
            new QueryPlateCommand("INVALIDA"),
            CancellationToken.None);

        // Assert — falla sin llamar al gateway ni al repositorio
        Assert.True(result.IsFailure);

        _gatewayMock.Verify(
            s => s.QueryPlateAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _repoMock.Verify(
            r => r.AddAsync(
                It.IsAny<PlateQuery>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}