using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using InterneTaakAfhandeling.Poller.Services.Openklant;
using InterneTaakAfhandeling.Poller.Services.Emailservices.SmtpMailService;
using InterneTaakAfhandeling.Poller.Services.Openklant.Models;
using InterneTaakAfhandeling.Poller.Services.ObjectApi;
using InterneTaakAfhandeling.Poller.Services.Contact;
using InterneTaakAfhandeling.Poller.Services.Emailservices.Content;

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
    private readonly IContactService _contactService;

    public InternetakenNotifier(
        IOpenKlantApiClient openKlantApiClient, 
        IConfiguration configuration,
        IEmailService emailService,
        ILogger<InternetakenNotifier> logger,
        IObjectApiClient objectApiClient,
        IEmailContentService emailContentService,
        IContactService contactService)
    {
        _openKlantApiClient = openKlantApiClient ?? throw new ArgumentNullException(nameof(openKlantApiClient));
         _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _objectApiClient = objectApiClient ?? throw new ArgumentNullException(nameof(objectApiClient));
        _hourThreshold = configuration.GetValue<int>("InternetakenNotifier:HourThreshold");
        _apiBaseUrl = configuration.GetValue<string>("OpenKlantApi:BaseUrl") 
            ?? throw new ArgumentException("OpenKlantApi:BaseUrl configuration is missing");
        _emailContentService = emailContentService ?? throw new ArgumentNullException(nameof(emailContentService));
        _contactService = contactService ?? throw new ArgumentNullException(nameof(contactService));
    }

    public async Task NotifyAboutNewInternetakenAsync()
    {
        

        try
        {
            var nextUrl = "internetaken";
          
            while (!string.IsNullOrEmpty(nextUrl))
            {
                var internetaken = await _openKlantApiClient.GetInternetakenAsync(nextUrl);

                if (internetaken?.Results == null || internetaken.Results.Count == 0)
                {
                    _logger.LogInformation("No new internetaken found");
                    break;
                }
                var filteredResults = FilterNewInternetakenAsync(internetaken);

                if (filteredResults == null || filteredResults.Count == 0)
                {
                    _logger.LogInformation("No new internetaken found within the last {HourThreshold} hour(s)", _hourThreshold);
                    break;
                }
                await ProcessInternetakenBatchAsync(filteredResults);
               
                nextUrl = GetNextPageUrl(internetaken.Next ?? string.Empty);
              }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while processing internetaken batch");
            throw;
        }
    }

    private string GetNextPageUrl(string nextUrl)
    {
        if (string.IsNullOrEmpty(nextUrl)) return string.Empty;
        return nextUrl.Replace(_apiBaseUrl, "");
    }

    private async Task ProcessInternetakenBatchAsync(List<Internetaken> internetaken)
    {
        _logger.LogInformation("Starting to process {Count} internetaken", internetaken.Count); 
 
        await Task.WhenAll(
            internetaken.Select(ProcessInternetakenItemAsync)
        );
    }

    private async Task ProcessInternetakenItemAsync(Internetaken request)
    {
        var emailTo = string.Empty;
        try
        {
            _logger.LogInformation("Processing internetaken: {Number}", request.Nummer);

            emailTo = await _contactService.ResolveKlantcontactEmailAsync(request);
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

    private List<Internetaken> FilterNewInternetakenAsync(InternetakenResponse internetaken)
    {
        var thresholdTime = DateTimeOffset.UtcNow.AddHours(_hourThreshold);
        return internetaken.Results
           // .Where(item => item.ToegewezenOp > thresholdTime)
            .ToList();
    }
}
