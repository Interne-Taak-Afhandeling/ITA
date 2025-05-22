using InterneTaakAfhandeling.Web.Server.Authentication;
using InterneTaakAfhandeling.Web.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


namespace InterneTaakAfhandeling.Web.Server.Features.AssignInternetaken
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public class AssignInternetakenController(IAssignInternetakenService AssignInternetakenService, ITAUser user) : Controller
    { 

        private readonly IAssignInternetakenService _AssignInternetakenService = AssignInternetakenService;
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpPost("{internetakenId}/to-self")]
        public async Task<IActionResult> AssignInternetakenAsync([FromRoute] string internetakenId)
        {
            return Ok(await _AssignInternetakenService.ToSelfAsync(internetakenId, user));
        }
    }
}
