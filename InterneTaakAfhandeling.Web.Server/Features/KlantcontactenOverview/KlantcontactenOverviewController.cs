using InterneTaakAfhandeling.Common.Services.Klantcontacten;
using InterneTaakAfhandeling.Common.ApiClients.OpenKlantApi;
using InterneTaakAfhandeling.Common.ApiClients.OpenKlantApi.Models;
using InterneTaakAfhandeling.Web.Server.Authentication;
using InterneTaakAfhandeling.Web.Server.Guards;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InterneTaakAfhandeling.Web.Server.Features.KlantcontactenOverview
{
    [Route("api/klantcontacten")] 
    [ApiController]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public class KlantcontactenOverviewController : Controller
    {
        private readonly IKlantcontactService _klantcontactService;
        private readonly IContactverzoekAutorisatieGuardService _contactverzoekAutorisatieGuardService;
        private readonly ITAUser _user;
        private readonly ILogger<KlantcontactenOverviewController> _logger;

        public KlantcontactenOverviewController(
            IKlantcontactService klantcontactService,
            IContactverzoekAutorisatieGuardService contactverzoekAutorisatieGuardService,
            ITAUser user,
            ILogger<KlantcontactenOverviewController> logger)
        {
            _klantcontactService = klantcontactService ?? throw new ArgumentNullException(nameof(klantcontactService));
            _contactverzoekAutorisatieGuardService = contactverzoekAutorisatieGuardService ?? throw new ArgumentNullException(nameof(contactverzoekAutorisatieGuardService));
            _user = user ?? throw new ArgumentNullException(nameof(user));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [ProducesResponseType(typeof(ContactmomentenResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [HttpGet("{klantcontactId}/klantcontacten")]
        public async Task<IActionResult> GetKlantcontactKeten(Guid klantcontactId)
        {
            await _contactverzoekAutorisatieGuardService.GuardAgainstGeenLidVanAfdelingOfGroepVoorKlantcontactAsync(klantcontactId, _user);

            try
            {
                var response = await GetKlantcontactKetenInternal(klantcontactId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving contact chain for klantcontact {ParsedKlantcontactId}",
                    klantcontactId);

                return NotFound(new ProblemDetails
                {
                    Title = "Contact chain not found",
                    Detail = ex.Message,
                    Status = StatusCodes.Status404NotFound
                });
            }
        }

        private async Task<ContactmomentenResponse> GetKlantcontactKetenInternal(Guid klantcontactUuid)
        {
            var ketenVolgorde = await _klantcontactService.BouwKlantcontactKetenAsync(klantcontactUuid);
            ketenVolgorde.Reverse();  // Nieuwste bovenaan

            var laatsteKlantcontactUuid = ketenVolgorde.Count > 0 ? ketenVolgorde[0].Uuid : klantcontactUuid;
            var contactmomenten = ketenVolgorde.Select(k => new ContactmomentResponse(
                Uuid: k.Uuid,
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
            Guid Uuid,
            bool ContactGelukt,
            string Tekst,
            DateTimeOffset Datum,
            string Medewerker,
            string Kanaal
        );

        public record ContactmomentenResponse(
            List<ContactmomentResponse> Contactmomenten,
            Guid LaatsteBekendKlantcontactUuid
        );
    }
}
