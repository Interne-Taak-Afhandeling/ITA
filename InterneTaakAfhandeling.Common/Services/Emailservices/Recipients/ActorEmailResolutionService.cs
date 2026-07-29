using InterneTaakAfhandeling.Common.Services.Emailservices.SmtpMailService;
using InterneTaakAfhandeling.Common.Services.ObjectApi;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;

namespace InterneTaakAfhandeling.Common.Services.Emailservices.Recipients
{
    public interface IActorEmailResolutionService
    {
        Task<ActorEmailResolutionResult> ResolveActorsEmailAsync(IReadOnlyList<Actor> actors);
    }

    public class ActorEmailResolutionService(
            IObjectApiClient objectApiClient
            ) : IActorEmailResolutionService
    {
        public async Task<ActorEmailResolutionResult> ResolveActorsEmailAsync(IReadOnlyList<Actor> actors)
        {
            var result = new ActorEmailResolutionResult();
            var medewerkerEmails = new List<string>();
            var groepEmails = new List<string>();

            foreach (var actor in actors)
            {
                var actorIdentificator = actor.Actoridentificator;

                if (actorIdentificator.Matches(KnownMedewerkerIdentificators.EmailHandmatig))
                {
                    if (EmailService.IsValidEmail(actorIdentificator.ObjectId))
                    {
                        medewerkerEmails.Add(actorIdentificator.ObjectId);
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
                        groepEmails.Add(email);
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
                        result.Errors.Add($"Geen groep gevonden in overigeobjecten voor actorIdentificator {actorIdentificator.ObjectId}");
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
                        groepEmails.Add(email);
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
                            medewerkerEmails.Add(x);
                        }
                        else
                        {
                            result.Errors.Add($"E-mailadres voor medewerker {actorIdentificator.ObjectId} in objectenregistratie is niet valide");
                        }
                    });

                }
            }

            // Precedence: a medewerker email always takes priority over the afdeling/groep mailbox,
            // so that no more than one party ever receives a notification. If the medewerker doesn't
            // resolve to a valid address, the system falls back to the groep.
            result.FoundEmails.AddRange(medewerkerEmails.Count > 0 ? medewerkerEmails : groepEmails);

            return result;
        }
    }

    public class ActorEmailResolutionResult
    {
        public List<string> FoundEmails { get; } = [];
        public List<string> Errors { get; } = [];
    }
}
