namespace FirstBankNigeria.Copilot.Application.Common.Settings;

/// <summary>
/// Configuration settings for the HealthCheck feature.
/// Bind from the "Health" section in appsettings.json.
/// </summary>
public sealed class HealthSettings
{
    public const string SectionName = "Health";

    /// <summary>
    /// The human-readable name of this service returned by the health endpoint.
    /// Set this per deployment in appsettings.json so the name is never inferred
    /// from the repository name.
    /// </summary>
    public string ServiceName { get; init; } = string.Empty;
}
