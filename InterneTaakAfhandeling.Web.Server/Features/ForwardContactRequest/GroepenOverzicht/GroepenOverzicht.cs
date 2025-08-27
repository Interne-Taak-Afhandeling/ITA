using InterneTaakAfhandeling.Common.Services.ObjectApi;
using InterneTaakAfhandeling.Common.Services.ObjectApi.Models;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using InterneTaakAfhandeling.Web.Server.Authentication;
using InterneTaakAfhandeling.Web.Server.Features.InterneTakenOverzicht;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;

namespace InterneTaakAfhandeling.Web.Server.Features.ForwardContactRequest.GroepenOverzicht
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
                var result = groepen.Select(x => new { x.Naam, x.Uuid }).ToList();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all Groepen");
                return Problem("Er is een fout opgetreden bij het ophalen van de lijst met groepen"); 
            }
        }


        private async Task<IEnumerable<(string Naam, Guid Uuid)>> GetGroepenRecursive(int page)
        {
            var groepen = new List<(string Naam, Guid Uuid)>();

            var result = await objectApiClient.GetGroepen(page);

            if (result == null)
            {
                return [];
            }

            groepen = [.. result.Results.Select(x => (x.Record.Data.Naam, x.Uuid))];

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


