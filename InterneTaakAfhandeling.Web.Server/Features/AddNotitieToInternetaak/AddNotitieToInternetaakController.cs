using InterneTaakAfhandeling.Web.Server.Authentication;
using InterneTaakAfhandeling.Web.Server.Guards;
using InterneTaakAfhandeling.Web.Server.Services.LogboekService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InterneTaakAfhandeling.Web.Server.Features.AddNotitieToInternetaak;

[Route("api/internetaken")]
[ApiController]
[Authorize]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public class AddNotitieToInternetaakController(
    ITAUser user,
    ILogboekService logboekService,
    IInternetaakGuardService internetaakGuardService) : Controller
{
    private readonly ILogboekService _logboekService =
        logboekService ?? throw new ArgumentNullException(nameof(logboekService));

    private readonly ITAUser _user = user ?? throw new ArgumentNullException(nameof(user));

    private readonly IInternetaakGuardService _internetaakGuardService =
        internetaakGuardService ?? throw new ArgumentNullException(nameof(internetaakGuardService));

    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [HttpPost("{internetaakId}/notitie")]
    public async Task<IActionResult> AddNote([FromRoute] Guid internetaakId, [FromBody] AddNotitieRequest request)
    {
        var blocked = await _internetaakGuardService.EnsureNotVerwerktAsync(internetaakId);
        if (blocked != null) return blocked;

        var notitieAction = KnownContactAction.Note(request.Notitie, _user);
        await _logboekService.LogContactRequestAction(notitieAction, internetaakId);

        return Ok(new { Message = "Notitie succesvol toegevoegd" });
    }
}

public class AddNotitieRequest
{
    public required string Notitie { get; set; }
}