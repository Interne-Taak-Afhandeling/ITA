using InterneTaakAfhandeling.Common.Exceptions;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using InterneTaakAfhandeling.Web.Server.Features.InterneTaak;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InterneTaakAfhandeling.Web.Server.Features.Internetaken
{
    [Route("api/internetaken")]
    [ApiController]
    [Authorize]
    public class InternetaakDetailsController(IInternetaakService internetakenService) : Controller
    {

        private readonly IInternetaakService _internetakenService = internetakenService;

        [ProducesResponseType(typeof(InterneTaakDetailsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [HttpGet("{internetaakNummer}")]
        public async Task<IActionResult> Get([FromRoute] string internetaakNummer)
        {
            var interneTaakQueryParameters = new InterneTaakQuery { Nummer = internetaakNummer };
            var internetaak = await _internetakenService.Get(interneTaakQueryParameters);

            if (internetaak == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Interne taak niet gevonden",
                    Detail = $"Geen interne taak gevonden met identificatie: {interneTaakQueryParameters.Nummer}",
                    Status = StatusCodes.Status404NotFound
                });
            }

            if (internetaak.AanleidinggevendKlantcontact?.Nummer == null)
            {
                return Conflict(new ProblemDetails
                {
                    Title = "Contactmoment-nummer ontbreekt",
                    Detail = "Het aanleidinggevend klantcontact heeft geen nummer. Dit is een foutsituatie.",
                    Status = StatusCodes.Status409Conflict
                });
            }

            return Ok(ToResponse(internetaak));
        }

        [ProducesResponseType(typeof(InterneTaakDetailsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [HttpGet("by-klantcontact/{nummer}")]
        public async Task<IActionResult> GetByKlantcontactNummer([FromRoute] string nummer)
        {
            Internetaak? internetaak;

            try
            {
                internetaak = await _internetakenService.GetByKlantcontactNummer(nummer);
            }
            catch (ConflictException ex)
            {
                return Conflict(new ProblemDetails
                {
                    Title = "Meerdere interne taken gevonden. Dit is een foutsituatie.",
                    Detail = ex.Message,
                    Status = StatusCodes.Status409Conflict
                });
            }

            if (internetaak == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Interne taak niet gevonden",
                    Detail = $"Geen interne taak gevonden voor klantcontact met nummer: {nummer}",
                    Status = StatusCodes.Status404NotFound
                });
            }

            if (internetaak.AanleidinggevendKlantcontact?.Nummer == null)
            {
                return Conflict(new ProblemDetails
                {
                    Title = "Contactmoment-nummer ontbreekt",
                    Detail = "Het aanleidinggevend klantcontact heeft geen nummer. Dit is een foutsituatie.",
                    Status = StatusCodes.Status409Conflict
                });
            }

            return Ok(ToResponse(internetaak));
        }

        private static InterneTaakDetailsResponse ToResponse(Internetaak internetaak)
        {
            var actoren = internetaak.ToegewezenAanActoren ?? [];
            var medewerker = actoren.FirstOrDefault(a => a.SoortActor == SoortActor.medewerker);
            var oe = actoren.FirstOrDefault(a => a.SoortActor == SoortActor.organisatorische_eenheid);
            var oeType = oe?.Actoridentificator?.CodeObjecttype switch
            {
                KnownAfdelingIdentificators.CodeObjecttypeAfdeling => "Afdeling",
                KnownGroepIdentificators.CodeObjecttypeGroep => "Groep",
                _ => oe != null ? "Organisatorische eenheid" : null
            };

            var klantcontact = internetaak.AanleidinggevendKlantcontact;
            var klantNaam = ResolveKlantNaam(klantcontact);
            var telefoonnummers = ResolveTelefoonnummers(klantcontact);

            return new InterneTaakDetailsResponse
            {
                Uuid = internetaak.Uuid,
                Nummer = internetaak.Nummer,
                GevraagdeHandeling = internetaak.GevraagdeHandeling,
                AanleidinggevendKlantcontact = klantcontact,
                ToegewezenAanActoren = internetaak.ToegewezenAanActoren,
                Toelichting = internetaak.Toelichting,
                Status = internetaak.Status,
                ToegewezenOp = internetaak.ToegewezenOp,
                AfgehandeldOp = internetaak.AfgehandeldOp,
                Zaak = internetaak.Zaak,
                BehandelaarNaam = medewerker?.Naam,
                OrganisatorischeEenheidNaam = oe?.Naam,
                OrganisatorischeEenheidType = oeType,
                KlantNaam = klantNaam,
                Organisatienaam = ResolveOrganisatienaam(klantcontact, klantNaam),
                Email = ResolveEmail(klantcontact),
                Telefoonnummer1 = telefoonnummers.Count > 0 ? telefoonnummers[0] : null,
                Telefoonnummer2 = telefoonnummers.Count > 1 ? telefoonnummers[1] : null,
                PlaatsgevondenOp = klantcontact?.PlaatsgevondenOp,
                Kanaal = klantcontact?.Kanaal,
                AangemaaktDoor = klantcontact?.HadBetrokkenActoren?
                    .Select(a => a.Naam)
                    .FirstOrDefault(naam => !string.IsNullOrEmpty(naam))
            };
        }

        private static string? ResolveKlantNaam(Klantcontact? klantcontact) =>
            klantcontact?.Expand?.HadBetrokkenen?
                .Select(b => b.VolledigeNaam ?? b.Organisatienaam)
                .FirstOrDefault(naam => !string.IsNullOrEmpty(naam));

        private static string? ResolveOrganisatienaam(Klantcontact? klantcontact, string? klantNaam) =>
            klantcontact?.Expand?.HadBetrokkenen?
                .Select(b => b.Organisatienaam)
                .FirstOrDefault(naam => !string.IsNullOrEmpty(naam) && naam != klantNaam);

        private static string? ResolveEmail(Klantcontact? klantcontact) =>
            klantcontact?.Expand?.HadBetrokkenen?[0]?.Expand?.DigitaleAdressen?
                .Where(a => a.SoortDigitaalAdres == "email" && !string.IsNullOrEmpty(a.Adres))
                .Select(a => a.Adres)
                .FirstOrDefault();

        private static List<TelefoonnummerItem> ResolveTelefoonnummers(Klantcontact? klantcontact) =>
            klantcontact?.Expand?.HadBetrokkenen?[0]?.Expand?.DigitaleAdressen?
                .Where(a => a.SoortDigitaalAdres == "telefoonnummer" && !string.IsNullOrEmpty(a.Adres))
                .Select((a, i) => new TelefoonnummerItem
                {
                    Adres = a.Adres!,
                    Omschrijving = PascalCase(a.Omschrijving) ?? $"Telefoonnummer {i + 1}"
                })
                .ToList() ?? [];

        private static string? PascalCase(string? s) =>
            string.IsNullOrEmpty(s) ? s : char.ToUpper(s[0]) + s[1..];

    }
}
