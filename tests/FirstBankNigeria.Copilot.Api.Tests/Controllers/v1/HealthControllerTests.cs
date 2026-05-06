using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace FirstBankNigeria.Copilot.Api.Tests.Controllers.v1;

public sealed class HealthControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public HealthControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Get_HealthEndpoint_Returns200Ok()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/v1/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Get_HealthEndpoint_ReturnsHealthyStatus()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/v1/health");
        var content = await response.Content.ReadAsStringAsync();
        var body = JsonSerializer.Deserialize<JsonElement>(content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.GetProperty("status").GetString().Should().Be("Healthy");
    }

    [Fact]
    public async Task Get_HealthEndpoint_ReturnsCorrectServiceName()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/v1/health");
        var content = await response.Content.ReadAsStringAsync();
        var body = JsonSerializer.Deserialize<JsonElement>(content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.GetProperty("service").GetString().Should().Be("Copilot API");
    }

    [Fact]
    public async Task Get_HealthEndpoint_DoesNotRequireAuthentication()
    {
        // Arrange — no Authorization header set
        var request = new HttpRequestMessage(HttpMethod.Get, "/v1/health");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }
}
