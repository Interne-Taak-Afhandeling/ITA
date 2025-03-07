using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using InterneTaakAfhandeling.Poller.Services.Openklant;
using InterneTaakAfhandeling.Poller.Services.Emailservices.SmtpMailService;
using InterneTaakAfhandeling.Poller.Services.Openklant.Models;
using InterneTaakAfhandeling.Poller.Services.ObjectApi;
using InterneTaakAfhandeling.Poller.Services.Emailservices.Content;
using InterneTaakAfhandeling.Poller.Services.ObjectApi.Models;

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

    public InternetakenNotifier(
        IOpenKlantApiClient openKlantApiClient,
        IConfiguration configuration,
        IEmailService emailService,
        ILogger<InternetakenNotifier> logger,
        IObjectApiClient objectApiClient,
        IEmailContentService emailContentService)
    {
        _openKlantApiClient = openKlantApiClient ?? throw new ArgumentNullException(nameof(openKlantApiClient));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _objectApiClient = objectApiClient ?? throw new ArgumentNullException(nameof(objectApiClient));
        _hourThreshold = configuration.GetValue<int>("InternetakenNotifier:HourThreshold");
        _apiBaseUrl = configuration.GetValue<string>("OpenKlantApi:BaseUrl")
            ?? throw new ArgumentException("OpenKlantApi:BaseUrl configuration is missing");
        _emailContentService = emailContentService ?? throw new ArgumentNullException(nameof(emailContentService));
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
                    var emailContent = _emailContentService.BuildInternetakenEmailContent(taak);
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
        var actorId = request.ToegewezenAanActoren?.FirstOrDefault()?.Uuid;
        if (string.IsNullOrEmpty(actorId))
        {
            _logger.LogWarning("No toegewezen aan actor found for internetaken {Nummer}", request.Nummer);
            return string.Empty;
        }

        var actor = await _openKlantApiClient.GetActorAsync(actorId);
        if (actor?.Actoridentificator == null || actor.Actoridentificator.CodeObjecttype != "mdw")
        {
            _logger.LogWarning("Invalid actor found for actor {Uuid}. ActorIdentificator: {ActorIdentificator}", 
                actorId, actor?.Actoridentificator);
            return string.Empty;
        }

        var objectId = actor.Actoridentificator.ObjectId;
        var actorIdentificator = actor.Actoridentificator;

        // Check if we need to fetch email from object API
        if (actorIdentificator.CodeSoortObjectId == "idf" &&
            actorIdentificator.CodeRegister == "obj" &&
            !EmailService.IsValidEmail(objectId))
        {
            var objectRecords = await _objectApiClient.GetObjectsByIdentificatie(objectId);
            if (objectRecords.Count == 0)
            {
                _logger.LogWarning("No object found for identificatie {ObjectId}", objectId);
                return string.Empty;
            }

            if (objectRecords.Count > 1)
            {
                _logger.LogWarning("Multiple objects found for identificatie {ObjectId}. Expected exactly one match.", objectId);
                return string.Empty;
            }

            return objectRecords.First().Data?.Emails?.FirstOrDefault()?.Email ?? string.Empty;
        }

        return objectId;
    }

    private List<Internetaken> FilterNewInternetaken(InternetakenResponse internetaken)
    {
        var thresholdTime = DateTimeOffset.UtcNow.AddHours(-_hourThreshold);
        return internetaken.Results
           // .Where(item => item.ToegewezenOp > thresholdTime)
            .ToList();
    }
}
