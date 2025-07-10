using InterneTaakAfhandeling.Web.Server.Authentication;
using InterneTaakAfhandeling.Web.Server.Services.LogboekService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InterneTaakAfhandeling.Web.Server.Features.InterneTaakNote.AddNoteToInternetaak;

[Route("api/internetaken")]
[ApiController]
[Authorize]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public class AddNoteToInternetaakController(
    ITAUser user, 
    ILogboekService logboekService) : Controller
{
    private readonly ILogboekService _logboekService = logboekService ?? throw new ArgumentNullException(nameof(logboekService)); 
    private readonly ITAUser _user = user ?? throw new ArgumentNullException(nameof(user));

    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [HttpPost("{internetaakId}/add-note")]
    public async Task<IActionResult> AddNote([FromRoute] Guid internetaakId, [FromBody] AddNoteRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Note))
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Ongeldige aanvraag",
                    Detail = "Notitie is vereist en mag niet leeg zijn",
                    Status = StatusCodes.Status400BadRequest
                });
            } 

            var noteAction = KnownContactAction.Note(request.Note, _user);
            await _logboekService.LogContactRequestAction(noteAction, internetaakId); 

            return Ok(new { Message = "Notitie succesvol toegevoegd" });
        }
        catch (Exception)
        { 
            return StatusCode(500, new ProblemDetails
            {
                Title = "Interne server fout",
                Detail = "Er is een fout opgetreden bij het toevoegen van de notitie",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }
}

public class AddNoteRequest
{
    public required string Note { get; set; }
}