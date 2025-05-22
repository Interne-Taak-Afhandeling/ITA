using InterneTaakAfhandeling.Common.Services.OpenklantApi;
using InterneTaakAfhandeling.Common.Services.OpenklantApi.Models;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Mapper;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using InterneTaakAfhandeling.Web.Server.Authentication;

namespace InterneTaakAfhandeling.Web.Server.Features.AssignInternetaken
{
    public interface IAssignInternetakenService
    {
        Task<Common.Services.OpenKlantApi.Models.Internetaken> ToSelfAsync(string internetakenId, ITAUser user);
    }
    public class AssignInternetakenService(IOpenKlantApiClient openKlantApiClient) : IAssignInternetakenService
    {
        private readonly IOpenKlantApiClient _openKlantApiClient = openKlantApiClient;
        public async Task<Common.Services.OpenKlantApi.Models.Internetaken> ToSelfAsync(string internetakenId, ITAUser user)
        {
            var actor = await GetActor(user) ?? throw new Exception("Actor not found.");

            var internetaken = await _openKlantApiClient.GetInternetakenByIdAsync(internetakenId) ?? throw new Exception($"Internetaken with ID {internetakenId} not found.");

            var actorTasks = internetaken.ToegewezenAanActoren?.Select(x => _openKlantApiClient.GetActorAsync(x.Uuid)) ?? [];
            var actors = (await Task.WhenAll(actorTasks)).Where(x => x.SoortActor != SoortActor.medewerker.ToString()).ToList() ?? [];

            actors.Add(actor);

            internetaken.ToegewezenAanActoren = actors;

            return await _openKlantApiClient.UpdateInternetakenAsync(internetaken.MapToUpdateRequest(), internetaken.Uuid) ?? throw new Exception($"Unable to update Internetaken with ID {internetakenId}.");

        }

        private async Task<Actor?> GetActor(ITAUser user)
        {

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

                return fromEntra;

            }

            return null;
        }

    }
}
