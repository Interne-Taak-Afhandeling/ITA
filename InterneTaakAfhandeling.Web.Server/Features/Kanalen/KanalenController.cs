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

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(KanalenEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> GetKanalenById(string id)
    {
        return Ok(await kanalenService.GetKanalenById(id));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(KanalenEntity), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateKanalen([FromBody] KanalenModel request)
    {
        return !ModelState.IsValid
            ? ValidationProblem(ModelState)
            : StatusCode(201, await kanalenService.CreateKanalen(request.Naam));
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(KanalenEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EditKanalen(string id, [FromBody] KanalenModel request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var updated = await kanalenService.EditKanalen(id, request.Naam);
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteKanalen(string id)
    {
        await kanalenService.DeleteKanalen(id);
        return NoContent();
    }
}