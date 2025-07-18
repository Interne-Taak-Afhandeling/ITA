using InterneTaakAfhandeling.Web.Server.Authentication;
using InterneTaakAfhandeling.Web.Server.Features.MyInterneTakenOverview;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InterneTaakAfhandeling.Web.Server.Features.MyInterneTaken
{
    [Route("api/internetaken")]
    [ApiController]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public class MyInterneTakenOverviewController(IMyInterneTakenOverviewService userService, ITAUser user) : Controller
    {
           private readonly IMyInterneTakenOverviewService _userService = userService ?? throw new ArgumentNullException(nameof(userService));


        [ProducesResponseType(typeof(List<InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models.Internetaak>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [HttpGet("assigned-to-me")]
        public async Task<IActionResult> GetInternetaken([FromQuery] bool afgerond   )
        {
            var result = await _userService.GetInterneTakenByAssignedUser(user, afgerond);
            return Ok(result);
        } 
      
    }
}
