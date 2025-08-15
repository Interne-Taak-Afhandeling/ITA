using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InterneTaakAfhandeling.Web.Server.Features.ForwardContactRequest;

[Route("api/internetaken")]
[ApiController]
[Authorize]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public class ForwardContactRequestController(
    IForwardContactRequestService forwardContactRequestService) : Controller
{
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpPost("{id:guid}/forward")]
    public async Task<IActionResult> ForwardContactRequestAsync([FromRoute] Guid id,
        [FromBody] ForwardContactRequestModel request)
    {
        return Ok(await forwardContactRequestService.ForwardAsync(id, request));
    }
}