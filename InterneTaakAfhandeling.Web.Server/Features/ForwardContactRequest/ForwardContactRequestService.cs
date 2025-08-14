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
        if (string.IsNullOrEmpty(request.ActorIdentifier))
            throw new ArgumentException(
                "Actor identifier must not be empty.");
        var targetActor = await GetTargetActor(request) ??
                          throw new ArgumentException(
                              $"Actor with identifier {request.ActorIdentifier} of type {request.ActorType} not found.");

        var internetaak = await openKlantApiClient.GetInternetaakByIdAsync(internetaakId) ??
                          throw new ArgumentException($"Internetaak with ID {internetaakId} not found.");

        var actors = await GetAssignedOrganisationalUnitActors(internetaak);

        actors.Add(targetActor);


        if (internetaak.Status == null || internetaak.Nummer == null || internetaak.Status == null ||
            internetaak.GevraagdeHandeling == null) return null;
        var internetakenUpdateRequest = new InternetakenUpdateRequest
        {
            Nummer = internetaak.Nummer,
            GevraagdeHandeling = internetaak.GevraagdeHandeling,
            AanleidinggevendKlantcontact = new UuidObject
                { Uuid = Guid.Parse(internetaak.AanleidinggevendKlantcontact.Uuid) },
            ToegewezenAanActoren = actors
                .Select(x => new UuidObject { Uuid = Guid.Parse(x.Uuid) })
                .ToList(),
            Toelichting = internetaak.Toelichting ?? string.Empty,
            Status = internetaak.Status
        };


        var updatedInternetaak =
            await openKlantApiClient.PutInternetaakAsync(internetakenUpdateRequest, internetaak.Uuid) ??
            throw new InvalidOperationException(
                $"Unable to update Internetaak with ID {internetaakId}.");

        return updatedInternetaak;
    }

    private async Task<Actor?> GetTargetActor(ForwardContactRequestModel request)
    {
        return request.ActorType.ToLower() switch
        {
            "medewerker" => await GetMedewerkerActor(request.ActorIdentifier),
            "afdeling" => await GetAfdelingActor(request.ActorIdentifier),
            "groep" => await GetGroepActor(request.ActorIdentifier),
            _ => throw new ArgumentException($"Invalid actor type: {request.ActorType}")
        };
    }

    private async Task<Actor?> GetMedewerkerActor(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        var actor = await openKlantApiClient.QueryActorAsync(new ActorQuery
        {
            ActoridentificatorCodeObjecttype = KnownMedewerkerIdentificators.EmailFromEntraId.CodeObjecttype,
            ActoridentificatorCodeRegister = KnownMedewerkerIdentificators.EmailFromEntraId.CodeRegister,
            ActoridentificatorCodeSoortObjectId = KnownMedewerkerIdentificators.EmailFromEntraId.CodeSoortObjectId,
            IndicatieActief = true,
            SoortActor = SoortActor.medewerker,
            ActoridentificatorObjectId = email
        });

        return actor;
    }

    private async Task<Actor?> GetAfdelingActor(string identifier)
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

        return actor;
    }

    private async Task<Actor?> GetGroepActor(string identifier)
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

        return actor;
    }

    private async Task<List<Actor>> GetAssignedOrganisationalUnitActors(Internetaak internetaak)
    {
        var internetaakActorTasks =
            internetaak.ToegewezenAanActoren?.Select(x => openKlantApiClient.GetActorAsync(x.Uuid)) ?? [];
        var notMedewerkerActors = (await Task.WhenAll(internetaakActorTasks))
            .Where(x => x.SoortActor != SoortActor.medewerker).ToList() ?? [];
        return notMedewerkerActors;
    }
}