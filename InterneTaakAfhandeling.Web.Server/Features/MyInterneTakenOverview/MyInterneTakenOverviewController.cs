using InterneTaakAfhandeling.Web.Server.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InterneTaakAfhandeling.Web.Server.Features.MyInterneTakenOverview
{
    [Route("api/internetaken")]
    [ApiController]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public class MyInterneTakenOverviewController(IMyInterneTakenOverviewService myInterneTakenService, ILogger<MyInterneTakenOverviewController> logger, ITAUser user) : Controller
    {
        private readonly IMyInterneTakenOverviewService _myInterneTakenService = myInterneTakenService ?? throw new ArgumentNullException(nameof(myInterneTakenService));
        private readonly ILogger<MyInterneTakenOverviewController> _logger = logger;

        [ProducesResponseType(typeof(List<Common.Services.OpenKlantApi.Models.Internetaak>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [HttpGet("aan-mij-toegewezen")]
        public async Task<IActionResult> GetInternetaken([FromQuery] bool afgerond)
        {
            var result = await _myInterneTakenService.GetMyInterneTaken(user, afgerond);

            Console.WriteLine($"Found {result.Count} internetaken for user {user.Email}");
            foreach (var item in result)
            {
                Console.WriteLine($"Internetaak: {item.Uuid}, Status: {item.Status}, Onderwerp: {item.AanleidinggevendKlantcontact?.Onderwerp}");
            }

            var json = System.Text.Json.JsonSerializer.Serialize(result, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true,
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve
            });

            return Ok(result);
        }



        [ProducesResponseType(typeof(MyInterneTakenResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [HttpGet]
        public async Task<IActionResult> GetInterneTaken([FromQuery] MyInterneTakenQueryParameters queryParameters)
        {

            try
            {
                _logger.LogInformation("Fetching interne taken  with page {Page}, pageSize {PageSize}",
                    queryParameters.Page, queryParameters.PageSize);




                var result = await _myInterneTakenService.GetMyInterneTakenOverviewAsync(queryParameters);

                _logger.LogInformation("Successfully fetched {Count} interne taken out of {Total} total",
                    result.Results.Count, result.Count);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching interne taken  with page {Page}, pageSize {PageSize}",
                    queryParameters.Page, queryParameters.PageSize);
                throw;
            }
        }


    }


}

