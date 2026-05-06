using FirstBankNigeria.Copilot.Application.Common.Settings;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FirstBankNigeria.Copilot.Application.Features.Health.Queries.GetHealth;

/// <summary>
/// Handles the <see cref="GetHealthQuery"/> and returns the current health status.
/// </summary>
public sealed class GetHealthQueryHandler : IRequestHandler<GetHealthQuery, GetHealthResponse>
{
    private readonly ILogger<GetHealthQueryHandler> _logger;
    private readonly HealthSettings _healthSettings;

    public GetHealthQueryHandler(
        ILogger<GetHealthQueryHandler> logger,
        IOptions<HealthSettings> healthSettings)
    {
        _logger = logger;
        _healthSettings = healthSettings.Value;
    }

    public Task<GetHealthResponse> Handle(GetHealthQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Health check requested.");

        var response = new GetHealthResponse
        {
            Status = "Healthy",
            Service = _healthSettings.ServiceName
        };

        return Task.FromResult(response);
    }
}
