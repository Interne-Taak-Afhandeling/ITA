using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using InterneTaakAfhandeling.Common.Services;
using InterneTaakAfhandeling.Web.Server.Authentication;
using InterneTaakAfhandeling.Web.Server.Features.Internetaken;
using InterneTaakAfhandeling.Web.Server.Features.InternetakenOverviews.Shared.Urgentie;

namespace InterneTaakAfhandeling.Web.Server.Features.InternetakenOverviews.MyInternetakenOverview
{
    public interface IMyInterneTakenOverviewService
    {
        Task<IReadOnlyList<MyInterneTaakOverviewItem>> GetInterneTakenByAssignedUser(ITAUser user, bool afgerond);
    }
    public class MyInterneTakenOverviewService(
        IOpenKlantApiClient openKlantApiClient,
        IUrgentieBerekenService urgentieBerekenService,
        ILogger<MyInterneTakenOverviewService> logger) : IMyInterneTakenOverviewService
    {
        private readonly IOpenKlantApiClient _openKlantApiClient = openKlantApiClient;
        private readonly IUrgentieBerekenService _urgentieBerekenService = urgentieBerekenService;
        private readonly ILogger<MyInterneTakenOverviewService> _logger = logger;


        public async Task<IReadOnlyList<MyInterneTaakOverviewItem>> GetInterneTakenByAssignedUser(ITAUser user, bool afgerond)
        {
            var actorIds = await GetActorIds(user);

            var internetakenTasks = actorIds
                .Where(id => Guid.TryParse(id, out _))
                .Select(actorId =>
                {
                    var query = new InterneTaakQuery
                    {
                        ToegewezenAanActor__Uuid = Guid.Parse(actorId),
                        Status = afgerond ? KnownInternetaakStatussen.Verwerkt : KnownInternetaakStatussen.TeVerwerken
                    };
                    return _openKlantApiClient.QueryInterneTakenAsync(query);
                });

            var results = await Task.WhenAll(internetakenTasks);

            var internetaken = results
                .SelectMany(x => x)
                .OrderByDescending(x => x.ToegewezenOp)
                .ToList();

            var overviewItems = await Task.WhenAll(internetaken.Select(MapToOverviewItemAsync));

            return [.. overviewItems];
        }

        private async Task<MyInterneTaakOverviewItem> MapToOverviewItemAsync(Internetaak internetaak)
        {
            var contactDatum = internetaak.AanleidinggevendKlantcontact?.PlaatsgevondenOp;
            var afdelingNaam = await GetAfdelingNaamAsync(internetaak);

            return new MyInterneTaakOverviewItem
            {
                Uuid = internetaak.Uuid,
                Nummer = internetaak.Nummer,
                GevraagdeHandeling = internetaak.GevraagdeHandeling,
                Status = internetaak.Status,
                ToegewezenOp = internetaak.ToegewezenOp,
                AfgehandeldOp = internetaak.AfgehandeldOp,
                AanleidinggevendKlantcontact = internetaak.AanleidinggevendKlantcontact,
                Urgentie = _urgentieBerekenService.Bereken(contactDatum),
                AfdelingNaam = afdelingNaam
            };
        }

        private async Task<string?> GetAfdelingNaamAsync(Internetaak internetaak)
        {
            if (internetaak.ToegewezenAanActoren?.Any() != true)
                return null;

            var actorTasks = internetaak.ToegewezenAanActoren
                .Where(actorRef => !string.IsNullOrEmpty(actorRef.Uuid))
                .Select(actorRef => GetActorSafeAsync(actorRef.Uuid));

            var actors = await Task.WhenAll(actorTasks);

            var afdelingActors = actors
                .Where(a => a?.SoortActor != SoortActor.medewerker && !string.IsNullOrEmpty(a?.Naam))
                .Select(a => a!.Naam)
                .ToList();

            return afdelingActors.Count != 0 ? string.Join(", ", afdelingActors) : null;
        }

        private async Task<Actor?> GetActorSafeAsync(string uuid)
        {
            try
            {
                return await _openKlantApiClient.GetActorAsync(uuid);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch actor {Uuid}", uuid);
                return null;
            }
        }

        private async Task<IReadOnlyList<string>> GetActorIds(ITAUser user)
        {
            var actorIds = new List<string>();

            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                var fromEntra = await _openKlantApiClient.QueryActorAsync(new ActorQuery
                {
                    ActoridentificatorCodeObjecttype = KnownMedewerkerIdentificators.EmailFromEntraId.CodeObjecttype,
                    ActoridentificatorCodeRegister = KnownMedewerkerIdentificators.EmailFromEntraId.CodeRegister,
                    ActoridentificatorCodeSoortObjectId = KnownMedewerkerIdentificators.EmailFromEntraId.CodeSoortObjectId,
                    IndicatieActief = true,
                    SoortActor = SoortActor.medewerker,
                    ActoridentificatorObjectId = user.Email
                });

                if (fromEntra != null)
                {
                    actorIds.Add(fromEntra.Uuid);
                }

                var fromHandmatig = await _openKlantApiClient.QueryActorAsync(new ActorQuery
                {
                    ActoridentificatorCodeObjecttype = KnownMedewerkerIdentificators.EmailHandmatig.CodeObjecttype,
                    ActoridentificatorCodeRegister = KnownMedewerkerIdentificators.EmailHandmatig.CodeRegister,
                    ActoridentificatorCodeSoortObjectId = KnownMedewerkerIdentificators.EmailHandmatig.CodeSoortObjectId,
                    IndicatieActief = true,
                    SoortActor = SoortActor.medewerker,
                    ActoridentificatorObjectId = user.Email
                });

                if (fromHandmatig != null)
                {
                    actorIds.Add(fromHandmatig.Uuid);
                }
            }

            if (!string.IsNullOrWhiteSpace(user.ObjectregisterMedewerkerId))
            {
                var fromObjecten = await _openKlantApiClient.QueryActorAsync(new ActorQuery
                {
                    ActoridentificatorCodeObjecttype = KnownMedewerkerIdentificators.ObjectRegisterId.CodeObjecttype,
                    ActoridentificatorCodeRegister = KnownMedewerkerIdentificators.ObjectRegisterId.CodeRegister,
                    ActoridentificatorCodeSoortObjectId = KnownMedewerkerIdentificators.ObjectRegisterId.CodeSoortObjectId,
                    IndicatieActief = true,
                    SoortActor = SoortActor.medewerker,
                    ActoridentificatorObjectId = user.ObjectregisterMedewerkerId
                });

                if (fromObjecten != null)
                {
                    actorIds.Add(fromObjecten.Uuid);
                }
            }

            return actorIds;
        }

    }
}
