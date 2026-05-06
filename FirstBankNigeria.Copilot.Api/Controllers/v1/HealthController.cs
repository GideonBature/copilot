using FirstBankNigeria.Copilot.Application.Features.Health.Queries.GetHealth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FirstBankNigeria.Copilot.Api.Controllers.v1;

/// <summary>
/// Provides a health-check endpoint to verify that the API is running.
/// </summary>
[AllowAnonymous]
public sealed class HealthController : BaseController
{
    /// <summary>
    /// Returns the current health status of the API.
    /// </summary>
    /// <returns>200 OK with status and service name.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(GetHealthResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var response = await Sender.Send(new GetHealthQuery(), cancellationToken);
        return Ok(response);
    }
}
