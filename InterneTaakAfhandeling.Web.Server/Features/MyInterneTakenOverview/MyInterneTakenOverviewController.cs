using InterneTaakAfhandeling.Web.Server.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InterneTaakAfhandeling.Web.Server.Features.MyInterneTakenOverview
{
    [Route("api/internetaken")]
    [ApiController]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public class MyInterneTakenOverviewController(IMyInterneTakenOverviewService myInterneTakenOverviewService, ITAUser user) : Controller
    {
        private readonly IMyInterneTakenOverviewService _myInterneTakenOverviewService = myInterneTakenOverviewService ?? throw new ArgumentNullException(nameof(myInterneTakenOverviewService));


        [ProducesResponseType(typeof(List<InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models.Internetaak>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [HttpGet("aan-mij-toegewezen")]
        public async Task<IActionResult> GetInternetaken([FromQuery] bool afgerond)
        {
            var result = await myInterneTakenOverviewService.GetInterneTakenByAssignedUser(user, afgerond);
            return Ok(result);
        }

    }
}