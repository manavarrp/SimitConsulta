using System.Net;
using System.Net.Http.Json;
using Moq;
using SimitConsulta.Application.Features.PlateQueries.DTOs;
using SimitConsulta.Application.Interfaces;
using SimitConsulta.Domain.Models;
using SimitConsulta.IntegrationTests.Fixtures;
using Xunit;

namespace SimitConsulta.IntegrationTests.Api;

/// <summary>
/// Tests de integración del PlateQueryController.
/// Peticiones HTTP reales contra la API en memoria.
/// Solo ISimitGateway está mockeado — todo lo demás es real.
/// </summary>
public class PlateQueryControllerTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;
    private readonly Mock<ISimitGateway> _gatewayMock;

    public PlateQueryControllerTests(ApiFactory factory)
    {
        _client = factory.CreateClient();
        _gatewayMock = factory.SimitGatewayMock;
    }

    // ── POST /api/v1/query ────────────────────────────────

    [Fact]
    public async Task PostQuery_ValidPlate_NoRecords_ShouldReturn200()
    {
        _gatewayMock
            .Setup(s => s.QueryPlateAsync(
                "ABC123",
                It.IsAny<string>(),              // ← captchaToken
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SimitResponse(
                [], [], 0, true, false, false, "{}"));

        var response = await _client.PostAsJsonAsync(
            "/api/v1/query",
            new { plate = "ABC123", captchaToken = "mock-token" });  // ← token

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content
                                 .ReadFromJsonAsync<PlateQueryDto>();
        Assert.NotNull(body);
        Assert.Equal("ABC123", body.Plate);
    }

    [Fact]
    public async Task PostQuery_PlateWithFines_ShouldReturnFinesInResponse()
    {
        _gatewayMock
            .Setup(s => s.QueryPlateAsync(
                "XYZ986",
                It.IsAny<string>(),              // ← captchaToken
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SimitResponse(
                Fines: [new SimitFine(
                    "F-001", 500_000, "Pendiente",
                    "2025-01-01", "Movilidad", "C.02")],
                Summons: [],
                TotalAmount: 500_000,
                ClearedOfDebts: false,
                Cancelled: false,
                Suspended: false,
                RawResponse: "{}"));

        var response = await _client.PostAsJsonAsync(
            "/api/v1/query",
            new { plate = "XYZ986", captchaToken = "mock-token" });  // ← token

        var body = await response.Content
                                 .ReadFromJsonAsync<PlateQueryDto>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(1, body!.FinesCount);
        Assert.Equal(500_000, body.TotalAmount);
    }

    [Fact]
    public async Task PostQuery_InvalidPlate_ShouldReturn400()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/query",
            new { plate = "INVALIDA", captchaToken = "mock-token" });  // ← token

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostQuery_EmptyPlate_ShouldReturn400()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/query",
            new { plate = "", captchaToken = "mock-token" });  // ← token

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ── POST /api/v1/query/bulk ───────────────────────────

    [Fact]
    public async Task PostBulkQuery_ThreePlates_ShouldReturnThreeResults()
    {
        _gatewayMock
            .Setup(s => s.QueryPlateAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),              // ← captchaToken
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SimitResponse(
                [], [], 0, true, false, false, "{}"));

        var response = await _client.PostAsJsonAsync(
            "/api/v1/query/bulk",
            new                                   // ← token en bulk
            {
                plates = new[] { "ABC123", "XYZ986", "LMN45D" },
                captchaToken = "mock-token"
            });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content
                                 .ReadFromJsonAsync<BulkQueryDto>();
        Assert.Equal(3, body!.TotalProcessed);
    }

    [Fact]
    public async Task PostBulkQuery_EmptyList_ShouldReturn400()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/query/bulk",
            new
            {
                plates = Array.Empty<string>(),
                captchaToken = "mock-token"        // ← token
            });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ── GET /api/v1/history ───────────────────────────────

    [Fact]
    public async Task GetHistory_NoFilters_ShouldReturn200()
    {
        var response = await _client.GetAsync("/api/v1/history");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content
                                 .ReadFromJsonAsync<HistoryDto>();
        Assert.NotNull(body);
    }

    [Fact]
    public async Task GetHistory_PageZero_ShouldReturn400()
    {
        var response = await _client.GetAsync(
            "/api/v1/history?page=0");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ── GET /api/v1/health ────────────────────────────────

    [Fact]
    public async Task GetHealth_ShouldReturn200()
    {
        var response = await _client.GetAsync("/api/v1/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}