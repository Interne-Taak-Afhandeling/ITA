using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;

namespace InterneTaakAfhandeling.Web.Server.Features.ForwardContactRequest;

public interface IForwardContactRequestService
{
    Task<Internetaak?> ForwardAsync(Guid internetaakId, ForwardContactRequestModel request);
}

public class ForwardContactRequestService(IOpenKlantApiClient openKlantApiClient) : IForwardContactRequestService
{
    public async Task<Internetaak?> ForwardAsync(Guid internetaakId, ForwardContactRequestModel request)
    {

        var actors = await GetTargetActors(request);

        var internetaak = await openKlantApiClient.GetInternetaakByIdAsync(internetaakId) ??
                          throw new ArgumentException($"Internetaak with ID {internetaakId} not found.");

         
        var internetakenUpdateRequest = new InternetakenPatchActorsRequest()
        { 
            ToegewezenAanActoren = [.. actors.Select(x => new UuidObject { Uuid = Guid.Parse(x.Uuid) })]
        };


        var updatedInternetaak =
            await openKlantApiClient.PatchInternetaakActorAsync(internetakenUpdateRequest, internetaak.Uuid) ??
            throw new InvalidOperationException(
                $"Unable to update Internetaak with ID {internetaakId}.");

        return updatedInternetaak;
    }

    private async Task<List<Actor>> GetTargetActors(ForwardContactRequestModel request)
    {
        var actors = new List<Actor>();

        if (!KnownActorTypeExtensions.TryParseActorType(request.ActorType, out var parsedActorType))
        {
            throw new ArgumentException($"Invalid actor type: {request.ActorType}");
        }

        var primaryActor = parsedActorType switch
        {
            KnownActorType.Medewerker => await GetOrCreateMedewerkerActor(request.ActorIdentifier),
            KnownActorType.Afdeling => await GetOrCreateAfdelingActor(request.ActorIdentifier),
            KnownActorType.Groep => await GetOrCreateGroepActor(request.ActorIdentifier),
            _ => throw new ArgumentException($"Invalid actor type: {request.ActorType}")
        };

        if (primaryActor != null)
        {
            actors.Add(primaryActor);
        }


        if((parsedActorType != KnownActorType.Afdeling && parsedActorType != KnownActorType.Groep) ||
            string.IsNullOrWhiteSpace(request.MedewerkerEmail)) return actors;
        var medewerkerActor = await GetOrCreateMedewerkerActor(request.MedewerkerEmail);
        if (medewerkerActor != null)
        {
            actors.Add(medewerkerActor);
        }

        return actors;
    }

    private async Task<Actor?> GetOrCreateMedewerkerActor(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            return null;

        var actor = await openKlantApiClient.QueryActorAsync(new ActorQuery
        {
            ActoridentificatorCodeObjecttype = KnownMedewerkerIdentificators.EmailHandmatig.CodeObjecttype,
            ActoridentificatorCodeRegister = KnownMedewerkerIdentificators.EmailHandmatig.CodeRegister,
            ActoridentificatorCodeSoortObjectId = KnownMedewerkerIdentificators.EmailHandmatig.CodeSoortObjectId,
            IndicatieActief = true,
            SoortActor = SoortActor.medewerker,
            ActoridentificatorObjectId = identifier
        });
        if (actor == null)
        {
            var actorRequest = new ActorRequest
            {
                SoortActor = SoortActor.medewerker,
                Naam = identifier,
                Actoridentificator = new Actoridentificator
                {
                    CodeObjecttype = KnownMedewerkerIdentificators.EmailHandmatig.CodeObjecttype,
                    CodeRegister = KnownMedewerkerIdentificators.EmailHandmatig.CodeRegister,
                    CodeSoortObjectId = KnownMedewerkerIdentificators.EmailHandmatig.CodeSoortObjectId,
                    ObjectId = identifier
                }
            };
            return await openKlantApiClient.CreateActorAsync(actorRequest);
        }
        return actor;
    }

    private async Task<Actor?> GetOrCreateAfdelingActor(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            return null;

        var actor = await openKlantApiClient.QueryActorAsync(new ActorQuery
        {
            IndicatieActief = true,
            SoortActor = SoortActor.organisatorische_eenheid,
            ActoridentificatorObjectId = identifier,
            ActoridentificatorCodeObjecttype = KnownAfdelingIdentificators.ObjectregisterId.CodeObjecttype,
            ActoridentificatorCodeRegister = KnownAfdelingIdentificators.ObjectregisterId.CodeRegister,
            ActoridentificatorCodeSoortObjectId = KnownAfdelingIdentificators.ObjectregisterId.CodeSoortObjectId
        });
        if (actor == null)
        {
            var actorRequest = new ActorRequest
            {
                SoortActor = SoortActor.organisatorische_eenheid,
                Naam = identifier,
                Actoridentificator = new Actoridentificator
                {
                    CodeObjecttype = KnownMedewerkerIdentificators.ObjectregisterId.CodeObjecttype,
                    CodeRegister = KnownMedewerkerIdentificators.ObjectregisterId.CodeRegister,
                    CodeSoortObjectId = KnownMedewerkerIdentificators.ObjectregisterId.CodeSoortObjectId,
                    ObjectId = identifier
                }
            };
            return await openKlantApiClient.CreateActorAsync(actorRequest);
        }
        return actor;
    }

    private async Task<Actor?> GetOrCreateGroepActor(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            return null;

        var actor = await openKlantApiClient.QueryActorAsync(new ActorQuery
        {
            IndicatieActief = true,
            SoortActor = SoortActor.organisatorische_eenheid,
            ActoridentificatorObjectId = identifier,
            ActoridentificatorCodeObjecttype = KnownGroepIdentificators.ObjectregisterId.CodeObjecttype,
            ActoridentificatorCodeRegister = KnownGroepIdentificators.ObjectregisterId.CodeRegister,
            ActoridentificatorCodeSoortObjectId = KnownGroepIdentificators.ObjectregisterId.CodeSoortObjectId
        });
        if (actor == null)
        {
            var actorRequest = new ActorRequest
            {
                SoortActor = SoortActor.organisatorische_eenheid,
                Naam = identifier,
                Actoridentificator = new Actoridentificator
                {
                    CodeObjecttype = KnownGroepIdentificators.ObjectregisterId.CodeObjecttype,
                    CodeRegister = KnownGroepIdentificators.ObjectregisterId.CodeRegister,
                    CodeSoortObjectId = KnownGroepIdentificators.ObjectregisterId.CodeSoortObjectId,
                    ObjectId = identifier
                }
            };
            return await openKlantApiClient.CreateActorAsync(actorRequest);
        }
        return actor;
    }

}