using InterneTaakAfhandeling.Common.Exceptions;
using InterneTaakAfhandeling.Web.Server.Authentication;
using InterneTaakAfhandeling.Web.Server.Data;
using InterneTaakAfhandeling.Web.Server.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InterneTaakAfhandeling.Web.Server.Features.Kanaal.Create;

[Route("api/kanaal")]
[ApiController]
[Authorize(Policy = FunctioneelBeheerderPolicy.Name)]
public class CreateController(ApplicationDbContext dbContext) : Controller
{
    [HttpPost] 
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(KanalenEntity), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateKanaal([FromBody] CreateKanaalModel request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        try
        {
            var kanaal = new KanalenEntity { Naam = request.Naam };
            dbContext.Add(kanaal);
            await dbContext.SaveChangesAsync();
            
            return StatusCode(201, kanaal);
        }
        catch (Exception ex)
        {
            // Handle unique constraint violations specifically for the Naam field
            if (ex is DbUpdateException dbEx && 
                dbEx.InnerException != null &&
                (dbEx.InnerException.Message.Contains("unique", StringComparison.OrdinalIgnoreCase) ||
                 dbEx.InnerException.Message.Contains("duplicate", StringComparison.OrdinalIgnoreCase)) &&
                dbEx.InnerException.Message.Contains("Naam", StringComparison.OrdinalIgnoreCase) &&
                dbEx.Entries.Any(entry => entry.Entity is KanalenEntity && entry.State == EntityState.Added))
            {
                return Conflict(new ProblemDetails
                {
                    Title = "Kanaal Bestaat Al",
                    Detail = $"Er bestaat al een kanaal met de naam {request.Naam}.",
                    Status = StatusCodes.Status409Conflict
                });
            }
             
            return StatusCode(500, new ProblemDetails
            {
                Title = "Interne Serverfout",
                Detail = "Er is een onverwachte fout opgetreden bij het aanmaken van het kanaal. Probeer het later opnieuw.",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }
}
