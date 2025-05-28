using InterneTaakAfhandeling.Common.Services.ObjectApi;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using InterneTaakAfhandeling.Common.Services.ZakenApi;
using InterneTaakAfhandeling.Common.Services.ZakenApi.Models;
using InterneTaakAfhandeling.Poller.Services.Emailservices.Content;
using InterneTaakAfhandeling.Poller.Services.Emailservices.SmtpMailService;
using InterneTaakAfhandeling.Poller.Services.NotifierState;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace InterneTaakAfhandeling.Poller.Features;

public interface IInternetakenProcessor
{
    Task NotifyAboutNewInternetakenAsync();
}

public class InternetakenNotifier : IInternetakenProcessor
{
    private readonly IOpenKlantApiClient _openKlantApiClient;
    private readonly IEmailService _emailService;
    private readonly ILogger<InternetakenNotifier> _logger;
    private readonly IObjectApiClient _objectApiClient;
    private readonly string _apiBaseUrl;
    private readonly IEmailContentService _emailContentService;
    private readonly IZakenApiClient _zakenApiClient;
    private readonly INotifierStateService _notifierStateService;
    private readonly IContactmomentenService _contactmomentenService;
    private const string EmailCodeSoortObjectId = "email";
    private const string HandmatigCodeRegister = "handmatig";

    public InternetakenNotifier(
        IOpenKlantApiClient openKlantApiClient,
        IConfiguration configuration,
        IEmailService emailService,
        ILogger<InternetakenNotifier> logger,
        IObjectApiClient objectApiClient,
        IEmailContentService emailContentService,
        IZakenApiClient zakenApiClient,
        INotifierStateService notifierStateService,
        IContactmomentenService contactmomentenService)
    {
        _openKlantApiClient = openKlantApiClient ?? throw new ArgumentNullException(nameof(openKlantApiClient));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _objectApiClient = objectApiClient ?? throw new ArgumentNullException(nameof(objectApiClient));
        _apiBaseUrl = configuration.GetValue<string>("OpenKlantApi:BaseUrl")
            ?? throw new ArgumentException("OpenKlantApi:BaseUrl configuration is missing");
        _emailContentService = emailContentService ?? throw new ArgumentNullException(nameof(emailContentService));
        _zakenApiClient = zakenApiClient ?? throw new ArgumentNullException(nameof(zakenApiClient));
        _notifierStateService = notifierStateService ?? throw new ArgumentNullException(nameof(notifierStateService));
        _contactmomentenService = contactmomentenService ?? throw new ArgumentNullException(nameof(contactmomentenService));
    }

    public async Task NotifyAboutNewInternetakenAsync()
    {
        var page = "internetaken";
        var notifierState = await _notifierStateService.StartJobAsync();

        ProcessingResult processResult = new(true, notifierState.LastInternetakenId, notifierState.LastInternetakenToegewezenOp, "");

        List<Internetaak> internetakens = [];

        while (!string.IsNullOrEmpty(page))
        {
            var response = await _openKlantApiClient.GetInternetakenAsync(page);
            if (response?.Results == null || response.Results.Count == 0)
                break;
            var newInternetaken = response.Results.Where(item => item.ToegewezenOp > notifierState.LastInternetakenToegewezenOp);
            if (!newInternetaken.Any())
                break;
            internetakens.AddRange(newInternetaken);

            page = response.Next?.Replace(_apiBaseUrl, string.Empty);
        }
        internetakens = [.. internetakens.OrderBy(x => x.ToegewezenOp)];
        if (internetakens.Count != 0)
        {
            foreach (var taak in internetakens)
            {

                processResult = await ProcessInternetakenAsync(taak);

                if (!processResult.Success)
                {
                    _logger.LogError("Failed to process internetaken: {ErrorMessage}", processResult.ErrorMessage);
                    break;
                }

                await _notifierStateService.TrackInternetakenAsync(processResult.LastInternetakenId, processResult.LastInternetakenToegewezenOp);
            }
        }
        else
        {
            _logger.LogInformation("No new internetaken found");
        }

        await _notifierStateService.FinishJobAsync(processResult);

    }

    public async Task<ProcessingResult> ProcessInternetakenAsync(Internetaak internetaken)
    {
        bool success = true;
        string? errorMessage = null;

        try
        {
            _logger.LogInformation("Processing internetaken: {Number}", internetaken.Nummer);

            var actorEmails = await ResolveActorsEmailAsync(internetaken);
            if (actorEmails.Count > 0)
            {
                var klantContact = await _openKlantApiClient.GetKlantcontactAsync(internetaken.AanleidinggevendKlantcontact.Uuid);

                var digitaleAdress = klantContact.Expand?.HadBetrokkenen?.SelectMany(x => x?.Expand?.DigitaleAdressen).ToList();

                Zaak? zaak = null;

                var onderwerpObjectId = _contactmomentenService.GetZaakOnderwerpObject(klantContact);

                if (!string.IsNullOrEmpty(onderwerpObjectId))
                {
                    zaak = await _zakenApiClient.GetZaakAsync(onderwerpObjectId);
                }

                var emailContent = _emailContentService.BuildInternetakenEmailContent(internetaken, klantContact, digitaleAdress, zaak);

                await Task.WhenAll(actorEmails.Select(email => _emailService.SendEmailAsync(email, $"Nieuw contactverzoek - {internetaken.Nummer}", emailContent)));

                _logger.LogInformation("Successfully processed internetaken: {Number}", internetaken.Nummer);
            }
            else
            {
                _logger.LogInformation("No actor emails found for internetaken: {Number}, skipping", internetaken.Nummer);
                errorMessage = "No actor emails found";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing internetaken {Number}", internetaken.Nummer);
            success = false;
            errorMessage = ex.Message;
        }

        return new ProcessingResult(success, Guid.Parse(internetaken.Uuid), internetaken.ToegewezenOp ?? DateTimeOffset.MinValue, errorMessage);

    }



    private async Task<List<string>> ResolveActorsEmailAsync(Internetaak internetaken)
    {
        if (internetaken.ToegewezenAanActoren == null)
        {
            _logger.LogWarning("No actor assigned to internetaak {Nummer}", internetaken.Nummer);
            return [];
        }

        var emailAddresses = new List<string>();

        foreach (var toegewezenAanActoren in internetaken.ToegewezenAanActoren)
        {
            var actor = await _openKlantApiClient.GetActorAsync(toegewezenAanActoren.Uuid);

            List<string> validCodeObjectTypes = new List<string> { "mdw", "afd", "grp" };

            if (actor?.Actoridentificator == null ||
                !validCodeObjectTypes.Contains(actor.Actoridentificator.CodeObjecttype))
            {
                continue;
            }

            var objectId = actor.Actoridentificator.ObjectId;
            var actorIdentificator = actor.Actoridentificator;

            if (actorIdentificator.CodeSoortObjectId == EmailCodeSoortObjectId &&
                actorIdentificator.CodeRegister == HandmatigCodeRegister)
            {
                emailAddresses.Add(objectId);
            }
            // Check if we need to fetch email from object API
            else if (actorIdentificator.CodeSoortObjectId == "idf" &&
                actorIdentificator.CodeRegister == "obj")
            {
                var objectRecords = await _objectApiClient.GetObjectsByIdentificatie(objectId);
                if (objectRecords.Count == 0)
                {
                    _logger.LogWarning("No medewerker found in overigeobjecten for actorIdentificator {ObjectId}", objectId);
                    continue;
                }

                if (objectRecords.Count > 1)
                {
                    _logger.LogWarning("Multiple objects found in overigeobjecten for actorIdentificator {ObjectId}. Expected exactly one match.", objectId);
                    continue;
                }

                objectRecords.First().Data?.EmailAddresses?.ForEach(x =>
              {

                  if (!string.IsNullOrEmpty(x) && EmailService.IsValidEmail(x))
                  {
                      emailAddresses.Add(x);
                  }
                  else
                  {
                      _logger.LogWarning("Invalid email address found for object {ObjectId}", objectId);
                  }
              });

            }
        }

        return emailAddresses;
    }


}
