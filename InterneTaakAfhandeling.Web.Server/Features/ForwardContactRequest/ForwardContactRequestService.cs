using InterneTaakAfhandeling.Common.Services.Klantcontacten;
using InterneTaakAfhandeling.Common.Services.Internetaken;
using System.ComponentModel.DataAnnotations;
using InterneTaakAfhandeling.Common.Helpers;
using InterneTaakAfhandeling.Common.ApiClients;
using InterneTaakAfhandeling.Common.Services.Actors;
using InterneTaakAfhandeling.Common.Services.Email.Content;
using InterneTaakAfhandeling.Common.Services.Email.Recipients;
using InterneTaakAfhandeling.Common.Services.Email.SmtpMailService;
using InterneTaakAfhandeling.Common.ApiClients.ObjectApi;
using InterneTaakAfhandeling.Common.ApiClients.OpenKlantApi;
using InterneTaakAfhandeling.Common.ApiClients.OpenKlantApi.Models;

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
    IActorEmailResolutionService actorEmailResolutionService,
    IActorService actorService,
    IKlantcontactService klantcontactService,
    IInternetaakService internetaakService,
    IConfiguration configuration) : IForwardContactRequestService
{
    private readonly string _itaBaseUrl = configuration.GetValue<string>("Ita:BaseUrl")
        ?? throw new ArgumentException("Ita:BaseUrl configuration is missing");
    public async Task<ForwardContactRequestResponse> ForwardAsync(Guid internetaakId,
        ForwardContactRequestModel request)
    {
        var actors = await GetTargetActors(request);

        var actorEmailResult = await actorEmailResolutionService.ResolveActorsEmailAsync(actors);

        // Reject the request upfront if no actor resolves to a usable email address. 
        // Then, the internetaak is patched with the new actors and, on success, those recipients are notified by email.
        ValidateEmailRecipients(actorEmailResult, request);

        var recipients = actors
            .Select(x => x.Naam)
            .OfType<string>()
            .Where(naam => !string.IsNullOrWhiteSpace(naam))
            .ToList();

        var updatedInternetaak = await PatchInternetaakActors(internetaakId, actors);

        if (updatedInternetaak.AanleidinggevendKlantcontact?.Uuid == null)
        {
            logger.LogError("Uuid van AanleidinggevendKlantcontact onbekend voor internetaak {Uuid}", updatedInternetaak.Uuid);
            return new ForwardContactRequestResponse
            {
                Internetaak = updatedInternetaak,
                NotificationResult = GenericError,
                Recipients = recipients
            };
        }

        //Although the AanleidinggevendKlantcontact was returned from the patch endpoint, it does not contain all details (like Nummer) that are needed for the email notification. Therefore, we need to fetch the full Klantcontact details.
        updatedInternetaak.AanleidinggevendKlantcontact = await klantcontactService.GetKlantcontactAsync(updatedInternetaak.AanleidinggevendKlantcontact.Uuid);

        var (notficationResult, notificationSentToAllRecipients) = await NotifyInternetaakActors(updatedInternetaak, actorEmailResult);

        return new ForwardContactRequestResponse
        {
            Internetaak = updatedInternetaak,
            NotificationResult = notficationResult,
            NotificationSentToAllRecipients = notificationSentToAllRecipients,
            Recipients = recipients
        };
    }

    private void ValidateEmailRecipients(ActorEmailResolutionResult actorEmailResult, ForwardContactRequestModel request)
    {
        if (actorEmailResult.Errors.Count != 0)
        {
            logger.LogWarning("Email address resolution errors: \n{Errors}", string.Join("\n", actorEmailResult.Errors));
        }

        if (actorEmailResult.FoundEmails.Count < 1)
        {
            var doelType = !string.IsNullOrWhiteSpace(request.Afdeling) ? "afdeling" : "groep";
            throw new ValidationException($"Selecteer een medewerker: de geselecteerde {doelType} heeft geen mailbox.");
        }
    }

    private async Task<Internetaak> PatchInternetaakActors(Guid internetaakId, List<Actor> actors)
    {
        var internetaak = await internetaakService.GetInternetaakByIdAsync(internetaakId);

        var internetakenUpdateRequest = new InternetakenPatchActorsRequest
        {
            ToegewezenAanActoren = [.. actors.Select(x => new UuidObject { Uuid = Guid.Parse(x.Uuid) })]
        };

        return await openKlantApiClient.PatchInternetaakActorAsync(internetakenUpdateRequest, internetaak.Uuid);
    }

    private const string GenericError = "Het contactverzoek is doorgestuurd, maar hiervan kon geen e-mailnotificatie verstuurd worden";

    private async Task<(string Message, bool SentToAllRecipients)> NotifyInternetaakActors(Internetaak internetaken, ActorEmailResolutionResult actorEmailResult)
    {
        try
        {
            if (!emailService.IsConfiguredCorrectly())
                return (GenericError, false);

            var contactmomentNummer = (internetaken.AanleidinggevendKlantcontact?.Nummer) ?? throw new InvalidOperationException(
                    $"AanleidinggevendKlantcontact.Nummer ontbreekt voor internetaak {internetaken.Nummer}");
            var emailContent = emailContentService.BuildInternetakenEmailContent(internetaken, _itaBaseUrl);
            var subject = $"Contactverzoek Doorgestuurd - {contactmomentNummer}";

            var sendResults = await SendEmailsAsync(actorEmailResult.FoundEmails, subject, emailContent);

            return GetResultMessage(sendResults, actorEmailResult);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing e-mail notifications for forwarded interne taak {Number}", SecureLogging.SanitizeAndTruncate(internetaken.Nummer));
            return (GenericError, false);
        }
    }

    private async Task<EmailResult[]> SendEmailsAsync(IEnumerable<string> emails, string subject, string content)
    {
        var tasks = emails.Select(email => emailService.SendEmailAsync(email, subject, content));
        return await Task.WhenAll(tasks);
    }

    private static (string Message, bool SentToAllRecipients) GetResultMessage(EmailResult[] results, ActorEmailResolutionResult actorEmailResult)
    {
        var failedEmails = results.Where(r => !r.Success).ToList();
        var anySuccess = results.Length > 0 && results.Length != failedEmails.Count;

        if (!anySuccess)
            return (GenericError, false);

        if (failedEmails.Any())
            return ($"Contactverzoek doorgestuurd, maar {failedEmails.Count} email(s) notificaties mislukte tijdens het doorsturen.", false);

        if (actorEmailResult.Errors.Any())
            return ($"Het contactverzoek is doorgestuurd, maar niet elke e-mailnotificatie kon verstuurd worden: \n{string.Join("\n", actorEmailResult.Errors)}", false);

        return ("Contactverzoek succesvol doorgestuurd", true);
    }

    private async Task<List<Actor>> GetTargetActors(ForwardContactRequestModel request)
    {
        var actors = new List<Actor>();

        if(!string.IsNullOrWhiteSpace(request.Medewerker))
        {
            actors.Add(await GetOrCreateMedewerkerActorByObjectRegisterId(request.Medewerker));
        }

        if(!string.IsNullOrWhiteSpace(request.Afdeling))
        {
            actors.Add(await GetOrCreateAfdelingActor(request.Afdeling));
        }

        if(!string.IsNullOrWhiteSpace(request.Groep))
        {
            actors.Add(await GetOrCreateGroepActor(request.Groep));
        }

        return actors;
    }

    private async Task<Actor> GetOrCreateMedewerkerActorByObjectRegisterId(string identificatie)
    {
        var actor = await actorService.QueryActorAsync(new ActorQuery
        {
            ActoridentificatorCodeObjecttype = KnownMedewerkerIdentificators.ObjectRegisterId.CodeObjecttype,
            ActoridentificatorCodeRegister = KnownMedewerkerIdentificators.ObjectRegisterId.CodeRegister,
            ActoridentificatorCodeSoortObjectId = KnownMedewerkerIdentificators.ObjectRegisterId.CodeSoortObjectId,
            IndicatieActief = true,
            SoortActor = SoortActor.medewerker,
            ActoridentificatorObjectId = identificatie
        });

        if (actor != null)
            return actor;

        var medewerkers = await objectApiClient.GetMedewerkersByIdentificatie(identificatie);

        var medewerker = medewerkers.Count switch
        {
            0 => throw new InvalidOperationException($"Medewerker met identificatie '{identificatie}' niet gevonden in het objectenregister."),
            > 1 => throw new InvalidOperationException($"Meerdere medewerkers gevonden voor identificatie '{identificatie}' in het objectenregister."),
            _ => medewerkers.Single()
        };

        var actorRequest = new ActorRequest
        {
            SoortActor = SoortActor.medewerker,
            Naam = medewerker.VolledigeNaam ?? $"{medewerker.Voornaam} {medewerker.Achternaam}".Trim(),
            IndicatieActief = true,
            Actoridentificator = new Actoridentificator
            {
                CodeObjecttype = KnownMedewerkerIdentificators.ObjectRegisterId.CodeObjecttype,
                CodeRegister = KnownMedewerkerIdentificators.ObjectRegisterId.CodeRegister,
                CodeSoortObjectId = KnownMedewerkerIdentificators.ObjectRegisterId.CodeSoortObjectId,
                ObjectId = identificatie
            }
        };

        return await openKlantApiClient.CreateActorAsync(actorRequest);
    }

    private async Task<Actor> GetOrCreateMedewerkerActor(string email)
    {
        var actor = await actorService.QueryActorAsync(new ActorQuery
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
        var actor = await actorService.QueryActorAsync(new ActorQuery
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
        var actor = await actorService.QueryActorAsync(new ActorQuery
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
