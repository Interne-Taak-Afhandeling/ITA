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

        // Endpoint voor contactmomenten van een internetaak
        [ProducesResponseType(typeof(ContactmomentenResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [HttpGet("{contactverzoekId}/contactmomenten")]
        public async Task<IActionResult> GetContactmomenten(string contactverzoekId)
        {
            // Haal de internetaak op
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

            // Roep de andere endpoint aan om de klantcontact keten op te halen
            var ketenResponse = await GetKlantcontactKetenInternal(aanleidinggevendKlantcontactId);

            return Ok(ketenResponse);
        }

        // Endpoint voor klantcontact keten op basis van een aanleidinggevend klantcontact
        [ProducesResponseType(typeof(ContactmomentenResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [HttpGet("klantcontacten/{aanleidinggevendKlantcontactId}/contactmomenten")]
        public async Task<IActionResult> GetKlantcontactKeten(string aanleidinggevendKlantcontactId)
        {
            var response = await GetKlantcontactKetenInternal(aanleidinggevendKlantcontactId);
            return Ok(response);
        }

        // Interne methode voor het ophalen van de klantcontact keten
        private async Task<ContactmomentenResponse> GetKlantcontactKetenInternal(string aanleidinggevendKlantcontactUuid)
        {
            // We beginnen met een lege lijst van klantcontacten
            var klantcontactenInKeten = new List<Klantcontact>();
            var verwerkte_uuids = new HashSet<string>();

            // We halen het startklantcontact op
            var aanleidinggevendKlantcontact = await _openKlantApiClient.GetKlantcontactAsync(aanleidinggevendKlantcontactUuid);

            if (aanleidinggevendKlantcontact == null)
            {
                throw new Exception("Klantcontact niet gevonden");
            }

            // We zoeken de keten van klantcontacten, uitgaande van het aanleidinggevende klantcontact
            var ketenVolgorde = await BouwKlantcontactKeten(aanleidinggevendKlantcontact);

            // Nu hebben we de keten in chronologische volgorde (eerst aanleidinggevend, laatst het nieuwste)
            // We draaien de volgorde om zodat de nieuwste bovenaan staat
            ketenVolgorde.Reverse();

            // Het laatste klantcontact is nu de eerste in de lijst (na het omdraaien)
            var laatsteKlantcontactUuid = ketenVolgorde.Count > 0 ? ketenVolgorde[0].Uuid : aanleidinggevendKlantcontactUuid;

            // Zet om naar ContactmomentResponse objecten
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

        // Helper methode voor datumformattering die rekening houdt met eventuele null waarden
        private string FormateerDatum(DateTimeOffset datum)
        {
            // Controleer op de standaardwaarde die gebruikt wordt voor niet-ingevulde datums
            if (datum.Year == 1 && datum.Month == 1 && datum.Day == 1)
            {
                return "Onbekend";
            }

            return datum.ToString("dd-MM-yyyy");
        }

        // Methode om de keten van klantcontacten op te bouwen
        private async Task<List<Klantcontact>> BouwKlantcontactKeten(Klantcontact aanleidinggevendKlantcontact)
        {
            var keten = new List<Klantcontact>();
            var verwerkte_uuids = new HashSet<string>();

            // Voeg aanleidinggevend klantcontact toe aan de keten
            keten.Add(aanleidinggevendKlantcontact);
            verwerkte_uuids.Add(aanleidinggevendKlantcontact.Uuid);

            // Begin bij het aanleidinggevend klantcontact
            var huidigKlantcontact = aanleidinggevendKlantcontact;

            // Blijf zoeken naar nieuwere klantcontacten zolang we ze kunnen vinden
            bool nieuwKlantcontactGevonden;
            do
            {
                nieuwKlantcontactGevonden = false;

                try
                {
                    // Zoek klantcontacten die verwijzen naar het huidige klantcontact
                    var nieuwereKlantcontacten = await _openKlantApiClient.GetKlantcontactenVerwijzendNaarKlantcontact(huidigKlantcontact.Uuid);

                    foreach (var nieuwKlantcontact in nieuwereKlantcontacten)
                    {
                        if (!verwerkte_uuids.Contains(nieuwKlantcontact.Uuid))
                        {
                            // Voeg nieuw klantcontact toe aan de keten
                            keten.Add(nieuwKlantcontact);
                            verwerkte_uuids.Add(nieuwKlantcontact.Uuid);

                            // Update huidige klantcontact zodat we verder zoeken vanaf dit punt
                            huidigKlantcontact = nieuwKlantcontact;
                            nieuwKlantcontactGevonden = true;

                            // We gaan uit van één keten, dus als we een nieuwer klantcontact hebben gevonden,
                            // stoppen we met zoeken naar alternatieven op hetzelfde niveau
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Bij fout stoppen we de lus
                    nieuwKlantcontactGevonden = false;
                }
            } while (nieuwKlantcontactGevonden);

            return keten;
        }

        // Response types
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