using InterneTaakAfhandeling.Common.Exceptions;
using InterneTaakAfhandeling.Common.Helpers;
using InterneTaakAfhandeling.Common.ApiClients.ObjectApi;
using InterneTaakAfhandeling.Common.ApiClients.ObjectApi.Models;
using InterneTaakAfhandeling.Common.ApiClients.OpenKlantApi;
using InterneTaakAfhandeling.Common.ApiClients.OpenKlantApi.Models;
using InterneTaakAfhandeling.Common.Services.Actors;
using InterneTaakAfhandeling.Common.Services.Internetaken;
using InterneTaakAfhandeling.Web.Server.Authentication;

namespace InterneTaakAfhandeling.Web.Server.Guards;

public class ContactverzoekAutorisatieGuardService(
    IObjectApiClient objectApiClient,
    IInternetaakService internetaakService,
    IActorService actorService,
    ILogger<ContactverzoekAutorisatieGuardService> logger) : IContactverzoekAutorisatieGuardService
{
    private const string GeenToegangMessage = "Je hebt geen toegang tot dit contactverzoek.";
    private const string GeenToegangAfdelingOfGroepMessage = "Je hebt geen toegang tot deze afdeling of groep.";

    public async Task GuardAgainstGeenToegangAsync(Guid internetaakId, ITAUser user)
    {
        if (user.HasFunctioneelBeheerderAccess)
        {
            return;
        }

        // GetInternetaakByIdAsync only returns actor references (uuid)
        var internetaak = await internetaakService.GetInternetaakByIdAsync(internetaakId);
        var actoren = await Task.WhenAll(
            (internetaak.ToegewezenAanActoren ?? [])
            .Where(a => !string.IsNullOrEmpty(a.Uuid))
            .Select(a => actorService.GetActorAsync(a.Uuid)));
        internetaak.ToegewezenAanActoren = [.. actoren];

        await GuardAgainstGeenToegangAsync(internetaak, user);
    }

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

        var gebruikerAfdelingenEnGroepen = await GetAfdelingEnGroepNamenVanGebruikerAsync(user);

        if (!HeeftOverlap(gebruikerAfdelingenEnGroepen, taakAfdelingenEnGroepen))
        {
            logger.LogWarning(
                "Toegang geweigerd: gebruiker behoort niet tot de afdeling/groep van interne taak {InternetaakUuid}",
                SecureLogging.SanitizeUuid(internetaak.Uuid));
            throw new GeenToegangException(GeenToegangMessage);
        }
    }

    public async Task GuardAgainstGeenToegangTotAfdelingOfGroepAsync(string afdelingOfGroepNaam, ITAUser user)
    {
        if (user.HasFunctioneelBeheerderAccess)
        {
            return;
        }

        var gebruikerAfdelingenEnGroepen = await GetAfdelingEnGroepNamenVanGebruikerAsync(user);

        if (string.IsNullOrWhiteSpace(afdelingOfGroepNaam) || !HeeftOverlap(gebruikerAfdelingenEnGroepen, [afdelingOfGroepNaam]))
        {
            logger.LogWarning("Toegang geweigerd: gebruiker behoort niet tot de gevraagde afdeling/groep");
            throw new GeenToegangException(GeenToegangAfdelingOfGroepMessage);
        }
    }

    private async Task<List<string>> GetAfdelingEnGroepNamenVanGebruikerAsync(ITAUser user)
    {
        var medewerkers = await objectApiClient.GetMedewerkersByIdentificatie(user.ObjectregisterMedewerkerId);
        if (medewerkers.Count > 1)
        {
            throw new ConflictException(
                $"Meerdere medewerkers gevonden met dezelfde identificatie {SecureLogging.SanitizeAndTruncate(user.ObjectregisterMedewerkerId, 5)}");
        }

        // A user that is not registered in the Objectregister belongs to no afdeling/groep: return 403
        // instead of failing with an unhandled exception (500).
        if (medewerkers.Count == 0)
        {
            logger.LogWarning("Geen medewerker gevonden in het objectenregister voor de ingelogde gebruiker");
            return [];
        }

        var medewerker = medewerkers.Single();
        var afdelingen = medewerker.Afdelingen ?? Enumerable.Empty<MedewerkerAfdeling>();
        var groepen = medewerker.Groepen ?? Enumerable.Empty<MedewerkerGroep>();

        return afdelingen.Select(a => a.Afdelingnaam)
            .Concat(groepen.Select(g => g.Groepsnaam))
            .ToList();
    }

    private static bool HeeftOverlap(IEnumerable<string> gebruikerNamen, IReadOnlyCollection<string> taakNamen) =>
        gebruikerNamen.Any(gebruikerNaam =>
            taakNamen.Any(taakNaam =>
                string.Equals(taakNaam.Trim(), gebruikerNaam.Trim(), StringComparison.OrdinalIgnoreCase)));

    private static List<string> GetAfdelingEnGroepNamenVanTaak(Internetaak internetaak)
    {
        // Only ToegewezenAanActoren: ToegewezenAanActor is deprecated in OpenKlant and is not loaded (uuid only).
        var actoren = new List<Actor>();
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
