using InterneTaakAfhandeling.Common.Exceptions;
using InterneTaakAfhandeling.Common.Services.Emailservices.Content;
using InterneTaakAfhandeling.Common.Services.Emailservices.SmtpMailService;
using InterneTaakAfhandeling.Common.Services.ObjectApi;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using InterneTaakAfhandeling.Common.Services.ZakenApi;
using InterneTaakAfhandeling.Common.Services.ZakenApi.Models;

namespace InterneTaakAfhandeling.Web.Server.Features.ForwardContactRequest;

public interface IForwardContactRequestService
{
    Task<Internetaak?> ForwardAsync(Guid internetaakId, ForwardContactRequestModel request);
}

public class ForwardContactRequestService(
    IOpenKlantApiClient openKlantApiClient,
    IObjectApiClient objectApiClient,
    IEmailService emailService,
    IEmailContentService emailContentService,
    IZakenApiClient zakenApiClient,
    IContactmomentenService contactmomentenService) : IForwardContactRequestService
{
    public async Task<Internetaak?> ForwardAsync(Guid internetaakId, ForwardContactRequestModel request)
    {
        var actors = await GetTargetActors(request);

        var internetaak = await openKlantApiClient.GetInternetaakByIdAsync(internetaakId) ??
                          throw new ArgumentException($"Internetaak with ID {internetaakId} not found.");


        var internetakenUpdateRequest = new InternetakenPatchActorsRequest
        {
            ToegewezenAanActoren = [.. actors.Select(x => new UuidObject { Uuid = Guid.Parse(x.Uuid) })]
        };


        var updatedInternetaak =
            await openKlantApiClient.PatchInternetaakActorAsync(internetakenUpdateRequest, internetaak.Uuid) ??
            throw new InvalidOperationException(
                $"Unable to update Internetaak with ID {internetaakId}.");

        await NotifyInternetaakActors(updatedInternetaak);

        return updatedInternetaak;
    }


    private async Task NotifyInternetaakActors(Internetaak internetaken)
    {
        try
        {
            var actorEmails = await ResolveActorsEmailAsync(internetaken);
            if (actorEmails.Count > 0)
            {
                var klantContact =
                    await openKlantApiClient.GetKlantcontactAsync(internetaken.AanleidinggevendKlantcontact.Uuid);

                var digitaleAdress = klantContact.Expand?.HadBetrokkenen
                    ?.SelectMany(x => x.Expand?.DigitaleAdressen ?? []).ToList();

                Zaak? zaak = null;

                var onderwerpObjectId = contactmomentenService.GetZaakOnderwerpObject(klantContact);

                if (!string.IsNullOrEmpty(onderwerpObjectId))
                    zaak = await zakenApiClient.GetZaakAsync(onderwerpObjectId);

                var emailContent =
                    emailContentService.BuildInternetakenEmailContent(internetaken, klantContact, digitaleAdress, zaak);

                await Task.WhenAll(actorEmails.Select(email =>
                    emailService.SendEmailAsync(email, $"Contactverzoek Doorgestuurd - {internetaken.Nummer}",
                        emailContent)));
            }
            else
            {
                throw new KeyNotFoundException($"No actor emails found for internetaken: {internetaken.Nummer}"
                );
            }
        }
        catch (Exception ex)
        {
            throw new ConflictException($"Error notifying actors for internetaken {ex}");
        }
    }


    private async Task<List<string>> ResolveActorsEmailAsync(Internetaak internetaken)
    {
        if (internetaken.ToegewezenAanActoren == null)
            throw new Exception($"No actor assigned to internetaak {internetaken.Nummer}");

        var emailAddresses = new List<string>();

        foreach (var toegewezenAanActoren in internetaken.ToegewezenAanActoren)
        {
            var actor = await openKlantApiClient.GetActorAsync(toegewezenAanActoren.Uuid);

            var validCodeObjectTypes = new List<string> { "mdw", "afd", "grp" };

            if (actor.Actoridentificator == null ||
                !validCodeObjectTypes.Contains(actor.Actoridentificator.CodeObjecttype))
                continue;

            var objectId = actor.Actoridentificator.ObjectId;
            var actorIdentificator = actor.Actoridentificator;

            if (actorIdentificator.CodeSoortObjectId ==
                KnownMedewerkerIdentificators.EmailHandmatig.CodeSoortObjectId &&
                actorIdentificator.CodeRegister == KnownMedewerkerIdentificators.EmailHandmatig.CodeRegister)
            {
                emailAddresses.Add(objectId);
            }

            else if (actorIdentificator is { CodeSoortObjectId: "idf", CodeRegister: "obj" })
            {
                var objectRecords = await objectApiClient.GetObjectsByIdentificatie(objectId);
                switch (objectRecords.Count)
                {
                    case 0:
                        throw new KeyNotFoundException(
                            $"No Object found in overigeobjecten for actorIdentificator {objectId}");
                    case > 1:
                        throw new ConflictException(
                            $"Multiple objects found in overigeobjecten for actorIdentificator {objectId}. Expected exactly one match.");
                    default:
                        objectRecords.First().Data.EmailAddresses.ForEach(x =>
                        {
                            if (!string.IsNullOrEmpty(x) && EmailService.IsValidEmail(x))
                                emailAddresses.Add(x);
                            else
                                throw new ConflictException($"Invalid email address found for object {objectId}");
                        });
                        break;
                }
            }
        }

        return emailAddresses;
    }


    private async Task<List<Actor>> GetTargetActors(ForwardContactRequestModel request)
    {
        var actors = new List<Actor>();

        var primaryActor = request.ActorType switch
        {
            KnownActorType.Medewerker => await GetOrCreateMedewerkerActor(request.ActorIdentifier),
            KnownActorType.Afdeling => await GetOrCreateAfdelingActor(request.ActorIdentifier),
            KnownActorType.Groep => await GetOrCreateGroepActor(request.ActorIdentifier),
            _ => throw new ArgumentException($"Invalid actor type: {request.ActorType}")
        };

        if (primaryActor != null) actors.Add(primaryActor);


        if ((request.ActorType != KnownActorType.Afdeling && request.ActorType != KnownActorType.Groep) ||
            string.IsNullOrWhiteSpace(request.MedewerkerEmail)) return actors;
        var medewerkerActor = await GetOrCreateMedewerkerActor(request.MedewerkerEmail);
        if (medewerkerActor != null) actors.Add(medewerkerActor);

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