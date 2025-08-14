using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.Web.Server.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InterneTaakAfhandeling.Web.Server.Features.InterneTakenOverzicht;
using InterneTaakAfhandeling.Common.Services.ObjectApi;
using InterneTaakAfhandeling.Common.Services.ObjectApi.Models;
using Microsoft.Extensions.Options;

namespace InterneTaakAfhandeling.Web.Server.Features.AssignInternetaak.AfdelingenOverzicht
{


    [Route("api")]
    [ApiController]
    [Authorize(Policy = ITAPolicy.Name)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public class AfdelingenOverzichtController(ILogger<AfdelingenOverzichtController> logger, IObjectApiClient objectApiClient)
    : Controller
    {

        private readonly ILogger<AfdelingenOverzichtController> _logger =  logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IObjectApiClient objectApiClient = objectApiClient ?? throw new ArgumentNullException(nameof(objectApiClient));

        [ProducesResponseType(typeof(InterneTakenOverzichtResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [HttpGet("afdelingen")]
        public async Task<IActionResult> GetAfdelingenOverzicht()
        {
            try
            {
                var afdelingen = await GetAfdelingenRecursive(1);
                var result = afdelingen.Select(x => new { Naam = x.Item1, Uuid = x.Item2 }).ToList();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all afdelingen");
                return Problem("Er is een fout opgetreden bij het ophalen van de lijst met afdelingen");
            }
        }

        private async Task<IEnumerable<Tuple<string, Guid>>> GetAfdelingenRecursive(int page)
        {
            // declare afdeling in tuple format <name afdeling, uuid of afdeling in the objecten register>
            var afdelingen = new List<Tuple<string, Guid>>();

            var result = await objectApiClient.GetAfdelingen(page);

            if (result == null)
            {
                return [];
            }

            afdelingen = [.. result.Results.Select(x => new Tuple<string, Guid>(x.Record.Data.Naam, x.Uuid))];

            if (result.Next != null)
            {
                var nextpageResult = await GetAfdelingenRecursive(page+1);

                if(nextpageResult != null)
                {
                    afdelingen.AddRange(nextpageResult);
                }
            }

            return afdelingen;

        }

    }
}


