using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using InterneTaakAfhandeling.Poller.Services.Openklant;
using InterneTaakAfhandeling.Poller.Services.Emailservices.SmtpMailService;
using InterneTaakAfhandeling.Poller.Services.Openklant.Models;
using InterneTaakAfhandeling.Poller.Services.ObjectApi;
using InterneTaakAfhandeling.Poller.Services.Emailservices.Content;
using InterneTaakAfhandeling.Poller.Services.ZakenApi;
using InterneTaakAfhandeling.Poller.Services.ZakenApi.Models;

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
    private readonly int _hourThreshold;
    private readonly IEmailContentService _emailContentService;
    private readonly IZakenApiClient _zakenApiClient;

    private const string EmailCodeSoortObjectId = "email";
    private const string HandmatigCodeRegister = "handmatig";

    public InternetakenNotifier(
        IOpenKlantApiClient openKlantApiClient,
        IConfiguration configuration,
        IEmailService emailService,
        ILogger<InternetakenNotifier> logger,
        IObjectApiClient objectApiClient,
        IEmailContentService emailContentService,
        IZakenApiClient zakenApiClient)
    {
        _openKlantApiClient = openKlantApiClient ?? throw new ArgumentNullException(nameof(openKlantApiClient));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _objectApiClient = objectApiClient ?? throw new ArgumentNullException(nameof(objectApiClient));
        _hourThreshold = configuration.GetValue<int>("InternetakenNotifier:HourThreshold");
        _apiBaseUrl = configuration.GetValue<string>("OpenKlantApi:BaseUrl")
            ?? throw new ArgumentException("OpenKlantApi:BaseUrl configuration is missing");
        _emailContentService = emailContentService ?? throw new ArgumentNullException(nameof(emailContentService));
        _zakenApiClient = zakenApiClient;
    }

    public async Task NotifyAboutNewInternetakenAsync()
    {
        var page = "internetaken";
        while (!string.IsNullOrEmpty(page))
        {
            var response = await _openKlantApiClient.GetInternetakenAsync(page);
            if (response?.Results == null || response.Results.Count == 0)
                break;

            var newTaken = FilterNewInternetaken(response);
            if (newTaken.Count != 0)
            {
                await ProcessInternetakenBatchAsync(newTaken);
            }
            else
            {
                _logger.LogInformation("No new internetaken found");
            }

            page = response.Next?.Replace(_apiBaseUrl, string.Empty);
        }
    }

    private async Task ProcessInternetakenBatchAsync(List<Internetaken> internetakens)
    {
        foreach (var taak in internetakens)
        {
            try
            {
                _logger.LogInformation("Processing internetaken: {Number}", taak.Nummer);

                var actorEmails = await ResolveActorsEmailAsync(taak);
                if (actorEmails.Count > 0)
                {
                    var klantContact = await _openKlantApiClient.GetKlantcontactAsync(taak.AanleidinggevendKlantcontact.Uuid);

                    var digitaleAdress = klantContact.Expand?.HadBetrokkenen?.SelectMany(x => x?.Expand?.DigitaleAdressen).ToList();

                    Zaak? zaak = null;

                    var onderwerpObjectId = klantContact.Expand?.GingOverOnderwerpobjecten?.FirstOrDefault()?.Onderwerpobjectidentificator?.ObjectId;
                    if (!string.IsNullOrEmpty(onderwerpObjectId))
                    {
                        zaak = await _zakenApiClient.GetZaakAsync(onderwerpObjectId);
                    }

                    var emailContent = _emailContentService.BuildInternetakenEmailContent(taak, klantContact, digitaleAdress, zaak);
                    actorEmails.ForEach(async email => await _emailService.SendEmailAsync(email, $"Nieuw contactverzoek - {taak.Nummer}", emailContent));
                   _logger.LogInformation("Successfully processed internetaken: {Number}", taak.Nummer);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing internetaken {Number}", taak.Nummer);
                throw;
            }
        }
    }

    private async Task<List<string>> ResolveActorsEmailAsync(Internetaken internetaken)
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
                      _logger.LogWarning("Invalid email address found for object {ObjectId}: {EmailAddress}", objectId, x);
                    }
                });

            }
        }

        return emailAddresses;
    }


    private List<Internetaken> FilterNewInternetaken(InternetakenResponse internetaken)
    {
        var thresholdTime = DateTimeOffset.UtcNow.AddHours(-1 * _hourThreshold);
        return internetaken.Results
            .Where(item => item.ToegewezenOp > thresholdTime)
            .ToList();
    }
}
