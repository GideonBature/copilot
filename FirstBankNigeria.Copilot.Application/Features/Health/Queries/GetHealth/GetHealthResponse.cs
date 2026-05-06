namespace FirstBankNigeria.Copilot.Application.Features.Health.Queries.GetHealth;

/// <summary>
/// Response returned by the health check query.
/// </summary>
public sealed class GetHealthResponse
{
    /// <summary>
    /// Overall health status of the service.
    /// </summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>
    /// Name of the service being checked.
    /// </summary>
    public string Service { get; init; } = string.Empty;
}
