using InterneTaakAfhandeling.Web.Server.Authentication;
using InterneTaakAfhandeling.Web.Server.Data;
using InterneTaakAfhandeling.Web.Server.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InterneTaakAfhandeling.Web.Server.Features.Kanalen;

[Route("api/kanalen")]
[ApiController]
[Authorize]
public class KanalenController(ApplicationDbContext dbContext) : Controller
{
    [HttpGet]
    [ProducesResponseType(typeof(List<KanalenEntity>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetKanalen()
    {
        return Ok(await dbContext.Kanalen.ToListAsync());
    }
}