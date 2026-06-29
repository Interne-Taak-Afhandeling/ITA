using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using InterneTaakAfhandeling.Common.Services;
using InterneTaakAfhandeling.Web.Server.Authentication;
using InterneTaakAfhandeling.Web.Server.Features.Internetaken;
using InterneTaakAfhandeling.Web.Server.Features.Urgentie;

namespace InterneTaakAfhandeling.Web.Server.Features.MyInterneTakenOverview
{
    public interface IMyInterneTakenOverviewService
    {
        Task<IReadOnlyList<MyInterneTaakOverviewItem>> GetInterneTakenByAssignedUser(ITAUser user, bool afgerond);
    }
    public class MyInterneTakenOverviewService(
        IOpenKlantApiClient openKlantApiClient,
        IUrgentieBerekenService urgentieBerekenService) : IMyInterneTakenOverviewService
    {
        private readonly IOpenKlantApiClient _openKlantApiClient = openKlantApiClient;
        private readonly IUrgentieBerekenService _urgentieBerekenService = urgentieBerekenService;


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
                        Status = (afgerond == true) ? KnownInternetaakStatussen.Verwerkt : KnownInternetaakStatussen.TeVerwerken
                    };
                    return _openKlantApiClient.QueryInterneTakenAsync(query);
                });

            var results = await Task.WhenAll(internetakenTasks);

            return [.. results
                .SelectMany(x => x)
                .OrderByDescending(x => x.ToegewezenOp)
                .Select(MapToOverviewItem)];
        }

        private MyInterneTaakOverviewItem MapToOverviewItem(Internetaak internetaak)
        {
            var contactDatum = internetaak.AanleidinggevendKlantcontact?.PlaatsgevondenOp;

            return new MyInterneTaakOverviewItem
            {
                Uuid = internetaak.Uuid,
                Nummer = internetaak.Nummer,
                GevraagdeHandeling = internetaak.GevraagdeHandeling,
                Status = internetaak.Status,
                ToegewezenOp = internetaak.ToegewezenOp,
                AfgehandeldOp = internetaak.AfgehandeldOp,
                AanleidinggevendKlantcontact = internetaak.AanleidinggevendKlantcontact,
                Urgentie = _urgentieBerekenService.Bereken(contactDatum)
            };
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