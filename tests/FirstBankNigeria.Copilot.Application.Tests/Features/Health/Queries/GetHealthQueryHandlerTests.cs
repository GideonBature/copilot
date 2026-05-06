using FirstBankNigeria.Copilot.Application.Common.Settings;
using FirstBankNigeria.Copilot.Application.Features.Health.Queries.GetHealth;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace FirstBankNigeria.Copilot.Application.Tests.Features.Health.Queries;

public sealed class GetHealthQueryHandlerTests
{
    private readonly Mock<ILogger<GetHealthQueryHandler>> _loggerMock;
    private readonly IOptions<HealthSettings> _healthOptions;
    private readonly GetHealthQueryHandler _handler;

    public GetHealthQueryHandlerTests()
    {
        _loggerMock = new Mock<ILogger<GetHealthQueryHandler>>();
        _healthOptions = Options.Create(new HealthSettings { ServiceName = "Test Service" });
        _handler = new GetHealthQueryHandler(_loggerMock.Object, _healthOptions);
    }

    [Fact]
    public async Task Handle_ValidQuery_ReturnsHealthyStatus()
    {
        // Arrange
        var query = new GetHealthQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be("Healthy");
    }

    [Fact]
    public async Task Handle_ValidQuery_ReturnsConfiguredServiceName()
    {
        // Arrange
        var query = new GetHealthQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Service.Should().Be("Test Service");
    }

    [Fact]
    public async Task Handle_ValidQuery_ReturnsNonNullResponse()
    {
        // Arrange
        var query = new GetHealthQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().NotBeNullOrWhiteSpace();
        result.Service.Should().NotBeNullOrWhiteSpace();
    }
}
