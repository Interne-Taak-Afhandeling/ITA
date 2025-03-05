using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using ITA.Poller.Services.Openklant;
using ITA.Poller.Services.Emailservices.SmtpMailService;
using ITA.Poller.Services.Openklant.Models;
using ITA.Poller.Services.ObjectApi;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using ITA.Poller.Services.Contact;
using ITA.Poller.Services.Emailservices.Content;

namespace ITA.Poller.Features;

public interface IInternetakenProcessor
{
    Task ProcessInternetakenAsync();
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

    public async Task ProcessInternetakenAsync()
    {
        

        try
        {
            var nextUrl = "internetaken";
          
            while (!string.IsNullOrEmpty(nextUrl))
            {
                var response = await _openKlantApiClient.GetInternetakenAsync(nextUrl);

                if (response?.Results == null || response.Results.Count == 0)
                {
                    _logger.LogInformation("No new internetaken found");
                    break;
                }

                var thresholdTime = DateTimeOffset.UtcNow.AddHours(_hourThreshold);
                var filteredResults = response.Results
                    .Where(item => item.ToegewezenOp > thresholdTime)
                    .ToList();

                if (filteredResults.Count == 0)
                {
                    _logger.LogInformation("No new internetaken found within the last {HourThreshold} hour(s)", _hourThreshold);
                    break;
                }

                await ProcessInternetakenBatchAsync(filteredResults);
               
                nextUrl = GetNextPageUrl(response.Next);
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
        if (string.IsNullOrEmpty(nextUrl)) return null;
        return nextUrl.Replace(_apiBaseUrl, "");
    }

    private async Task ProcessInternetakenBatchAsync(List<InternetakenItem> requests)
    {
        _logger.LogInformation("Starting to process {Count} internetaken", requests.Count);

        await Task.WhenAll(
            requests.Select(request => ProcessSingleInternetakenAsync(request))
        );
    }

    private async Task ProcessSingleInternetakenAsync(InternetakenItem request)
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
}
