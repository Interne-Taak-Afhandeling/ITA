using InterneTaakAfhandeling.Web.Server.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InterneTaakAfhandeling.Web.Server.Features.Kanaal.Delete;

[Route("api/kanaal")]
[ApiController]
[Authorize]
public class DeleteController(ApplicationDbContext dbContext) : Controller
{
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteKanalen(string id)
    {
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

            dbContext.Kanalen.Remove(kanalen);
            await dbContext.SaveChangesAsync();

            return NoContent();
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
    }
}