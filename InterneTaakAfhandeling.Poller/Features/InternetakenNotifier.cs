using InterneTaakAfhandeling.Common.Services.Emailservices.Content;
using InterneTaakAfhandeling.Common.Services.Emailservices.SmtpMailService;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
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
    private readonly string _apiBaseUrl;
    private readonly string _itaBaseUrl;
    private readonly IEmailContentService _emailContentService;
    private readonly INotifierStateService _notifierStateService;
    private readonly IInterneTaakEmailInputService _emailInputService;

    public InternetakenNotifier(
        IOpenKlantApiClient openKlantApiClient,
        IConfiguration configuration,
        IEmailService emailService,
        ILogger<InternetakenNotifier> logger,
        IEmailContentService emailContentService,
        INotifierStateService notifierStateService,
        IInterneTaakEmailInputService emailInputService)
    {
        _openKlantApiClient = openKlantApiClient ?? throw new ArgumentNullException(nameof(openKlantApiClient));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _apiBaseUrl = configuration.GetValue<string>("OpenKlantApi:BaseUrl")
            ?? throw new ArgumentException("OpenKlantApi:BaseUrl configuration is missing");
        _itaBaseUrl = configuration.GetValue<string>("Ita:BaseUrl")
            ?? throw new ArgumentException("Ita:BaseUrl configuration is missing");
        _emailContentService = emailContentService ?? throw new ArgumentNullException(nameof(emailContentService));
        _notifierStateService = notifierStateService ?? throw new ArgumentNullException(nameof(notifierStateService));
        _emailInputService = emailInputService;
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

            var actors = await GetActorsAsync(internetaken);
            var actorEmailsResult = await _emailInputService.ResolveActorsEmailAsync(actors);
            var actorEmails = actorEmailsResult.FoundEmails;

            foreach (var error in actorEmailsResult.Errors)
            {
                _logger.LogWarning("Error while resolving actors for interne taak {Number}: {Error}", internetaken.Nummer, error);
            }

            if (actorEmails.Count > 0)
            {
                var emailContent = _emailContentService.BuildInternetakenEmailContent(internetaken, _itaBaseUrl);

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
            _logger.LogError(ex, "Error processing e-mail notifications for new interne taak {Number}", internetaken.Nummer);
            success = false;
            errorMessage = ex.Message;
        }

        return new ProcessingResult(success, Guid.Parse(internetaken.Uuid), internetaken.ToegewezenOp ?? DateTimeOffset.MinValue, errorMessage);
    }

    private async Task<List<Actor>> GetActorsAsync(Internetaak internetaken)
    {
        var actoren = new List<Actor>();

        foreach (var item in internetaken.ToegewezenAanActoren ?? [])
        {
            var actor = await _openKlantApiClient.GetActorAsync(item.Uuid);
            actoren.Add(actor);
        }

        return actoren;
    }


}
