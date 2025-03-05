using InterneTaakAfhandeling.Poller.Services.ObjectApi;
using InterneTaakAfhandeling.Poller.Services.Openklant;
using InterneTaakAfhandeling.Poller.Services.Openklant.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace InterneTaakAfhandeling.Poller.Services.Contact;

public interface IContactService
{
    Task<string> ResolveKlantcontactEmailAsync(InternetakenItem request);
}

public class ContactService : IContactService
{
    private readonly IOpenKlantApiClient _openKlantApiClient;
    private readonly IObjectApiClient _objectApiClient;
    private readonly ILogger<IContactService> _logger;

    public ContactService(
        IOpenKlantApiClient openKlantApiClient,
        IObjectApiClient objectApiClient,
        ILogger<IContactService> logger)
    {
        _openKlantApiClient = openKlantApiClient;
        _objectApiClient = objectApiClient;
        _logger = logger;
    }

    private bool IsActorObject(Actoridentificator actorIdentificator) =>
        actorIdentificator.CodeSoortObjectId == "idf" &&
        actorIdentificator.CodeObjecttype == "mdw" &&
        actorIdentificator.CodeRegister == "obj";

    public async Task<string> ResolveKlantcontactEmailAsync(InternetakenItem request)
    {
        var klantcontact = await _openKlantApiClient
            .GetKlantcontactAsync(request.AanleidinggevendKlantcontact.Uuid);

        var actor = klantcontact?.HadBetrokkenActoren
            .FirstOrDefault()?.Actoridentificator;

        if (actor == null)
        {
            _logger.LogWarning("No valid actor found for klantcontact {Uuid}", 
                request.AanleidinggevendKlantcontact.Uuid);
            return string.Empty;
        }

        try
        {
            return IsActorObject(actor)
                ? await _objectApiClient.GetObjectByIdentificatieAsync(actor.ObjectId) 
                : actor.ObjectId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving object ID for klantcontact {Uuid}", 
                request.AanleidinggevendKlantcontact.Uuid);
            throw;
        }
    }
}
