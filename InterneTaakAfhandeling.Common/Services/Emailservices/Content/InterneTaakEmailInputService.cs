using InterneTaakAfhandeling.Common.Services.Emailservices.SmtpMailService;
using InterneTaakAfhandeling.Common.Services.ObjectApi;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using InterneTaakAfhandeling.Common.Services.ZakenApi;
using InterneTaakAfhandeling.Common.Services.ZakenApi.Models;

namespace InterneTaakAfhandeling.Common.Services.Emailservices.Content
{
    public interface IInterneTaakEmailInputService
    {
        Task<InterneTakenEmailInput> FetchInterneTaakEmailInput(Internetaak interneTaak);
        Task<ActorEmailResolutionResult> ResolveActorsEmailAsync(IReadOnlyList<Actor> actors);
    }

    public class InterneTaakEmailInputService(
            IOpenKlantApiClient openKlantApiClient,
            IContactmomentenService contactmomentenService,
            IZakenApiClient zakenApiClient,
            IObjectApiClient objectApiClient
            ) : IInterneTaakEmailInputService
    {
        public async Task<InterneTakenEmailInput> FetchInterneTaakEmailInput(Internetaak interneTaak)
        {
            var klantContact =
                    await openKlantApiClient.GetKlantcontactAsync(interneTaak.AanleidinggevendKlantcontact.Uuid);

            var digitaleAdressen = klantContact.Expand?.HadBetrokkenen
                ?.SelectMany(x => x.Expand?.DigitaleAdressen ?? [])
                .ToArray() ?? [];

            Zaak? zaak = null;

            var onderwerpObjectId = contactmomentenService.GetZaakOnderwerpObject(klantContact);

            if (!string.IsNullOrEmpty(onderwerpObjectId))
                zaak = await zakenApiClient.GetZaakAsync(onderwerpObjectId);

            return new(interneTaak, klantContact, digitaleAdressen, zaak);
        }

        public async Task<ActorEmailResolutionResult> ResolveActorsEmailAsync(IReadOnlyList<Actor> actors)
        {
            var result = new ActorEmailResolutionResult();

            foreach (var actor in actors)
            {
                var actorIdentificator = actor.Actoridentificator;

                if (actorIdentificator.Matches(KnownMedewerkerIdentificators.EmailHandmatig))
                {
                    result.FoundEmails.Add(actorIdentificator.ObjectId);
                }
                else if (actorIdentificator.Matches(KnownAfdelingIdentificators.ObjectRegisterId))
                {
                    var afdeling = await objectApiClient.GetAfdeling(actorIdentificator.ObjectId);

                    if (string.IsNullOrWhiteSpace(afdeling.Record.Data.Email))
                    {
                        result.Errors.Add($"Er is geen e-mailadres bekend voor afdeling {afdeling.Record.Data.Naam}");
                    }
                    else
                    {
                        result.FoundEmails.Add(afdeling.Record.Data.Email);
                    }
                }
                else if (actorIdentificator.Matches(KnownGroepIdentificators.ObjectRegisterId))
                {
                    var groep = await objectApiClient.GetGroep(actorIdentificator.ObjectId);

                    if (string.IsNullOrWhiteSpace(groep.Record.Data.Email))
                    {
                        result.Errors.Add($"Er is geen e-mailadres bekend voor groep {groep.Record.Data.Naam}");
                    }
                    else
                    {
                        result.FoundEmails.Add(groep.Record.Data.Email);
                    }
                }
                else if (actorIdentificator.Matches(KnownMedewerkerIdentificators.ObjectRegisterId))
                {
                    var objectRecords = await objectApiClient.GetObjectsByIdentificatie(actorIdentificator.ObjectId);

                    if (objectRecords.Count == 0)
                    {
                        result.Errors.Add($"No medewerker found in overigeobjecten for actorIdentificator {actorIdentificator.ObjectId}");
                        continue;
                    }

                    if (objectRecords.Count > 1)
                    {
                        result.Errors.Add($"Multiple objects found in overigeobjecten for actorIdentificator {actorIdentificator.ObjectId}. Expected exactly one match.");
                        continue;
                    }

                    objectRecords.First().Data?.EmailAddresses?.ForEach(x =>
                    {
                        if (!string.IsNullOrEmpty(x) && EmailService.IsValidEmail(x))
                        {
                            result.FoundEmails.Add(x);
                        }
                        else
                        {
                            result.Errors.Add($"Invalid email address found for object {actorIdentificator.ObjectId}");
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
