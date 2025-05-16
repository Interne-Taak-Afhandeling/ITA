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
        private readonly IOpenKlantApiClient _openKlantApiClient;
        private readonly ILogger<KlantcontactenOverviewController> _logger;

        public KlantcontactenOverviewController(
            IOpenKlantApiClient openKlantApiClient,
            ILogger<KlantcontactenOverviewController> logger)
        {
            _openKlantApiClient = openKlantApiClient ?? throw new ArgumentNullException(nameof(openKlantApiClient));
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
                    _logger.LogError(ex, $"Error retrieving contact chain for klantcontact {parsedKlantcontactId}");
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
            var aanleidinggevendKlantcontact = await _openKlantApiClient.GetKlantcontactAsync(klantcontactUuid);

            if (aanleidinggevendKlantcontact == null)
            {
                throw new Exception($"Klantcontact met UUID {klantcontactUuid} niet gevonden");
            }

            var ketenVolgorde = await BouwKlantcontactKeten(aanleidinggevendKlantcontact);
            ketenVolgorde.Reverse();  // Nieuwste bovenaan

            var laatsteKlantcontactUuid = ketenVolgorde.Count > 0 ? ketenVolgorde[0].Uuid : klantcontactUuid;
            var contactmomenten = ketenVolgorde.Select(k => new ContactmomentResponse(
                ContactGelukt: k.IndicatieContactGelukt ?? false,
                Tekst: k.Inhoud ?? string.Empty,
                Datum: FormateerDatum(k.PlaatsgevondenOp),
                Medewerker: k.HadBetrokkenActoren.FirstOrDefault()?.Naam ?? "Onbekend",
                Kanaal: k.Kanaal ?? "Onbekend"
            )).ToList();

            return new ContactmomentenResponse(
                Contactmomenten: contactmomenten,
                LaatsteBekendKlantcontactUuid: laatsteKlantcontactUuid
            );
        }

        private string FormateerDatum(DateTimeOffset datum)
        {
            if (datum.Year == 1 && datum.Month == 1 && datum.Day == 1)
            {
                return "Onbekend";
            }

            return datum.ToString("dd-MM-yyyy");
        }

        private async Task<List<Klantcontact>> BouwKlantcontactKeten(Klantcontact aanleidinggevendKlantcontact)
        {
            var keten = new List<Klantcontact>();
            var verwerkte_uuids = new HashSet<string>();

            keten.Add(aanleidinggevendKlantcontact);
            verwerkte_uuids.Add(aanleidinggevendKlantcontact.Uuid);

            await VoegKlantcontactenToeAanKeten(aanleidinggevendKlantcontact.Uuid, keten, verwerkte_uuids);

            return keten;
        }

        private async Task VoegKlantcontactenToeAanKeten(
            string klantcontactUuid,
            List<Klantcontact> keten,
            HashSet<string> verwerkte_uuids)
        {
            var klantcontacten = await _openKlantApiClient.GetKlantcontactenByOnderwerpobjectIdentificatorObjectIdAsync(klantcontactUuid);

            foreach (var klantcontact in klantcontacten)
            {
                if (!verwerkte_uuids.Contains(klantcontact.Uuid))
                {
                    keten.Add(klantcontact);
                    verwerkte_uuids.Add(klantcontact.Uuid);

                    await VoegKlantcontactenToeAanKeten(klantcontact.Uuid, keten, verwerkte_uuids);
                }
            }
        }

        public record ContactmomentResponse(
            bool ContactGelukt,
            string Tekst,
            string Datum,
            string Medewerker,
            string Kanaal
        );

        public record ContactmomentenResponse(
            List<ContactmomentResponse> Contactmomenten,
            string LaatsteBekendKlantcontactUuid
        );
    }
}