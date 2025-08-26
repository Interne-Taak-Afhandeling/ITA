using InterneTaakAfhandeling.Common.Services;
using InterneTaakAfhandeling.Common.Services.Emailservices.Content;
using InterneTaakAfhandeling.Common.Services.Emailservices.SmtpMailService;
using InterneTaakAfhandeling.Common.Services.ObjectApi;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;

namespace InterneTaakAfhandeling.Web.Server.Features.ForwardContactRequest;

public interface IForwardContactRequestService
{
    Task<ForwardContactRequestResponse> ForwardAsync(Guid internetaakId, ForwardContactRequestModel request);
}

public class ForwardContactRequestService(
    IOpenKlantApiClient openKlantApiClient,
    IObjectApiClient objectApiClient,
    IEmailService emailService,
    IEmailContentService emailContentService,
    ILogger<ForwardContactRequestService> logger,
    IInterneTaakEmailInputService emailInputService) : IForwardContactRequestService
{
    public async Task<ForwardContactRequestResponse> ForwardAsync(Guid internetaakId,
        ForwardContactRequestModel request)
    {
        var actors = await GetTargetActors(request);

        var internetaak = await openKlantApiClient.GetInternetaakByIdAsync(internetaakId);

        var internetakenUpdateRequest = new InternetakenPatchActorsRequest
        {
            ToegewezenAanActoren = [.. actors.Select(x => new UuidObject { Uuid = Guid.Parse(x.Uuid) })]
        };

        var updatedInternetaak = await openKlantApiClient.PatchInternetaakActorAsync(internetakenUpdateRequest, internetaak.Uuid);

        var notficationResult = await NotifyInternetaakActors(updatedInternetaak, actors);

        return new ForwardContactRequestResponse
        {
            Internetaak = updatedInternetaak,
            NotificationResult = notficationResult
        };
    }

    private async Task<string> NotifyInternetaakActors(Internetaak internetaken, IReadOnlyList<Actor> actors)
    {
        const string GenericError = "Het contactverzoek is doorgestuurd, maar hiervan kon geen emailnotificatie verstuurd worden";

        try
        {
            if (!emailService.IsConfiguredCorrectly())
            {
                return GenericError;
            }

            var actorEmailResult = await emailInputService.ResolveActorsEmailAsync(actors);

            if (actorEmailResult.FoundEmails.Count > 0)
            {
                var emailInput = await emailInputService.FetchInterneTaakEmailInput(internetaken);

                var emailContent = emailContentService.BuildInternetakenEmailContent(emailInput);

                var emailTasks = actorEmailResult.FoundEmails.Select(async email => await emailService.SendEmailAsync(email,
                        $"Contactverzoek Doorgestuurd - {internetaken.Nummer}", emailContent));

                await Task.WhenAll(emailTasks);
            }

            return actorEmailResult.Errors.Count > 0
                ? string.Join("\n", actorEmailResult.Errors)
                : "Contactverzoek succesvol doorgestuurd";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing internetaken {Number}", internetaken.Nummer);
            return GenericError;
        }
    }

    private async Task<List<Actor>> GetTargetActors(ForwardContactRequestModel request)
    {
        var actors = new List<Actor>();

        var primaryActor = request.ActorType switch
        {
            KnownActorType.Afdeling => await GetOrCreateAfdelingActor(request.ActorIdentifier),
            KnownActorType.Groep => await GetOrCreateGroepActor(request.ActorIdentifier),
            _ => throw new ArgumentException($"Invalid actor type: {request.ActorType}")
        };

        actors.Add(primaryActor);

        if (string.IsNullOrWhiteSpace(request.MedewerkerEmail)) return actors;

        var medewerkerActor = await GetOrCreateMedewerkerActor(request.MedewerkerEmail);
        actors.Add(medewerkerActor);

        return actors;
    }

    private async Task<Actor> GetOrCreateMedewerkerActor(string identifier)
    {
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
                IndicatieActief = true,
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

    private async Task<Actor> GetOrCreateAfdelingActor(string identifier)
    {
        var afdeling = await objectApiClient.GetAfdeling(identifier);

        var actor = await openKlantApiClient.QueryActorAsync(new ActorQuery
        {
            IndicatieActief = true,
            SoortActor = SoortActor.organisatorische_eenheid,
            ActoridentificatorObjectId = identifier,
            ActoridentificatorCodeObjecttype = KnownAfdelingIdentificators.ObjectRegisterId.CodeObjecttype,
            ActoridentificatorCodeRegister = KnownAfdelingIdentificators.ObjectRegisterId.CodeRegister,
            ActoridentificatorCodeSoortObjectId = KnownAfdelingIdentificators.ObjectRegisterId.CodeSoortObjectId
        });
        if (actor == null)
        {
            var actorRequest = new ActorRequest
            {
                SoortActor = SoortActor.organisatorische_eenheid,
                Naam = afdeling.Record.Data.Naam,
                IndicatieActief = true,
                Actoridentificator = new Actoridentificator
                {
                    CodeObjecttype = KnownAfdelingIdentificators.ObjectRegisterId.CodeObjecttype,
                    CodeRegister = KnownAfdelingIdentificators.ObjectRegisterId.CodeRegister,
                    CodeSoortObjectId = KnownAfdelingIdentificators.ObjectRegisterId.CodeSoortObjectId,
                    ObjectId = identifier
                }
            };
            return await openKlantApiClient.CreateActorAsync(actorRequest);
        }

        return actor;
    }

    private async Task<Actor> GetOrCreateGroepActor(string identifier)
    {
        var groep = await objectApiClient.GetGroep(identifier);

        var actor = await openKlantApiClient.QueryActorAsync(new ActorQuery
        {
            IndicatieActief = true,
            SoortActor = SoortActor.organisatorische_eenheid,
            ActoridentificatorObjectId = identifier,
            ActoridentificatorCodeObjecttype = KnownGroepIdentificators.ObjectRegisterId.CodeObjecttype,
            ActoridentificatorCodeRegister = KnownGroepIdentificators.ObjectRegisterId.CodeRegister,
            ActoridentificatorCodeSoortObjectId = KnownGroepIdentificators.ObjectRegisterId.CodeSoortObjectId
        });
        if (actor == null)
        {
            var actorRequest = new ActorRequest
            {
                SoortActor = SoortActor.organisatorische_eenheid,
                Naam = groep.Record.Data.Naam,
                IndicatieActief = true,
                Actoridentificator = new Actoridentificator
                {
                    CodeObjecttype = KnownGroepIdentificators.ObjectRegisterId.CodeObjecttype,
                    CodeRegister = KnownGroepIdentificators.ObjectRegisterId.CodeRegister,
                    CodeSoortObjectId = KnownGroepIdentificators.ObjectRegisterId.CodeSoortObjectId,
                    ObjectId = identifier
                }
            };
            return await openKlantApiClient.CreateActorAsync(actorRequest);
        }

        return actor;
    }
}
