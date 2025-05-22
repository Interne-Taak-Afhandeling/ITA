using InterneTaakAfhandeling.Common.Services.OpenklantApi;
using InterneTaakAfhandeling.Common.Services.OpenklantApi.Models;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Mapper;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using InterneTaakAfhandeling.Web.Server.Authentication;

namespace InterneTaakAfhandeling.Web.Server.Services
{
    public interface IUserService
    {
        Task<IReadOnlyList<Internetaken>> GetInterneTakenByAssignedUser(ITAUser user);
        Task<Internetaken> AssignInternetakenToSelfAsync(string internetakenId, ITAUser user);
    }
    public class UserService(IOpenKlantApiClient openKlantApiClient) : IUserService
    {
        private readonly IOpenKlantApiClient _openKlantApiClient = openKlantApiClient;

        public async Task<Internetaken> AssignInternetakenToSelfAsync(string internetakenId, ITAUser user)
        {
            var actor = await GetActor(user) ?? throw new Exception("Actor not found.");

            var internetaken = await _openKlantApiClient.GetInternetakenByIdAsync(internetakenId) ?? throw new Exception($"Internetaken with ID {internetakenId} not found.");
            var actors = internetaken.ToegewezenAanActoren?.Where(x => x.SoortActor != SoortActor.medewerker.ToString()).ToList() ?? [];

            actors.Add(actor);

            internetaken.ToegewezenAanActoren = actors;

            return await _openKlantApiClient.UpdateInternetakenAsync(internetaken.MapToUpdateRequest(), internetaken.Uuid) ?? throw new Exception($"Unable to update Internetaken with ID {internetakenId}.");

        }
        public async Task<IReadOnlyList<Internetaken>> GetInterneTakenByAssignedUser(ITAUser user)
        {
            var actorIds = await GetActorIds(user);
            var internetakenTasks = actorIds.Select(a => _openKlantApiClient.GetOutstandingInternetakenByToegewezenAanActor(a));

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
        private async Task<Actor?> GetActor(ITAUser user)
        {

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
                    return fromObjecten;
                }
            }

            return null;
        }

    }
}
