using InterneTaakAfhandeling.Common.Exceptions;
using InterneTaakAfhandeling.Common.Helpers;
using InterneTaakAfhandeling.Common.Services.ObjectApi;
using InterneTaakAfhandeling.Common.Services.ObjectApi.Models;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using InterneTaakAfhandeling.Web.Server.Authentication;

namespace InterneTaakAfhandeling.Web.Server.Guards;

public class ContactverzoekAutorisatieGuardService(
    IObjectApiClient objectApiClient,
    ILogger<ContactverzoekAutorisatieGuardService> logger) : IContactverzoekAutorisatieGuardService
{
    private const string GeenToegangMessage = "Je hebt geen toegang tot dit contactverzoek.";

    public async Task GuardAgainstGeenToegangAsync(Internetaak internetaak, ITAUser user)
    {
        if (user.HasFunctioneelBeheerderAccess)
        {
            return;
        }

        var taakAfdelingenEnGroepen = GetAfdelingEnGroepNamenVanTaak(internetaak);

        // Een taak zonder afdeling/groep-actor biedt geen basis voor toegang onder deze regel;
        // impliciet toestaan zou een makkelijke bypass zijn (taak alleen aan medewerker toewijzen = voor iedereen zichtbaar).
        if (taakAfdelingenEnGroepen.Count == 0)
        {
            logger.LogWarning(
                "Toegang geweigerd: interne taak {InternetaakUuid} heeft geen afdeling/groep-actor toegewezen",
                SecureLogging.SanitizeUuid(internetaak.Uuid));
            throw new GeenToegangException(GeenToegangMessage);
        }

        var medewerkers = await objectApiClient.GetMedewerkersByIdentificatie(user.ObjectregisterMedewerkerId);
        if (medewerkers.Count > 1)
        {
            throw new ConflictException(
                $"Meerdere medewerkers gevonden met dezelfde identificatie {SecureLogging.SanitizeAndTruncate(user.ObjectregisterMedewerkerId, 5)}");
        }

        var medewerker = medewerkers.Single();
        var afdelingen = medewerker.Afdelingen ?? Enumerable.Empty<MedewerkerAfdeling>();
        var groepen = medewerker.Groepen ?? Enumerable.Empty<MedewerkerGroep>();
        var gebruikerAfdelingenEnGroepen = afdelingen.Select(a => a.Afdelingnaam)
            .Concat(groepen.Select(g => g.Groepsnaam));

        var heeftToegang = gebruikerAfdelingenEnGroepen.Any(gebruikerNaam =>
            taakAfdelingenEnGroepen.Any(taakNaam =>
                string.Equals(taakNaam.Trim(), gebruikerNaam.Trim(), StringComparison.OrdinalIgnoreCase)));

        if (!heeftToegang)
        {
            logger.LogWarning(
                "Toegang geweigerd: gebruiker behoort niet tot de afdeling/groep van interne taak {InternetaakUuid}",
                SecureLogging.SanitizeUuid(internetaak.Uuid));
            throw new GeenToegangException(GeenToegangMessage);
        }
    }

    private static List<string> GetAfdelingEnGroepNamenVanTaak(Internetaak internetaak)
    {
        var actoren = new List<Actor>();
        if (internetaak.ToegewezenAanActor != null)
        {
            actoren.Add(internetaak.ToegewezenAanActor);
        }
        if (internetaak.ToegewezenAanActoren != null)
        {
            actoren.AddRange(internetaak.ToegewezenAanActoren);
        }

        return actoren
            .DistinctBy(a => a.Uuid)
            .Where(a => a.Actoridentificator?.CodeObjecttype is KnownAfdelingIdentificators.CodeObjecttypeAfdeling
                or KnownGroepIdentificators.CodeObjecttypeGroep)
            .Select(a => a.Naam)
            .Where(naam => !string.IsNullOrWhiteSpace(naam))
            .Select(naam => naam!)
            .ToList();
    }
}
