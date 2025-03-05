using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using ITA.Poller.Services.Openklant;
using ITA.Poller.Services.Emailservices.SmtpMailService;
using ITA.Poller.Services.Openklant.Models;
using System;

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
    private readonly IConfiguration _configuration;
  

    public InternetakenNotifier(
        IOpenKlantApiClient openKlantApiClient,        
        IConfiguration configuration,
        IEmailService emailService,
        ILogger<InternetakenNotifier> logger)
    {
         _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
         _openKlantApiClient = openKlantApiClient ?? throw new ArgumentNullException(nameof(openKlantApiClient));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ProcessInternetakenAsync()
    {
        try
        { 
            var hourThreshold = _configuration.GetValue<int>("InternetakenNotifier:HourThreshold");
            var apiBaseUrl = _configuration.GetValue<string>("OpenKlantApi:BaseUrl");
            var nextUrl = "internetaken";

            while (nextUrl != null)
            {
                var response = await _openKlantApiClient.GetInternetakenAsync(nextUrl);

                if (response.Results is not { Count: > 0 })
                {
                    _logger.LogInformation("No new internetaken found");
                    break;
                }

                var thresholdTime = DateTimeOffset.UtcNow.AddHours(-hourThreshold);
                var filteredResults = response.Results
                    .Where(item => item.ToegewezenOp > thresholdTime)
                    .ToList();

                if (filteredResults.Count == 0)
                {
                    _logger.LogInformation("No new internetaken found within the last {HourThreshold} hour(s)", hourThreshold);
                    break;
                }

                await ProcessInternetakenBatchAsync(filteredResults);

                nextUrl = response.Next?.Replace(apiBaseUrl, "");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while processing internetaken batch");
            throw;
        }
    }

    private async Task ProcessInternetakenBatchAsync(IEnumerable<InternetakenItem> requests)
    {
        _logger.LogInformation("Starting to process {Count} internetaken", requests.Count());

        foreach (var request in requests)
        {
            try
            {
                await ProcessSingleInternetakenAsync(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error processing internetaken. Will continue with next request");
                // Continue processing other requests even if one fails
            }
        }
    }

    private async Task ProcessSingleInternetakenAsync(InternetakenItem request)
    {
        _logger.LogInformation("Processing internetaken with number: {Number}", request.Nummer);

        var emailBody = $"Internetaken Number: {request.Nummer}\n\n" +
                       $"Requested Action: {string.Join("\n\n", request.GevraagdeHandeling)}\n\n" +
                       $"Explanation: {request.Toelichting ?? "No explanation provided"}\n\n" +
                       $"Status: {request.Status}";

        await _emailService.SendEmailAsync($"New Internetaken - {request.Nummer}", emailBody);

        _logger.LogInformation("Successfully processed internetaken: {Number}", request.Nummer);
    }
}
