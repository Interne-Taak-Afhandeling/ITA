using InterneTaakAfhandeling.Web.Server.Authentication;
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
    ILogboekService logboekService) : Controller
{
    private readonly ILogboekService _logboekService =
        logboekService ?? throw new ArgumentNullException(nameof(logboekService));

    private readonly ITAUser _user = user ?? throw new ArgumentNullException(nameof(user));

    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [HttpPost("{internetaakId}/notitie")]
    public async Task<IActionResult> AddNote([FromRoute] Guid internetaakId, [FromBody] AddNotitieRequest request)
    {
        var notitieAction = KnownContactAction.Note(request.Notitie, _user);
        await _logboekService.LogContactRequestAction(notitieAction, internetaakId);

        return Ok(new { Message = "Notitie succesvol toegevoegd" });
    }
}

public class AddNotitieRequest
{
    public required string Notitie { get; set; }
}