using InterneTaakAfhandeling.Common.Services.Emailservices.SmtpMailService;
using InterneTaakAfhandeling.Common.Services.ObjectApi;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;

namespace InterneTaakAfhandeling.Common.Services.Emailservices.Content
{
    public interface IInterneTaakEmailInputService
    {
        Task<InterneTakenEmailInput> FetchInterneTaakEmailInput(Internetaak interneTaak);
        Task<ActorEmailResolutionResult> ResolveActorsEmailAsync(IReadOnlyList<Actor> actors);
    }

    public class InterneTaakEmailInputService(
            IObjectApiClient objectApiClient
            ) : IInterneTaakEmailInputService
    {
        public Task<InterneTakenEmailInput> FetchInterneTaakEmailInput(Internetaak interneTaak)
        {
            return Task.FromResult(new InterneTakenEmailInput(interneTaak));
        }

        public async Task<ActorEmailResolutionResult> ResolveActorsEmailAsync(IReadOnlyList<Actor> actors)
        {
            var result = new ActorEmailResolutionResult();

            foreach (var actor in actors)
            {
                var actorIdentificator = actor.Actoridentificator;

                if (actorIdentificator.Matches(KnownMedewerkerIdentificators.EmailHandmatig))
                {
                    if (EmailService.IsValidEmail(actorIdentificator.ObjectId))
                    {
                        result.FoundEmails.Add(actorIdentificator.ObjectId);
                    }
                    else
                    {
                        result.Errors.Add($"actorIdentificator {actorIdentificator.ObjectId} is geen valide e-mailadres");
                    }
                }
                else if (actorIdentificator.Matches(KnownAfdelingIdentificators.ObjectRegisterId))
                {
                    var objectRecords = await objectApiClient.GetAfdelingenByIdentificatie(actorIdentificator.ObjectId);

                    if (objectRecords.Count == 0)
                    {
                        result.Errors.Add($"Geen afdeling gevonden in overigeobjecten voor actorIdentificator {actorIdentificator.ObjectId}");
                        continue;
                    }

                    if (objectRecords.Count > 1)
                    {
                        result.Errors.Add($"Meerdere afdelingen gevonden in overigeobjecten voor actorIdentificator {actorIdentificator.ObjectId}");
                        continue;
                    }

                    var afdeling = objectRecords.First();
                    var email = afdeling.Email;

                    if (!string.IsNullOrEmpty(email) && EmailService.IsValidEmail(email))
                    {
                        result.FoundEmails.Add(email);
                    }
                    else
                    {
                        result.Errors.Add($"Er is geen e-mailadres bekend voor afdeling {afdeling.Naam}");
                    }
                }
                else if (actorIdentificator.Matches(KnownGroepIdentificators.ObjectRegisterId))
                {
                    var objectRecords = await objectApiClient.GetGroepenByIdentificatie(actorIdentificator.ObjectId);

                    if (objectRecords.Count == 0)
                    {
                        result.Errors.Add($"Geen afdeling gevonden in overigeobjecten voor actorIdentificator {actorIdentificator.ObjectId}");
                        continue;
                    }

                    if (objectRecords.Count > 1)
                    {
                        result.Errors.Add($"Meerdere groepen gevonden in overigeobjecten voor actorIdentificator {actorIdentificator.ObjectId}");
                        continue;
                    }

                    var groep = objectRecords.First();
                    var email = groep.Email;

                    if (!string.IsNullOrEmpty(email) && EmailService.IsValidEmail(email))
                    {
                        result.FoundEmails.Add(email);
                    }
                    else
                    {
                        result.Errors.Add($"Er is geen e-mailadres bekend voor afdeling {groep.Naam}");
                    }
                }
                else if (actorIdentificator.Matches(KnownMedewerkerIdentificators.ObjectRegisterId))
                {
                    var objectRecords = await objectApiClient.GetMedewerkersByIdentificatie(actorIdentificator.ObjectId);

                    if (objectRecords.Count == 0)
                    {
                        result.Errors.Add($"Geen medewerker gevonden in overigeobjecten voor actorIdentificator {actorIdentificator.ObjectId}");
                        continue;
                    }

                    if (objectRecords.Count > 1)
                    {
                        result.Errors.Add($"Meerdere medewerkers gevonden in overigeobjecten voor actorIdentificator {actorIdentificator.ObjectId}");
                        continue;
                    }

                    objectRecords.First().EmailAddresses?.ForEach(x =>
                    {
                        if (!string.IsNullOrEmpty(x) && EmailService.IsValidEmail(x))
                        {
                            result.FoundEmails.Add(x);
                        }
                        else
                        {
                            result.Errors.Add($"E-mailadres voor medewerker {actorIdentificator.ObjectId} in objectenregistratie is niet valide");
                        }
                    });

                }
            }

            return result;
        }
    }

    public class ActorEmailResolutionResult
    {
        public List<string> FoundEmails { get; } = [];
        public List<string> Errors { get; } = [];
    }
}
