using InterneTaakAfhandeling.Common.Services.ObjectApi;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using InterneTaakAfhandeling.Web.Server.Authentication;
using InterneTaakAfhandeling.Web.Server.Services;

namespace InterneTaakAfhandeling.Web.Server.Features.MyInterneTakenOverview;

public interface IMyInterneTakenOverviewService
{
    Task<MyInterneTakenResponse> GetMyInterneTakenOverviewAsync(MyInterneTakenQueryParameters queryParameters);
    Task<MedewerkerResponse?> GetUserGropAndAfdeling(ITAUser user);
    Task<IReadOnlyList<Internetaak>> GetMyInterneTaken(ITAUser user);
}


public class MyInterneTakenOverviewService : IMyInterneTakenOverviewService
{
    private readonly IOpenKlantApiClient _openKlantApiClient;
    private readonly IObjectApiClient _objectApiClient;
    private readonly ILogger<InterneTakenOverviewService> _logger;

    public MyInterneTakenOverviewService(
        IOpenKlantApiClient openKlantApiClient,
        IObjectApiClient objectApiClient,
        ILogger<InterneTakenOverviewService> logger)
    {
        _openKlantApiClient = openKlantApiClient ?? throw new ArgumentNullException(nameof(openKlantApiClient));
        _objectApiClient = objectApiClient ?? throw new ArgumentNullException(nameof(objectApiClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<MyInterneTakenResponse> GetMyInterneTakenOverviewAsync(
        MyInterneTakenQueryParameters queryParameters)
    {
        var page = queryParameters.GetValidatedPage();
        var pageSize = queryParameters.GetValidatedPageSize();

        var query = new InterneTaakQuery
        {
            Page = page,
            PageSize = pageSize
        };
        if (!string.IsNullOrEmpty(queryParameters.Naam))
        {
            query.Actoren__Naam = queryParameters.Naam;
        }
        query.Status = queryParameters.Status switch
        {
            Status.TeVerwerken => InterneTaakStatus.TeVerwerken,
            Status.Verwerkt => InterneTaakStatus.Verwerkt,
            _ => null
        };

        var internetakenResponse = await _openKlantApiClient.GetAllInternetakenAsync(query);

        var overviewItemTasks = internetakenResponse.Results
            .Select(internetaak => MapInternetaakToOverviewItemAsync(internetaak))
            .ToList();

        var overviewItems = (await Task.WhenAll(overviewItemTasks))
            .OrderByDescending(x => x.ToegewezenOp)
            .ToList();

        return new MyInterneTakenResponse()
        {
            Count = internetakenResponse.Count,
            Next = internetakenResponse.Next,
            Previous = internetakenResponse.Previous,
            Results = overviewItems
        };
    }

    private async Task<MyInterneTaakItem> MapInternetaakToOverviewItemAsync(Internetaak internetaak)
    {
        var item = new MyInterneTaakItem()
        {
            Uuid = internetaak.Uuid,
            Nummer = internetaak.Nummer ?? string.Empty,
            GevraagdeHandeling = internetaak.GevraagdeHandeling ?? string.Empty,
            Status = internetaak.Status ?? string.Empty,
            ToegewezenOp = internetaak.ToegewezenOp ?? DateTimeOffset.MinValue,
            AfgehandeldOp = internetaak.AfgehandeldOp
        };

        await LoadKlantcontactInfoAsync(internetaak, item);
        await LoadActorInfoAsync(internetaak, item);

        return item;
    }
    public async Task<IReadOnlyList<Internetaak>> GetMyInterneTaken(ITAUser user)
    {
        var actors = await GetCurrentActors(user);
        var internetakenTasks = actors.Select(a => _openKlantApiClient.GetOutstandingInternetakenByToegewezenAanActor(a.Uuid));

        var results = await Task.WhenAll(internetakenTasks);

        return [.. results.SelectMany(x => x).OrderByDescending(x => x.ToegewezenOp)];
    }

    private async Task LoadKlantcontactInfoAsync(Internetaak internetaak, MyInterneTaakItem item)
    {
        if (internetaak.AanleidinggevendKlantcontact == null)
            return;

        try
        {
            var klantcontact = await GetKlantcontactAsync(internetaak.AanleidinggevendKlantcontact.Uuid);
            if (klantcontact != null)
            {
                item.Onderwerp = klantcontact.Onderwerp;
                item.ContactDatum = klantcontact.PlaatsgevondenOp;
                item.KlantNaam = ExtractKlantNaamFromKlantcontact(klantcontact);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error loading klantcontact {KlantcontactUuid} for internetaak {InternetaakUuid}",
                internetaak.AanleidinggevendKlantcontact.Uuid, internetaak.Uuid);
        }
    }

    private async Task LoadActorInfoAsync(Internetaak internetaak, MyInterneTaakItem item)
    {
        if (internetaak.ToegewezenAanActoren?.Any() != true)
            return;

        var actorTasks = internetaak.ToegewezenAanActoren
            .Where(actorRef => !string.IsNullOrEmpty(actorRef.Uuid))
            .Select(actorRef => GetActorAsync(actorRef.Uuid));

        var actors = await Task.WhenAll(actorTasks);

        var medewerkerActor = actors.FirstOrDefault(a => a?.SoortActor == SoortActor.medewerker);
        if (medewerkerActor != null)
        {
            item.BehandelaarNaam = medewerkerActor.Naam;
        }

        var afdelingActors = actors
            .Where(a => a?.SoortActor != SoortActor.medewerker && !string.IsNullOrEmpty(a?.Naam))
            .Select(a => a!.Naam)
            .ToList();

        if (afdelingActors.Any())
        {
            item.AfdelingNaam = string.Join(", ", afdelingActors);
        }
    }

    private async Task<Klantcontact?> GetKlantcontactAsync(string uuid)
    {
        try
        {
            _logger.LogInformation("Klantcontact {Uuid} fetched directly from API", uuid);
            return await _openKlantApiClient.GetKlantcontactAsync(uuid);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch klantcontact {Uuid}", uuid);
            return null;
        }
    }

    private async Task<Actor?> GetActorAsync(string uuid)
    {
        try
        {
            _logger.LogInformation("Actor {Uuid} fetched directly from API", uuid);
            return await _openKlantApiClient.GetActorAsync(uuid);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch actor {Uuid}", uuid);
            return null;
        }
    }

    private static string? ExtractKlantNaamFromKlantcontact(Klantcontact klantcontact)
    {
        return klantcontact.Expand?.HadBetrokkenen?
            .Select(b => b.VolledigeNaam ?? b.Organisatienaam)
            .FirstOrDefault(naam => !string.IsNullOrEmpty(naam));
    }

    public async Task<MedewerkerResponse?> GetUserGropAndAfdeling(ITAUser user)
    {
        var actorIds = await GetCurrentActors(user);
        var results = await Task.WhenAll(
            actorIds.Select(a => _objectApiClient.GetObjectsByIdentificatie(a.Actoridentificator?.ObjectId)));
        var res = results.SelectMany(x => x).ToList();
        return
            new MedewerkerResponse()
            {
                Afdelingen = (res.First().Data.Afdelingen ?? []).Select(x => x.Afdelingnaam).ToList(),
                Groepen = (res.First().Data.Groepen ?? []).Select(x => x.Groepsnaam).ToList()
            };

    }


    private async Task<IReadOnlyList<Actor>> GetCurrentActors(ITAUser user)
    {
        var actorIds = new List<Actor>();

        if (!string.IsNullOrWhiteSpace(user.Email))
        {
            var fromEntra = await _openKlantApiClient.QueryActorAsync(new ActorQuery
            {
                ActoridentificatorCodeObjecttype = KnownMedewerkerIdentificators.EmailFromEntraId.CodeObjecttype,
                ActoridentificatorCodeRegister = KnownMedewerkerIdentificators.EmailFromEntraId.CodeRegister,
                ActoridentificatorCodeSoortObjectId =
                    KnownMedewerkerIdentificators.EmailFromEntraId.CodeSoortObjectId,
                IndicatieActief = true,
                SoortActor = SoortActor.medewerker,
                ActoridentificatorObjectId = user.Email
            });

            if (fromEntra != null)
            {
                actorIds.Add(fromEntra);
            }

            var fromHandmatig = await _openKlantApiClient.QueryActorAsync(new ActorQuery
            {
                ActoridentificatorCodeObjecttype = KnownMedewerkerIdentificators.EmailHandmatig.CodeObjecttype,
                ActoridentificatorCodeRegister = KnownMedewerkerIdentificators.EmailHandmatig.CodeRegister,
                ActoridentificatorCodeSoortObjectId =
                    KnownMedewerkerIdentificators.EmailHandmatig.CodeSoortObjectId,
                IndicatieActief = true,
                SoortActor = SoortActor.medewerker,
                ActoridentificatorObjectId = user.Email
            });

            if (fromHandmatig != null)
            {
                actorIds.Add(fromHandmatig);
            }
        }

        if (string.IsNullOrWhiteSpace(user.ObjectregisterMedewerkerId)) return actorIds;
        var fromObjecten = await _openKlantApiClient.QueryActorAsync(new ActorQuery
        {
            ActoridentificatorCodeObjecttype = KnownMedewerkerIdentificators.ObjectregisterId.CodeObjecttype,
            ActoridentificatorCodeRegister = KnownMedewerkerIdentificators.ObjectregisterId.CodeRegister,
            ActoridentificatorCodeSoortObjectId =
                KnownMedewerkerIdentificators.ObjectregisterId.CodeSoortObjectId,
            IndicatieActief = true,
            SoortActor = SoortActor.medewerker,
            ActoridentificatorObjectId = user.ObjectregisterMedewerkerId
        });

        if (fromObjecten != null)
        {
            actorIds.Add(fromObjecten);
        }

        return actorIds;
    }



}
