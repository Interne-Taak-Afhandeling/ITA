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
        var nextUrl = "internetaken";

        while (!string.IsNullOrEmpty(nextUrl))
        {
            var internetaken = await _openKlantApiClient.GetInternetakenAsync(nextUrl);

            if (internetaken?.Results == null)
            {
                _logger.LogInformation("No new internetaken found");
                break;
            }

            var filteredResults = FilterNewInternetaken(internetaken);

            if (!filteredResults.Any())
            {
                _logger.LogInformation("No new internetaken found within the last {HourThreshold} hour(s)", _hourThreshold);
                break;
            }

            await ProcessInternetakenBatchAsync(filteredResults);
            nextUrl = GetNextPageUrl(internetaken.Next);
        }
    }
     
     

    private string GetNextPageUrl(string? nextUrl)
        => string.IsNullOrEmpty(nextUrl) ? string.Empty : nextUrl.Replace(_apiBaseUrl, "");

    private async Task ProcessInternetakenBatchAsync(List<Internetaken> internetaken)
    {
        _logger.LogInformation("Starting to process {Count} internetaken", internetaken.Count);
        await Task.WhenAll(internetaken.Select(ProcessInternetakenItemAsync));
    }

    private async Task ProcessInternetakenItemAsync(Internetaken request)
    {
        try
        {
            _logger.LogInformation("Processing internetaken: {Number}", request.Nummer);

            var emailTo = await ResolveActorEmailAsync(request);
            if (string.IsNullOrEmpty(emailTo))
            {
                _logger.LogWarning("No email address found for internetaken {Number}", request.Nummer);
                return;
            }
            var emailContent = _emailContentService.BuildInternetakenEmailContent(request);
            await _emailService.SendEmailAsync(emailTo, $"New Internetaken - {request.Nummer}", emailContent);
             
            _logger.LogInformation("Successfully processed internetaken: {Number}", request.Nummer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing internetaken {Number}", request.Nummer);
            throw;
        }
    }
 
    private async Task<string> ResolveActorEmailAsync(Internetaken request)
    {
        if (string.IsNullOrEmpty(request.ToegewezenAanActor?.Uuid))
        {
            _logger.LogWarning("No toegewezen aan actor found for internetaken {Nummer}", request.Nummer);
            return string.Empty;
        }

        var actor = await _openKlantApiClient.GetActorAsync(request.ToegewezenAanActor.Uuid);

        if (!Actor.IsValid(actor) || actor.Actoridentificator == null)
        {
            _logger.LogWarning("Invalid actor found for actor {Uuid}", request.ToegewezenAanActor.Uuid);
            return string.Empty;
        }
       return Actor.IsActorObject(actor.Actoridentificator)
                ? await _objectApiClient.GetObjectByIdentificatieAsync(actor.Actoridentificator.ObjectId)
                : actor.Actoridentificator.ObjectId; 
    }

   
  
   
   

    private List<Internetaken> FilterNewInternetaken(InternetakenResponse internetaken)
    {
        var thresholdTime = DateTimeOffset.UtcNow.AddHours(_hourThreshold);
        return internetaken.Results
            .Where(item => item.ToegewezenOp > thresholdTime)
            .ToList();
    }
}
