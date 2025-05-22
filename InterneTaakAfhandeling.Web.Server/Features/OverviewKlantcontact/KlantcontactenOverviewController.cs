using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using InterneTaakAfhandeling.Web.Server.Authentication;
using InterneTaakAfhandeling.Web.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InterneTaakAfhandeling.Web.Server.Features.KlantcontactenOverview
{
    [Route("api/klantcontacten-overview")]
    [ApiController]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public class KlantcontactenOverviewController : Controller
    {
        private readonly IKlantcontactService _klantcontactService;
        private readonly ILogger<KlantcontactenOverviewController> _logger;

        public KlantcontactenOverviewController(
            IKlantcontactService klantcontactService,
            ILogger<KlantcontactenOverviewController> logger)
        {
            _klantcontactService = klantcontactService ?? throw new ArgumentNullException(nameof(klantcontactService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [ProducesResponseType(typeof(ContactmomentenResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [HttpGet("{klantcontactId}/contactketen")]
        public async Task<IActionResult> GetKlantcontactKeten(string klantcontactId)
        {
            try
            {
                var response = await GetKlantcontactKetenInternal(klantcontactId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                if (Guid.TryParse(klantcontactId, out Guid parsedKlantcontactId))
                {
                    _logger.LogError(ex, "Error retrieving contact chain for klantcontact {ParsedKlantcontactId}",
                        parsedKlantcontactId);
                }

                return NotFound(new ProblemDetails
                {
                    Title = "Contact chain not found",
                    Detail = ex.Message,
                    Status = StatusCodes.Status404NotFound
                });
            }
        }

        private async Task<ContactmomentenResponse> GetKlantcontactKetenInternal(string klantcontactUuid)
        {
            var ketenVolgorde = await _klantcontactService.BouwKlantcontactKetenAsync(klantcontactUuid);
            ketenVolgorde.Reverse();  // Nieuwste bovenaan

            var laatsteKlantcontactUuid = ketenVolgorde.Count > 0 ? ketenVolgorde[0].Uuid : klantcontactUuid;
            var contactmomenten = ketenVolgorde.Select(k => new ContactmomentResponse(
                ContactGelukt: k.IndicatieContactGelukt ?? false,
                Tekst: k.Inhoud ?? string.Empty,
                Datum: k.PlaatsgevondenOp,
                Medewerker: k.HadBetrokkenActoren?.FirstOrDefault()?.Naam ?? "Onbekend",
                Kanaal: k.Kanaal ?? "Onbekend"
            )).ToList();

            return new ContactmomentenResponse(
                Contactmomenten: contactmomenten,
                LaatsteBekendKlantcontactUuid: laatsteKlantcontactUuid
            );
        }

        public record ContactmomentResponse(
            bool ContactGelukt,
            string Tekst,
            DateTimeOffset Datum,
            string Medewerker,
            string Kanaal
        );

        public record ContactmomentenResponse(
            List<ContactmomentResponse> Contactmomenten,
            string LaatsteBekendKlantcontactUuid
        );
    }
}