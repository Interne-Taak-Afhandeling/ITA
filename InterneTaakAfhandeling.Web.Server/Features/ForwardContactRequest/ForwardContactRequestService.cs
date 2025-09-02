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

    private const string GenericError = "Het contactverzoek is doorgestuurd, maar hiervan kon geen e-mailnotificatie verstuurd worden";

    private async Task<string> NotifyInternetaakActors(Internetaak internetaken, IReadOnlyList<Actor> actors)
    {
        try
        {
            if (!emailService.IsConfiguredCorrectly())
                return GenericError;

            var actorEmailResult = await emailInputService.ResolveActorsEmailAsync(actors);

            if (!actorEmailResult.FoundEmails.Any())
                return GetResultMessageWhenNoEmails(actorEmailResult);

            var emailInput = await emailInputService.FetchInterneTaakEmailInput(internetaken);
            var emailContent = emailContentService.BuildInternetakenEmailContent(emailInput);
            var subject = $"Contactverzoek Doorgestuurd - {internetaken.Nummer}";

            var sendResults = await SendEmailsAsync(actorEmailResult.FoundEmails, subject, emailContent);

            return GetResultMessage(sendResults, actorEmailResult);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing e-mail notifications for forwarded interne taak {Number}", internetaken.Nummer);
            return GenericError;
        }
    }

    private async Task<EmailResult[]> SendEmailsAsync(IEnumerable<string> emails, string subject, string content)
    {
        var tasks = emails.Select(email => emailService.SendEmailAsync(email, subject, content));
        return await Task.WhenAll(tasks);
    }

    private static string GetResultMessageWhenNoEmails(ActorEmailResolutionResult actorEmailResult)
    {
        return actorEmailResult.Errors.Any()
            ? $"Het contactverzoek is doorgestuurd, maar niet elke e-mailnotificatie kon verstuurd worden: \n{string.Join("\n", actorEmailResult.Errors)}"
            : "Contactverzoek succesvol doorgestuurd";
    }

    private static string GetResultMessage(EmailResult[] results, ActorEmailResolutionResult actorEmailResult)
    {
        var failedEmails = results.Where(r => !r.Success).ToList();
        var anySuccess = results.Length != 0 && results.Length != failedEmails.Count;

        if (!anySuccess)
            return GenericError;

        if (failedEmails.Any())
            return $"Contactverzoek doorgestuurd, maar {failedEmails.Count} email(s) notificaties mislukte tijdens het doorsturen.";

        if (actorEmailResult.Errors.Any())
            return $"Het contactverzoek is doorgestuurd, maar niet elke e-mailnotificatie kon verstuurd worden: \n{string.Join("\n", actorEmailResult.Errors)}";

        return "Contactverzoek succesvol doorgestuurd";
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

    private async Task<Actor> GetOrCreateMedewerkerActor(string email)
    {
        var actor = await openKlantApiClient.QueryActorAsync(new ActorQuery
        {
            ActoridentificatorCodeObjecttype = KnownMedewerkerIdentificators.EmailHandmatig.CodeObjecttype,
            ActoridentificatorCodeRegister = KnownMedewerkerIdentificators.EmailHandmatig.CodeRegister,
            ActoridentificatorCodeSoortObjectId = KnownMedewerkerIdentificators.EmailHandmatig.CodeSoortObjectId,
            IndicatieActief = true,
            SoortActor = SoortActor.medewerker,
            ActoridentificatorObjectId = email
        });
        if (actor == null)
        {
            var actorRequest = new ActorRequest
            {
                SoortActor = SoortActor.medewerker,
                Naam = email,
                IndicatieActief = true,
                Actoridentificator = new Actoridentificator
                {
                    CodeObjecttype = KnownMedewerkerIdentificators.EmailHandmatig.CodeObjecttype,
                    CodeRegister = KnownMedewerkerIdentificators.EmailHandmatig.CodeRegister,
                    CodeSoortObjectId = KnownMedewerkerIdentificators.EmailHandmatig.CodeSoortObjectId,
                    ObjectId = email
                }
            };
            return await openKlantApiClient.CreateActorAsync(actorRequest);
        }

        return actor;
    }

    private async Task<Actor> GetOrCreateAfdelingActor(string identificatie)
    {
        var actor = await openKlantApiClient.QueryActorAsync(new ActorQuery
        {
            IndicatieActief = true,
            SoortActor = SoortActor.organisatorische_eenheid,
            ActoridentificatorObjectId = identificatie,
            ActoridentificatorCodeObjecttype = KnownAfdelingIdentificators.ObjectRegisterId.CodeObjecttype,
            ActoridentificatorCodeRegister = KnownAfdelingIdentificators.ObjectRegisterId.CodeRegister,
            ActoridentificatorCodeSoortObjectId = KnownAfdelingIdentificators.ObjectRegisterId.CodeSoortObjectId
        });

        if (actor != null)
        {
            return actor;
        }

        var afdelingen = await objectApiClient.GetAfdelingenByIdentificatie(identificatie);
        var afdeling = afdelingen.First();

        var actorRequest = new ActorRequest
        {
            SoortActor = SoortActor.organisatorische_eenheid,
            Naam = afdeling.Naam,
            IndicatieActief = true,
            Actoridentificator = new Actoridentificator
            {
                CodeObjecttype = KnownAfdelingIdentificators.ObjectRegisterId.CodeObjecttype,
                CodeRegister = KnownAfdelingIdentificators.ObjectRegisterId.CodeRegister,
                CodeSoortObjectId = KnownAfdelingIdentificators.ObjectRegisterId.CodeSoortObjectId,
                ObjectId = identificatie
            }
        };

        return await openKlantApiClient.CreateActorAsync(actorRequest);
    }

    private async Task<Actor> GetOrCreateGroepActor(string identificatie)
    {
        var actor = await openKlantApiClient.QueryActorAsync(new ActorQuery
        {
            IndicatieActief = true,
            SoortActor = SoortActor.organisatorische_eenheid,
            ActoridentificatorObjectId = identificatie,
            ActoridentificatorCodeObjecttype = KnownGroepIdentificators.ObjectRegisterId.CodeObjecttype,
            ActoridentificatorCodeRegister = KnownGroepIdentificators.ObjectRegisterId.CodeRegister,
            ActoridentificatorCodeSoortObjectId = KnownGroepIdentificators.ObjectRegisterId.CodeSoortObjectId
        });

        if (actor != null)
        {
            return actor;
        }

        var groepen = await objectApiClient.GetGroepenByIdentificatie(identificatie);
        var groep = groepen.First();

        var actorRequest = new ActorRequest
        {
            SoortActor = SoortActor.organisatorische_eenheid,
            Naam = groep.Naam,
            IndicatieActief = true,
            Actoridentificator = new Actoridentificator
            {
                CodeObjecttype = KnownGroepIdentificators.ObjectRegisterId.CodeObjecttype,
                CodeRegister = KnownGroepIdentificators.ObjectRegisterId.CodeRegister,
                CodeSoortObjectId = KnownGroepIdentificators.ObjectRegisterId.CodeSoortObjectId,
                ObjectId = identificatie
            }
        };

        return await openKlantApiClient.CreateActorAsync(actorRequest);
    }
}
