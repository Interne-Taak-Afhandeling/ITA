using System.ComponentModel.DataAnnotations;
using InterneTaakAfhandeling.Web.Server.Authentication;
using InterneTaakAfhandeling.Web.Server.Guards;
using InterneTaakAfhandeling.Web.Server.Services.LogboekService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InterneTaakAfhandeling.Web.Server.Features.ReopenContactRequest;

[Route("api/internetaken")]
[ApiController]
[Authorize(Policy = FunctioneelBeheerderPolicy.Name)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public class ReopenContactRequestController(
    IReopenContactRequestService reopenContactRequestService,
    ILogboekService logboekService,
    IInternetaakGuardService internetaakGuardService,
    ITAUser user,
    ILogger<ReopenContactRequestController> logger) : Controller
{
    private readonly ITAUser _user = user ?? throw new ArgumentNullException(nameof(user));

    [ProducesResponseType(typeof(ReopenContactRequestResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [HttpPost("{id:guid}/heropen")]
    public async Task<IActionResult> ReopenContactRequestAsync(
        [FromRoute] Guid id,
        [FromBody] ReopenContactRequestModel request)
    {
        if (string.IsNullOrWhiteSpace(request.Reden))
        {
            throw new ValidationException("Reden is verplicht en mag niet leeg zijn.");
        }

        await internetaakGuardService.GuardAgainstNietVerwerktAsync(id);

        logger.LogInformation(
            "Reopening internetaak {InternetaakId} by user {UserId} with reason: {Reden}",
            id, _user.Email, request.Reden);

        var result = await reopenContactRequestService.ReopenAsync(id);

        await logboekService.LogContactRequestAction(
            KnownContactAction.Reopened(request.Reden, _user), id);

        return Ok(new ReopenContactRequestResponse
        {
            Internetaak = result.Internetaak,
            Waarschuwing = result.Waarschuwing
        });
    }
}
