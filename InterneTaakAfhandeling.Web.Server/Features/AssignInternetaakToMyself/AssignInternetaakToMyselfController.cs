using InterneTaakAfhandeling.Web.Server.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace InterneTaakAfhandeling.Web.Server.Features.AssignInternetaakToMyself
{
    [Route("api/internetaken")]
    [ApiController]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public class AssignInternetaakToMyselfController(IAssignInternetaakToMyselfService AssignInternetakenService, ITAUser user) : Controller
    { 

        private readonly IAssignInternetaakToMyselfService _AssignInternetakenService = AssignInternetakenService;
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpPost("{internetakenId}/assign-to-self")]
        public async Task<IActionResult> AssignInternetakenAsync([FromRoute] string internetakenId)
        {
            return Ok(await _AssignInternetakenService.ToSelfAsync(internetakenId, user));
        }
    }
}
