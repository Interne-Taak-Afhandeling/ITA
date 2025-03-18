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
            if (response?.Results == null || !response.Results.Any())
                break;

            var newTaken = FilterNewInternetaken(response);
            if (newTaken.Any())
            {
                await ProcessInternetakenBatchAsync(newTaken);
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

                var emailTo = await ResolveActorEmailAsync(taak);
                if (!string.IsNullOrEmpty(emailTo))
                {
                 var klantContact   = await _openKlantApiClient.GetKlantcontactAsync(taak.AanleidinggevendKlantcontact.Uuid);
                    var betrokkenen = await _openKlantApiClient.GetBetrokkeneAsync(klantContact.HadBetrokkenen.First().Uuid);
                 var digitaleAdresTasks = betrokkenen.DigitaleAdressen
                                         .Select(async x => await _openKlantApiClient.GetDigitaleAdresAsync(x.Uuid))
                                         .ToList();
                    var digitaleAdress = await Task.WhenAll(
                        betrokkenen.DigitaleAdressen.Select(async x => await _openKlantApiClient.GetDigitaleAdresAsync(x.Uuid))
                    );

                    Zaak? zaak = null;
                    var onderwerpObjectId = klantContact.Expand?.GingOverOnderwerpobjecten?.FirstOrDefault()?.Onderwerpobjectidentificator?.ObjectId;
                    if (!string.IsNullOrEmpty(onderwerpObjectId))
                    {
                        zaak = await _zakenApiClient.GetZaakAsync(onderwerpObjectId);
                    }
                    
                    var emailContent = _emailContentService.BuildInternetakenEmailContent(taak, klantContact, digitaleAdress.ToList(), zaak);

                    await _emailService.SendEmailAsync(emailTo, $"New Internetaken - {taak.Nummer}", emailContent);
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

    private async Task<string> ResolveActorEmailAsync(Internetaken request)
    {
        if (request.ToegewezenAanActoren == null)
        {
            _logger.LogWarning("No actor assigned to internetaak {Nummer}", request.Nummer);
            return string.Empty;
        }

        foreach (var toegewezenAanActoren in request.ToegewezenAanActoren)
        {
            
            var actor = await _openKlantApiClient.GetActorAsync(toegewezenAanActoren.Uuid);
            if (actor?.Actoridentificator == null || actor.Actoridentificator.CodeObjecttype != "mdw")
            {              
                continue;
            }

            var objectId = actor.Actoridentificator.ObjectId;
            var actorIdentificator = actor.Actoridentificator;

            // Check if we need to fetch email from object API
            // note, this is a temporary solution. https://dimpact.atlassian.net/browse/PC-983 will provide
            // a better way to distinguish actors with email address from actors with an id
            if (actorIdentificator.CodeSoortObjectId == "idf" &&
                actorIdentificator.CodeRegister == "obj" &&
                !EmailService.IsValidEmail(objectId))
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

                var emailAddress = objectRecords.First().Data?.Emails?.FirstOrDefault()?.Email;

                if (!string.IsNullOrEmpty(emailAddress) && EmailService.IsValidEmail(emailAddress))
                {
                    return emailAddress;
                }

                _logger.LogWarning("Invalid email address found for object {ObjectId}: {EmailAddress}", objectId, emailAddress);
                continue;
            }
            return objectId;
        }

        return string.Empty;
    }


    private List<Internetaken> FilterNewInternetaken(InternetakenResponse internetaken)
    {
        var thresholdTime = DateTimeOffset.UtcNow.AddHours(-_hourThreshold);
        return internetaken.Results
          //  .Where(item => item.ToegewezenOp > thresholdTime)
            .ToList();
    }
}
