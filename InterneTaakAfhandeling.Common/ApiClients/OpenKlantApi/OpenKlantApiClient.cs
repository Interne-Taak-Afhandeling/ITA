using InterneTaakAfhandeling.Common.Exceptions;
using InterneTaakAfhandeling.Common.ApiClients.OpenKlantApi.Models;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace InterneTaakAfhandeling.Common.ApiClients.OpenKlantApi;

/// Everything else except GetAsync and GetAsyncHttpResponse on this interface is still to be refactored. 
/// Those methods contain business logic (building paths and queries, 404 handling, exception mapping, etc.)
/// That logic belongs in the respective service in Common/Services
/// (ActorService, KlantcontactService, InternetaakService, ...), which should call the
/// generic methods of this client. Behavior must stay exactly the same when moving it.
/// Do not add new domain-specific methods here.
public interface IOpenKlantApiClient
{
    Task<T?> GetAsync<T>(string path) where T : class;
    Task<HttpResponseMessage> GetAsyncHttpResponse(string path);
    
    Task<Actor> CreateActorAsync(ActorRequest request);
    Task<Klantcontact> CreateKlantcontactAsync(KlantcontactRequest request);
    Task<Klantcontact> PutKlantcontactAsync(Guid uuid, KlantcontactRequest request);
    Task DeleteKlantcontactAsync(Guid uuid);
    Task<Internetaak> CreateInterneTaak(InternetaakPostRequest internetaakPostRequest);
    Task<Internetaak> PatchInternetaakStatusAsync(InternetakenPatchStatusRequest internetakenUpdateRequest, string uuid);
    Task<Internetaak> PatchInternetaakActorAsync(InternetakenPatchActorsRequest internetakenUpdateRequest, string uuid);
    Task DeleteInterneTaakAsync(Guid uuid);
    Task<Onderwerpobject> CreateOnderwerpobjectAsync(KlantcontactOnderwerpobjectRequest request);
    Task<Onderwerpobject> UpdateOnderwerpobjectAsync(Guid uuid, KlantcontactOnderwerpobjectRequest request);
    Task<Betrokkene> CreateBetrokkeneAsync(BetrokkeneRequest request);
    Task<ActorKlantcontact> CreateActorKlantcontactAsync(ActorKlantcontactRequest request);
}

public partial class OpenKlantApiClient(
    HttpClient httpClient,
    ILogger<OpenKlantApiClient> logger) : IOpenKlantApiClient
{
    private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    private readonly ILogger<OpenKlantApiClient> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<T?> GetAsync<T>(string path) where T : class
    {
        using var response = await _httpClient.GetAsync(path);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<T>();
    }
    
    public async Task<HttpResponseMessage> GetAsyncHttpResponse(string path)
    {
        return await _httpClient.GetAsync(path);
    }
    
    public async Task<Klantcontact> PutKlantcontactAsync(Guid uuid, KlantcontactRequest request)
    {
        var response = await _httpClient.PutAsJsonAsync($"klantcontacten/{uuid}", request);
        response.EnsureSuccessStatusCode();

        var klantcontact = await response.Content.ReadFromJsonAsync<Klantcontact>();
        if (klantcontact == null)
        {
            throw new ConflictException("Failed to deserialize updated klantcontact response",
                code: "KLANTCONTACT_DESERIALIZATION_FAILED");
        }

        return klantcontact;
    }

    public async Task<Klantcontact> CreateKlantcontactAsync(KlantcontactRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("klantcontacten", request);
            var responseContent = await response.Content.ReadAsStringAsync();
            var klantcontact = await response.Content.ReadFromJsonAsync<Klantcontact>();

            if (klantcontact == null)
            {
                throw new ConflictException("Failed to deserialize created klantcontact response",
                    code: "KLANTCONTACT_DESERIALIZATION_FAILED");
            }

            return klantcontact;
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<ActorKlantcontact> CreateActorKlantcontactAsync(ActorKlantcontactRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("actorklantcontacten", request);
            response.EnsureSuccessStatusCode();

            var actorKlantcontact = await response.Content.ReadFromJsonAsync<ActorKlantcontact>();

            if (actorKlantcontact == null)
            {
                throw new ConflictException("Failed to deserialize created actor-klantcontact response",
                    code: "ACTOR_KLANTCONTACT_DESERIALIZATION_FAILED");
            }

            return actorKlantcontact;
        }
        catch (Exception ex)
        {
            throw new ConflictException($"Error linking actor to klantcontact: {ex.Message}",
                                       code: "ACTOR_KLANTCONTACT_LINKING_ERROR");
        }
    }

    public async Task<Actor> CreateActorAsync(ActorRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("actoren", request);
            response.EnsureSuccessStatusCode();
    
            var actor = await response.Content.ReadFromJsonAsync<Actor>();
            return actor!;
        }
        catch (Exception ex)
        {
            throw new ConflictException($"Error creating actor: {ex.Message}",
                                       code: "ACTOR_CREATION_ERROR");
        }
    }

    public async Task<Onderwerpobject> CreateOnderwerpobjectAsync(KlantcontactOnderwerpobjectRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("onderwerpobjecten", request);
            response.EnsureSuccessStatusCode();

            var onderwerpobject = await response.Content.ReadFromJsonAsync<Onderwerpobject>();

            if (onderwerpobject == null)
            {
                throw new ConflictException("Failed to deserialize created onderwerpobject response",
                    code: "ONDERWERPOBJECT_DESERIALIZATION_FAILED");
            }

            return onderwerpobject;
        }
        catch (Exception ex)
        {
            throw new ConflictException($"Error creating onderwerpobject: {ex.Message}",
                                       code: "ONDERWERPOBJECT_CREATION_ERROR");
        }
    }

    public async Task<Onderwerpobject> UpdateOnderwerpobjectAsync(Guid uuid, KlantcontactOnderwerpobjectRequest request)
    {
        try
        {
            var response = await _httpClient.PatchAsJsonAsync($"onderwerpobjecten/{uuid}", request);
            response.EnsureSuccessStatusCode();

            var onderwerpobject = await response.Content.ReadFromJsonAsync<Onderwerpobject>();

            if (onderwerpobject == null)
            {
                throw new ConflictException("Failed to deserialize updated onderwerpobject response",
                    code: "ONDERWERPOBJECT_DESERIALIZATION_FAILED");
            }

            return onderwerpobject;
        }
        catch (Exception ex)
        {
            throw new ConflictException($"Error updating onderwerpobject: {ex.Message}",
                                       code: "ONDERWERPOBJECT_UPDATE_ERROR");
        }
    }

    public async Task DeleteKlantcontactAsync(Guid uuid)
    {
        await _httpClient.DeleteAsync($"klantcontacten/{uuid}");
    }

    public async Task DeleteInterneTaakAsync(Guid uuid)
    {
        await _httpClient.DeleteAsync($"internetaken/{uuid}");
    }

    public async Task<Betrokkene> CreateBetrokkeneAsync(BetrokkeneRequest request)
    {
        try
        {

            _logger.LogInformation("Creating betrokkene with partij {parsedWasPartijUuid} for klantcontact {parsedHadKlantcontactUuid}",
                        request.WasPartij.Uuid, request.HadKlantcontact.Uuid);

            var response = await _httpClient.PostAsJsonAsync("betrokkenen", request);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("CreateBetrokkeneAsync response: {Response}", responseContent);

            var betrokkene = await response.Content.ReadFromJsonAsync<Betrokkene>();

            if (betrokkene == null)
            {
                throw new ConflictException("Failed to deserialize created betrokkene response",
                    code: "BETROKKENE_DESERIALIZATION_FAILED");
            }

            _logger.LogInformation("Successfully created betrokkene {Uuid}", betrokkene.Uuid);
            return betrokkene;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error in CreateBetrokkeneAsync: {Message}", ex.Message);
            throw new ConflictException($"Error creating betrokkene: {ex.Message}",
                                       code: "BETROKKENE_HTTP_ERROR");
        }
        catch (ConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in CreateBetrokkeneAsync: {Message}", ex.Message);
            throw new ConflictException($"Error creating betrokkene: {ex.Message}",
                                       code: "BETROKKENE_CREATION_ERROR");
        }
    }

    // only used in e2e tests, not used in production code
    public async Task<List<Partij>> GetPartijenByBsnAsync(string bsn)
    {
        try
        {
            _logger.LogInformation("Fetching partijen with partijIdentificator__objectId={ObjectId}", bsn);
            
            var partijResponse = await GetAsync<PartijResponse>($"partijen?partijIdentificator__objectId={bsn}");
            var partijen = partijResponse?.Results ?? [];

            _logger.LogInformation("Found {Count} partijen referring to objectId {ObjectId}",
                partijen.Count, bsn);

            return partijen;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving partijen by BSN: {Message}", ex.Message);
            return [];
        }
    }
    
    public async Task<T> PostAsJsonAsync<T>(string path, T request) where T : class
    {
        using var response = await _httpClient.PostAsJsonAsync(path, request);
        response.EnsureSuccessStatusCode();

        var obj = await response.Content.ReadFromJsonAsync<T>();
        return obj!;
    }

    public async Task<Internetaak> UpdateInternetakenAsync(InternetakenUpdateRequest internetakenUpdateRequest, string uuid)
    {
        try
        {
            var response = await _httpClient.PutAsync($"internetaken/{uuid}", JsonContent.Create(internetakenUpdateRequest));
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadFromJsonAsync<Internetaak>();

            return content ?? throw new InvalidOperationException("Failed to update Internetaken. The response content is null.");
        }
        catch (Exception e)
        {
            throw new InvalidOperationException($"Failed to update Internetaken: {e}");
        }
    }

    public async Task<Internetaak> CreateInterneTaak(InternetaakPostRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsync($"internetaken", JsonContent.Create(request));

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadFromJsonAsync<Internetaak>();

            return content ?? throw new InvalidOperationException("Failed to post Internetaak. The response content is null.");
        }
        catch (Exception e)
        {
            throw new InvalidOperationException($"Failed to post Internetaak {e}");
        }
    }

    public async Task<Internetaak> PatchInternetaakStatusAsync(InternetakenPatchStatusRequest request, string uuid)
    {
        return await PatchInterneTaak(JsonContent.Create(request), uuid);
    }
    public async Task<Internetaak> PatchInternetaakActorAsync(InternetakenPatchActorsRequest request, string uuid)
    {
        return await PatchInterneTaak(JsonContent.Create(request), uuid);
    }
    
    private async Task<Internetaak> PatchInterneTaak(JsonContent request, string uuid)
    {
        try
        {
            var response = await _httpClient.PatchAsync($"internetaken/{uuid}", request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadFromJsonAsync<Internetaak>();

            return content ?? throw new InvalidOperationException("Failed to update Internetaken. The response content is null.");
        }
        catch (Exception e)
        {
            throw new InvalidOperationException($"Failed to update Internetaken: {e}");
        }
    }

    private class PartijResponse
    {
        public int Count { get; set; }
        public string? Next { get; set; }
        public string? Previous { get; set; }
        public List<Partij> Results { get; set; } = [];
    }
}
