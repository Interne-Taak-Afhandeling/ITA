using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using InterneTaakAfhandeling.Web.Server.Authentication;
using InterneTaakAfhandeling.Web.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace InterneTaakAfhandeling.Web.Server.Features.InternetaakContactmomentenController
{
    [Route("api/klantcontacten")]
    [ApiController]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public class InternetaakContactmomentenController(IOpenKlantApiClient openKlantApiClient, IUserService userService, ITAUser user) : Controller
    {
        private readonly IOpenKlantApiClient _openKlantApiClient = openKlantApiClient ?? throw new ArgumentNullException(nameof(openKlantApiClient));
        private readonly IUserService _userService = userService ?? throw new ArgumentNullException(nameof(userService));

        [ProducesResponseType(typeof(ContactmomentenResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [HttpGet("{contactverzoekId}/contactmomenten")]
        public async Task<IActionResult> GetContactmomenten(string contactverzoekId)
        {
            var internetaak = await _openKlantApiClient.GetInternetaak(contactverzoekId);

            if (internetaak == null)
            {
                return NotFound("Internetaak niet gevonden");
            }

            var aanleidinggevendKlantcontactId = internetaak.AanleidinggevendKlantcontact?.Uuid;

            if (string.IsNullOrWhiteSpace(aanleidinggevendKlantcontactId))
            {
                return NotFound("Geen aanleidinggevend klantcontact gevonden voor deze internetaak");
            }

            var ketenResponse = await GetKlantcontactKetenInternal(aanleidinggevendKlantcontactId);

            return Ok(ketenResponse);
        }

        [ProducesResponseType(typeof(ContactmomentenResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [HttpGet("{aanleidinggevendKlantcontactId}/contactketen")]
        public async Task<IActionResult> GetKlantcontactKeten(string aanleidinggevendKlantcontactId)
        {
            var response = await GetKlantcontactKetenInternal(aanleidinggevendKlantcontactId);
            return Ok(response);
        }

        private async Task<ContactmomentenResponse> GetKlantcontactKetenInternal(string aanleidinggevendKlantcontactUuid)
        {
            var klantcontactenInKeten = new List<Klantcontact>();
            var verwerkte_uuids = new HashSet<string>();
            var aanleidinggevendKlantcontact = await _openKlantApiClient.GetKlantcontactAsync(aanleidinggevendKlantcontactUuid);

            if (aanleidinggevendKlantcontact == null)
            {
                throw new Exception("Klantcontact niet gevonden");
            }

            var ketenVolgorde = await BouwKlantcontactKeten(aanleidinggevendKlantcontact);


            ketenVolgorde.Reverse();

            var laatsteKlantcontactUuid = ketenVolgorde.Count > 0 ? ketenVolgorde[0].Uuid : aanleidinggevendKlantcontactUuid;
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