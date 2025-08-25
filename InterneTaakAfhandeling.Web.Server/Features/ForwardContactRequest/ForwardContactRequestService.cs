using InterneTaakAfhandeling.Common.Services;
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
    Task<ForwardContactRequestResponse> ForwardAsync(Guid internetaakId, ForwardContactRequestModel request);
}

public class ForwardContactRequestService(
    IOpenKlantApiClient openKlantApiClient,
    IObjectApiClient objectApiClient,
    IEmailService emailService,
    IEmailContentService emailContentService,
    IZakenApiClient zakenApiClient,
    IContactmomentenService contactmomentenService,
    ILogger<ForwardContactRequestService> logger) : IForwardContactRequestService
{
    public async Task<ForwardContactRequestResponse> ForwardAsync(Guid internetaakId,
        ForwardContactRequestModel request)
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

        var messages = await NotifyInternetaakActors(updatedInternetaak);

        return new ForwardContactRequestResponse
        {
            Internetaak = updatedInternetaak,
            NotificationResult = string.Join(", ", messages)
        };
    }


    private async Task<List<string>> NotifyInternetaakActors(Internetaak internetaken)
    {
        var notificationResults = new List<string> { "Contactverzoek succesvol doorgestuurd" };
        try
        {
            var emailResult = await ResolveActorsEmailAsync(internetaken);

            if (emailResult.NotFoundActors.Count > 0)
            {
                var notFoundMessage = emailResult.NotFoundActors.Select(actor => $"{actor.Naam}' heeft geen e-mailadres geregistreerd");
                notificationResults.AddRange(notFoundMessage);
            }

            if (emailResult.FoundEmails.Count > 0)
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

                var emailTasks = emailResult.FoundEmails.Select(async email =>
                {
                    var result = await emailService.SendEmailAsync(email,
                        $"Contactverzoek Doorgestuurd - {internetaken.Nummer}", emailContent);
                    return new { Email = email, Result = result };
                });

                var emailResults = await Task.WhenAll(emailTasks);

                var failedEmails = emailResults.Where(r => !r.Result.Success).ToList();
                if (failedEmails.Count > 0)
                {
                    var failedEmailsWithErrors =
                        string.Join(", ", failedEmails.Select(f => $"{f.Email} ({f.Result.Message})"));
                    logger.LogWarning("Some emails failed to send for internetaak {Number}: {FailedEmails}",
                        internetaken.Nummer, failedEmailsWithErrors);

                    var failedEmailAddresses = string.Join(", ", failedEmails.Select(f => f.Email));
                    notificationResults.Add(
                        $"E-mail verzending gedeeltelijk mislukt voor internetaak {internetaken.Nummer}. Mislukte e-mails: {failedEmailAddresses}");
                }
            }
            else
            {
                logger.LogInformation("No actor emails found for internetaken: {Number}, skipping",
                    internetaken.Nummer);
                notificationResults.Add(
                    $"No actor emails found for internetaak {internetaken.Nummer}, skipping email notifications");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing internetaken {Number}", internetaken.Nummer);
            notificationResults.Add($"Error processing internetaak {internetaken.Nummer}");
        }

        return notificationResults;
    }


    private async Task<ActorEmailResolutionResult> ResolveActorsEmailAsync(Internetaak internetaken)
    {
        var result = new ActorEmailResolutionResult();

        if (internetaken.ToegewezenAanActoren == null)
        {
            logger.LogWarning("No actor assigned to internetaak {Nummer}", internetaken.Nummer);
            return result;
        }


        foreach (var toegewezenAanActoren in internetaken.ToegewezenAanActoren)
        {
            var actor = await openKlantApiClient.GetActorAsync(toegewezenAanActoren.Uuid);

            var validCodeObjectTypes = new List<string>
            {
                KnownMedewerkerIdentificators.ObjectRegisterId.CodeObjecttype,
                KnownAfdelingIdentificators.ObjectRegisterId.CodeObjecttype,
                KnownGroepIdentificators.ObjectRegisterId.CodeObjecttype
            };

            if (actor.Actoridentificator == null ||
                !validCodeObjectTypes.Contains(actor.Actoridentificator.CodeObjecttype))
            {
                result.NotFoundActors.Add(actor);
                continue;
            }

            var objectId = actor.Actoridentificator.ObjectId;
            var actorIdentificator = actor.Actoridentificator;

            if (actorIdentificator.CodeSoortObjectId ==
                KnownMedewerkerIdentificators.EmailHandmatig.CodeSoortObjectId &&
                actorIdentificator.CodeRegister == KnownMedewerkerIdentificators.EmailHandmatig.CodeRegister)
            {
                result.FoundEmails.Add(objectId);
            }

            else if (actorIdentificator.CodeSoortObjectId ==
                     KnownMedewerkerIdentificators.ObjectRegisterId.CodeSoortObjectId &&
                     actorIdentificator.CodeRegister == KnownMedewerkerIdentificators.ObjectRegisterId.CodeRegister)
            {
                var objectRecords = await objectApiClient.GetObjectsByIdentificatie(objectId);
                switch (objectRecords.Count)
                {
                    case 0:
                        logger.LogWarning("No medewerker found in overigeobjecten for actorIdentificator {ObjectId}",
                            objectId);
                        result.NotFoundActors.Add(actor);
                        continue;
                    case > 1:
                        logger.LogWarning(
                            "Multiple objects found in overigeobjecten for actorIdentificator {ObjectId}. Expected exactly one match.",
                            objectId);
                        result.NotFoundActors.Add(actor);
                        continue;
                    default:
                        objectRecords.First().Data.EmailAddresses.ForEach(x =>
                        {
                            if (!string.IsNullOrEmpty(x) && EmailService.IsValidEmail(x))
                            {
                                result.FoundEmails.Add(x);
                            }
                            else
                            {
                                logger.LogWarning("Invalid email address found for object {ObjectId}", objectId);
                                result.NotFoundActors.Add(actor);
                            }
                        });
                        break;
                }
            }
        }

        return result;
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

        if (primaryActor != null) actors.Add(primaryActor);

        if (string.IsNullOrWhiteSpace(request.MedewerkerEmail)) return actors;

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

        var afdeling = await objectApiClient.GetAfdeling(identifier);

        if (afdeling == null) throw new InvalidDataException($"Afdeling with identifier {identifier} does not exist.");

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

    private async Task<Actor?> GetOrCreateGroepActor(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            return null;

        var groep = await objectApiClient.GetGroep(identifier);

        if (groep == null) throw new InvalidDataException($"Groep with identifier {identifier} does not exist.");

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