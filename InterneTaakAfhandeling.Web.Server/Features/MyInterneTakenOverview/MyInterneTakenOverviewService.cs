using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using InterneTaakAfhandeling.Web.Server.Authentication;
using InterneTaakAfhandeling.Web.Server.Features.Internetaken;

namespace InterneTaakAfhandeling.Web.Server.Features.MyInterneTakenOverview
{
    public interface IMyInterneTakenOverviewService
    {
        Task<IReadOnlyList<Internetaak>> GetInterneTakenByAssignedUser(ITAUser user, bool afgerond);
    }
    public class MyInterneTakenOverviewService(IOpenKlantApiClient openKlantApiClient) : IMyInterneTakenOverviewService
    {
        private readonly IOpenKlantApiClient _openKlantApiClient = openKlantApiClient;


        public async Task<IReadOnlyList<Internetaak>> GetInterneTakenByAssignedUser(ITAUser user, bool afgerond)
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

            return [.. results.SelectMany(x => x).OrderByDescending(x => x.ToegewezenOp)];
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
                    ActoridentificatorCodeObjecttype = KnownMedewerkerIdentificators.ObjectregisterId.CodeObjecttype,
                    ActoridentificatorCodeRegister = KnownMedewerkerIdentificators.ObjectregisterId.CodeRegister,
                    ActoridentificatorCodeSoortObjectId = KnownMedewerkerIdentificators.ObjectregisterId.CodeSoortObjectId,
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
