using MediatR;

namespace FirstBankNigeria.Copilot.Application.Features.Health.Queries.GetHealth;

/// <summary>
/// Query to retrieve the health status of the API.
/// </summary>
public sealed class GetHealthQuery : IRequest<GetHealthResponse>
{
}
