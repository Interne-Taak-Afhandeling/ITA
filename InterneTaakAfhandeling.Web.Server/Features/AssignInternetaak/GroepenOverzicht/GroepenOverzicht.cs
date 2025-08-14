using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.Web.Server.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InterneTaakAfhandeling.Web.Server.Features.InterneTakenOverzicht;
using InterneTaakAfhandeling.Common.Services.ObjectApi;
using InterneTaakAfhandeling.Common.Services.ObjectApi.Models;
using Microsoft.Extensions.Options;

namespace InterneTaakAfhandeling.Web.Server.Features.AssignInternetaak.GroepenOverzicht
{


    [Route("api")]
    [ApiController]
    [Authorize(Policy = ITAPolicy.Name)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public class GroepenOverzichtController(ILogger<GroepenOverzichtController> logger, IObjectApiClient objectApiClient)
    : Controller
    {

        private readonly ILogger<GroepenOverzichtController> _logger =  logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IObjectApiClient objectApiClient = objectApiClient ?? throw new ArgumentNullException(nameof(objectApiClient));

        [ProducesResponseType(typeof(InterneTakenOverzichtResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [HttpGet("groepen")]
        public async Task<IActionResult> GetGroepenOverzicht()
        {
            try
            {
                var groepen = await GetGroepenRecursive(1); 
                var result = groepen.Select(x => new { Naam = x.Item1, Uuid = x.Item2 }).ToList();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all Groepen");
                return Problem("Er is een fout opgetreden bij het ophalen van de lijst met groepen"); 
            }
        }


        private async Task<IEnumerable<Tuple<string, Guid>>> GetGroepenRecursive(int page)
        {
            var groepen = new List<Tuple<string, Guid>>();

            var result = await objectApiClient.GetGroepen(page);

            if (result == null)
            {
                return [];
            }

            groepen = [.. result.Results.Select(x => new Tuple<string, Guid>(x.Record.Data.Naam, x.Uuid))];

            if (result.Next != null)
            {
                var nextpageResult = await GetGroepenRecursive(page + 1);

                if (nextpageResult != null)
                {
                    groepen.AddRange(nextpageResult);
                }
            }

            return groepen;
        }

    }
}


