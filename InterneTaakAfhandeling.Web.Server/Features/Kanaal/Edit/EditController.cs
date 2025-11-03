using InterneTaakAfhandeling.Web.Server.Authentication;
using InterneTaakAfhandeling.Web.Server.Data;
using InterneTaakAfhandeling.Web.Server.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InterneTaakAfhandeling.Web.Server.Features.Kanaal.Edit;

[Route("api/kanaal")]
[ApiController]
[Authorize(Policy = FunctioneelBeheerderPolicy.Name)]
public class EditController(ApplicationDbContext dbContext) : Controller
{
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(KanalenEntity), StatusCodes.Status200OK)]
    public async Task<IActionResult> EditKanalen(string id, [FromBody] EditKanaalModel request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        try
        {
            var kanalen = await dbContext.Kanalen.FindAsync(Guid.Parse(id));
            if (kanalen == null)
                return NotFound(new ProblemDetails
                {
                    Title = "Kanaal Niet Gevonden",
                    Detail = "Het opgegeven kanaal bestaat niet.",
                    Status = StatusCodes.Status404NotFound
                });

            kanalen.Naam = request.Naam;
            dbContext.Update(kanalen);
            await dbContext.SaveChangesAsync();

            return Ok(kanalen);
        }
        catch (FormatException)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Ongeldig ID",
                Detail = "Het opgegeven ID heeft een ongeldig formaat.",
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            // Handle unique constraint violations specifically for the Naam field
            if (ex is DbUpdateException dbEx &&
                dbEx.InnerException != null &&
                (dbEx.InnerException.Message.Contains("unique", StringComparison.OrdinalIgnoreCase) ||
                 dbEx.InnerException.Message.Contains("duplicate", StringComparison.OrdinalIgnoreCase)) &&
                dbEx.InnerException.Message.Contains("Naam", StringComparison.OrdinalIgnoreCase))
                return Conflict(new ProblemDetails
                {
                    Title = "Kanaal Bestaat Al",
                    Detail = $"Er bestaat al een kanaal met de naam {request.Naam}.",
                    Status = StatusCodes.Status409Conflict
                });

            throw;
        }
    }
}