using InterneTaakAfhandeling.Web.Server.Authentication;
using InterneTaakAfhandeling.Web.Server.Guards;
using InterneTaakAfhandeling.Web.Server.Services.LogboekService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InterneTaakAfhandeling.Web.Server.Features.ForwardContactRequest;

[Route("api/internetaken")]
[ApiController]
[Authorize]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public class ForwardContactRequestController(
    IForwardContactRequestService forwardContactRequestService,
    ILogboekService logboekService,
    IInternetaakGuardService internetaakGuardService,
    ITAUser user) : Controller
{
    private readonly ITAUser _user = user ?? throw new ArgumentNullException(nameof(user));

    [ProducesResponseType(typeof(ForwardContactRequestResponse),StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [HttpPost("{id:guid}/forward")]
    public async Task<IActionResult> ForwardContactRequestAsync([FromRoute] Guid id,
        [FromBody] ForwardContactRequestModel request)
    {
        var blocked = await internetaakGuardService.EnsureNotVerwerktAsync(id, "forward");
        if (blocked != null) return blocked;

        var response = await forwardContactRequestService.ForwardAsync(id, request);
        await logboekService.LogContactRequestAction(KnownContactAction.ForwardKlantContact(request, _user), id);

        return Ok(response);
    }
}