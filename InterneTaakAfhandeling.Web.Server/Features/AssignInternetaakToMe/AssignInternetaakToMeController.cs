using InterneTaakAfhandeling.Web.Server.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace InterneTaakAfhandeling.Web.Server.Features.AssignInternetaakToMe
{
    [Route("api/internetaken")]
    [ApiController]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public class AssignInternetaakToMeController(IAssignInternetaakToMeService AssignInternetakenService, ITAUser user) : Controller
    { 

        private readonly IAssignInternetaakToMeService _AssignInternetakenService = AssignInternetakenService;
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpPost("{internetakenId}/assign-to-me")]
        public async Task<IActionResult> AssignInternetakenAsync([FromRoute] string internetakenId)
        {
            return Ok(await _AssignInternetakenService.ToSelfAsync(internetakenId, user));
        }
    }
}
