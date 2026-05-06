using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FirstBankNigeria.Copilot.Api.Controllers;

/// <summary>
/// Base controller. All controllers inherit from this class.
/// JWT authorisation is enforced globally at this level.
/// </summary>
[Authorize]
[ApiController]
[Route("v1/[controller]")]
public abstract class BaseController : ControllerBase
{
    private ISender? _sender;

    protected ISender Sender =>
        _sender ??= HttpContext.RequestServices.GetRequiredService<ISender>();
}
