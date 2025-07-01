using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using InterneTaakAfhandeling.Web.Server.Authentication;

namespace InterneTaakAfhandeling.Web.Server.Features.AssignInternetaakToMe
{
    public interface IAssignInternetaakToMeService
    {
        Task<(Internetaak internetaak, Actor currentUserActor)> ToSelfAsync(Guid internetakenId, ITAUser user);
    }
    public class AssignInternetaakToMeService(IOpenKlantApiClient openKlantApiClient) : IAssignInternetaakToMeService
    {
        private readonly IOpenKlantApiClient _openKlantApiClient = openKlantApiClient;
        public async Task<(Common.Services.OpenKlantApi.Models.Internetaak internetaak, Actor currentUserActor)> ToSelfAsync(Guid internetakenId, ITAUser user)
        {
            var currentUserActor = await GetActor(user) ?? await CreateEntraActor(user);
            var internetaak = await _openKlantApiClient.GetInternetaakByIdAsync(internetakenId) ?? throw new Exception($"Internetaken with ID {internetakenId} not found.");

            var actors = await GetAssignedOrganisationalUnitActors(internetaak);

            if (currentUserActor == null)
            {
                throw new ArgumentNullException("Current user actor cannot be null.");
            }

            actors.Add(currentUserActor);

            var internetakenUpdateRequest = new InternetakenUpdateRequest
            {
                Nummer = internetaak.Nummer,
                GevraagdeHandeling = internetaak.GevraagdeHandeling,
                AanleidinggevendKlantcontact = new UuidObject { Uuid = Guid.Parse(internetaak.AanleidinggevendKlantcontact.Uuid) },
                ToegewezenAanActoren = actors
                    .Select(x => new UuidObject { Uuid = Guid.Parse(x.Uuid) })
                    .ToList(),
                Toelichting = internetaak.Toelichting ?? string.Empty,
                Status = internetaak.Status
            };

            var updatedInternetaak = await _openKlantApiClient.PutInternetaakAsync(internetakenUpdateRequest, internetaak.Uuid) ?? throw new Exception($"Unable to update Internetaken with ID {internetakenId}.");

            return (updatedInternetaak, currentUserActor);
        }

        private async Task<List<Actor>> GetAssignedOrganisationalUnitActors(Common.Services.OpenKlantApi.Models.Internetaak internetaken)
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
