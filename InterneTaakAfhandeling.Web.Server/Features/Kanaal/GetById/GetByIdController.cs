using InterneTaakAfhandeling.Web.Server.Authentication;
using InterneTaakAfhandeling.Web.Server.Data;
using InterneTaakAfhandeling.Web.Server.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InterneTaakAfhandeling.Web.Server.Features.Kanaal.GetById;

[Route("api/kanaal")]
[ApiController]
[Authorize(Policy = FunctioneelBeheerderPolicy.Name)]
public class GetByIdController(ApplicationDbContext dbContext) : Controller
{
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(KanalenEntity), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetKanalenById(string id)
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
    }
}