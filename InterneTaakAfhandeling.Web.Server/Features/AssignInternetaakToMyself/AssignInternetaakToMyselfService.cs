using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using InterneTaakAfhandeling.Web.Server.Authentication;

namespace InterneTaakAfhandeling.Web.Server.Features.AssignInternetaakToMyself
{
    public interface IAssignInternetaakToMyselfService
    {
        Task<Common.Services.OpenKlantApi.Models.Internetaken> ToSelfAsync(string internetakenId, ITAUser user);
    }
    public class AssignInternetaakToMyselfService(IOpenKlantApiClient openKlantApiClient) : IAssignInternetaakToMyselfService
    {
        private readonly IOpenKlantApiClient _openKlantApiClient = openKlantApiClient;
        public async Task<Common.Services.OpenKlantApi.Models.Internetaken> ToSelfAsync(string internetakenId, ITAUser user)
        {
            var currentUserActor = await GetActor(user) ?? await CreateEntraActor(user);
            var internetaken = await _openKlantApiClient.GetInternetakenByIdAsync(internetakenId) ?? throw new Exception($"Internetaken with ID {internetakenId} not found.");

            var actors = await GetAssignedOrganisationalUnitActors(internetaken);

            if (currentUserActor == null)
            {
                throw new ArgumentNullException("Current user actor cannot be null.");
            }

            actors.Add(currentUserActor);

            var internetakenUpdateRequest = new InternetakenUpdateRequest
            {
                Nummer = internetaken.Nummer,
                GevraagdeHandeling = internetaken.GevraagdeHandeling,
                AanleidinggevendKlantcontact = new UuidObject { Uuid = Guid.Parse(internetaken.AanleidinggevendKlantcontact.Uuid) },
                ToegewezenAanActoren = actors
                    .Select(x => new UuidObject { Uuid = Guid.Parse(x.Uuid) })
                    .ToList(),
                Toelichting = internetaken.Toelichting ?? string.Empty,
                Status = internetaken.Status
            };

            return await _openKlantApiClient.UpdateInternetakenAsync(internetakenUpdateRequest, internetaken.Uuid) ?? throw new Exception($"Unable to update Internetaken with ID {internetakenId}.");
        }

        private async Task<List<Actor>> GetAssignedOrganisationalUnitActors(Common.Services.OpenKlantApi.Models.Internetaken internetaken)
        {
            var internetaakActorTasks = internetaken.ToegewezenAanActoren?.Select(x => _openKlantApiClient.GetActorAsync(x.Uuid)) ?? [];
            var notMedewerkerActors = (await Task.WhenAll(internetaakActorTasks)).Where(x => x.SoortActor != SoortActor.medewerker).ToList() ?? [];
            return notMedewerkerActors;
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
        private async Task<Actor?> CreateEntraActor(ITAUser user)
        {
            var actorRequest = new ActorRequest
            {
                SoortActor = SoortActor.medewerker,
                Naam = user.Name,
                Actoridentificator = new Actoridentificator
                {
                    CodeObjecttype = KnownMedewerkerIdentificators.EmailFromEntraId.CodeObjecttype,
                    CodeRegister = KnownMedewerkerIdentificators.EmailFromEntraId.CodeRegister,
                    CodeSoortObjectId = KnownMedewerkerIdentificators.EmailFromEntraId.CodeSoortObjectId,
                    ObjectId = user.Email
                },
                IndicatieActief = true
            };
            return await _openKlantApiClient.CreateActorAsync(actorRequest);
        }
    }
}
