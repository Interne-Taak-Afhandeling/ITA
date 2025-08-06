using InterneTaakAfhandeling.Web.Server.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InterneTaakAfhandeling.Web.Server.Features.Kanalen;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class KanalenController(IKanalenService kanalenService) : Controller
{
    [HttpGet]
    [ProducesResponseType(typeof(List<KanalenEntity>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetKanalen()
    {
        return Ok(await kanalenService.GetKanalen());
    }

    [HttpPost] 
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(KanalenEntity), StatusCodes.Status201Created)]

    public async Task<IActionResult> CreateKanalen([FromBody] CreateKanalenModel request)
    {
        return !ModelState.IsValid ? ValidationProblem(ModelState) : StatusCode(201,await kanalenService.CreateKanalen(request.Naam));
    }
}