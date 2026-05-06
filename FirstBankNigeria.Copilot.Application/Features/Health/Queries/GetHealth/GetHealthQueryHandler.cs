using MediatR;
using Microsoft.Extensions.Logging;

namespace FirstBankNigeria.Copilot.Application.Features.Health.Queries.GetHealth;

/// <summary>
/// Handles the <see cref="GetHealthQuery"/> and returns the current health status.
/// </summary>
public sealed class GetHealthQueryHandler : IRequestHandler<GetHealthQuery, GetHealthResponse>
{
    private readonly ILogger<GetHealthQueryHandler> _logger;

    public GetHealthQueryHandler(ILogger<GetHealthQueryHandler> logger)
    {
        _logger = logger;
    }

    public Task<GetHealthResponse> Handle(GetHealthQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Health check requested.");

        var response = new GetHealthResponse
        {
            Status = "Healthy",
            Service = "Copilot API"
        };

        return Task.FromResult(response);
    }
}
