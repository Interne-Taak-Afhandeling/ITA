using InterneTaakAfhandeling.Common.Services.OpenklantApi;
using InterneTaakAfhandeling.Common.Services.OpenklantApi.Models;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using InterneTaakAfhandeling.Web.Server.Authentication;

namespace InterneTaakAfhandeling.Web.Server.Services
{
    public interface IUserService
    {
        Task<IReadOnlyList<Internetaken>> GetInterneTakenByAssignedUser(ITAUser user);
    }
    public class UserService(IOpenKlantApiClient openKlantApiClient) : IUserService
    {
        private readonly IOpenKlantApiClient _openKlantApiClient = openKlantApiClient;


        public async Task<IReadOnlyList<Internetaken>> GetInterneTakenByAssignedUser(ITAUser user)
        {
            var actorIds = await GetActorIds(user);
            var tasks = actorIds.Select(a => _openKlantApiClient.GetOutstandingInternetakenByToegewezenAanActor(a));
            var all = await Task.WhenAll(tasks);
            return all.SelectMany(x => x).OrderByDescending(x => x.ToegewezenOp).ToList();
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

            if (!string.IsNullOrWhiteSpace(user.Id))
            {
                var fromObjecten = await _openKlantApiClient.QueryActorAsync(new ActorQuery
                {
                    ActoridentificatorCodeObjecttype = KnownMedewerkerIdentificators.IdFromObjectRegistration.CodeObjecttype,
                    ActoridentificatorCodeRegister = KnownMedewerkerIdentificators.IdFromObjectRegistration.CodeRegister,
                    ActoridentificatorCodeSoortObjectId = KnownMedewerkerIdentificators.IdFromObjectRegistration.CodeSoortObjectId,
                    IndicatieActief = true,
                    SoortActor = SoortActor.medewerker,
                    ActoridentificatorObjectId = user.Id
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
