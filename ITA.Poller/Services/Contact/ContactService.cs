using ITA.Poller.Services.ObjecttypenApi;
using ITA.Poller.Services.Openklant;
using ITA.Poller.Services.Openklant.Models;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace ITA.Poller.Services.Contact;

public interface IContactService
{
    Task<string> ResolveKlantcontactEmailAsync(InternetakenItem request);
}

public class ContactService : IContactService
{
    private readonly IOpenKlantApiClient _openKlantApiClient;
    private readonly IObjecttypenApiClient _objecttypenApiClient;
    private readonly ILogger<IContactService> _logger;

    public ContactService(
        IOpenKlantApiClient openKlantApiClient,
        IObjecttypenApiClient objecttypenApiClient,
        ILogger<IContactService> logger)
    {
        _openKlantApiClient = openKlantApiClient;
        _objecttypenApiClient = objecttypenApiClient;
        _logger = logger;
    }

    public async Task<string> ResolveKlantcontactEmailAsync(InternetakenItem request)
    {
        var klantcontact = await _openKlantApiClient.GetKlantcontactAsync(request.AanleidinggevendKlantcontact.Uuid);
        if (klantcontact?.HadBetrokkenActoren == null)
        {
            _logger.LogWarning("No betrokken actoren found for klantcontact {Uuid}", 
                request.AanleidinggevendKlantcontact.Uuid);
            return string.Empty;
        }

        var emailActor = klantcontact.HadBetrokkenActoren
            .FirstOrDefault(a => a.Actoridentificator?.CodeSoortObjectId == "email") 
            ?? klantcontact.HadBetrokkenActoren.FirstOrDefault();

        if (emailActor?.Actoridentificator?.ObjectId == null)
        {
            _logger.LogWarning("No actor identifier found for klantcontact {Uuid}", 
                request.AanleidinggevendKlantcontact.Uuid);
            return string.Empty;
        }
        // object type client is not fully tested since all the scenarios emailActor.Actoridentificator.CodeSoortObjectId is email
       return emailActor.Actoridentificator.CodeSoortObjectId == "email"
            ? emailActor.Actoridentificator.ObjectId
            : await _objecttypenApiClient.GetObjectIdAsync(emailActor.Actoridentificator.ObjectId);
    }
}
