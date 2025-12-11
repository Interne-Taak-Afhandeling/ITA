using InterneTaakAfhandeling.Common.Exceptions;
using InterneTaakAfhandeling.Common.Helpers;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http.Headers;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http.Json;
using System.Text.Json;
using System.Web;

namespace InterneTaakAfhandeling.Common.Services.OpenKlantApi;

public interface IOpenKlantApiClient
{
    Task<Actor> GetActorAsync(string? uuid);
    Task<Actor?> QueryActorAsync(ActorQuery query);
    Task<Actor> CreateActorAsync(ActorRequest request);

    Task<Klantcontact> GetKlantcontactAsync(Guid uuid);
    Task<Klantcontact> CreateKlantcontactAsync(KlantcontactRequest request);
    Task<Klantcontact> PutKlantcontactAsync(Guid uuid, KlantcontactRequest request);
    Task<List<Klantcontact>> GetKlantcontactenByOnderwerpobjectIdentificatorObjectIdAsync(string objectId);
    Task<List<Klantcontact>> QueryKlantcontactAsync(KlantcontactQuery query);
    Task DeleteKlantcontactAsync(Guid uuid);

    Task<InternetakenResponse?> GetInternetakenAsync(string path);
    Task<Internetaak> GetInternetaakByIdAsync(Guid uuid);
    Task<InternetakenResponse> GetAllInternetakenAsync(InterneTaakQuery query);
    Task<Internetaak> CreateInterneTaak(InternetaakPostRequest internetaakPostRequest);
    Task<List<Internetaak>> QueryInterneTakenAsync(InterneTaakQuery interneTaakQueryParameters);
    Task<Internetaak> PatchInternetaakStatusAsync(InternetakenPatchStatusRequest internetakenUpdateRequest, string uuid);
    Task<Internetaak> PatchInternetaakActorAsync(InternetakenPatchActorsRequest internetakenUpdateRequest, string uuid);

    Task<Onderwerpobject> CreateOnderwerpobjectAsync(KlantcontactOnderwerpobjectRequest request);
    Task<Onderwerpobject> UpdateOnderwerpobjectAsync(Guid uuid, KlantcontactOnderwerpobjectRequest request);
    Task<Onderwerpobject?> GetOnderwerpobjectAsync(Guid uuid);

    Task<Betrokkene> CreateBetrokkeneAsync(BetrokkeneRequest request);

    Task<ActorKlantcontact> CreateActorKlantcontactAsync(ActorKlantcontactRequest request);


}

public partial class OpenKlantApiClient(
    HttpClient httpClient,
    ILogger<OpenKlantApiClient> logger) : IOpenKlantApiClient
{
    private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    private readonly ILogger<OpenKlantApiClient> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

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

    public async Task<Onderwerpobject?> GetOnderwerpobjectAsync(Guid uuid)
    {
        var response = await _httpClient.GetAsync($"onderwerpobjecten/{uuid}");

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Onderwerpobject>();
    }


    public async Task<InternetakenResponse?> GetInternetakenAsync(string path)
    {
        try
        {
            _logger.LogInformation("Fetching internetaken from OpenKlant API");

            var response = await _httpClient.GetAsync(path);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadFromJsonAsync<InternetakenResponse>();

            _logger.LogInformation(
                "Successfully retrieved {Count} internetaken",
                content?.Results?.Count ?? 0);

            return content;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(
                ex,
                "HTTP error occurred while fetching internetaken: {Message}",
                ex.Message);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(
                ex,
                "JSON deserialization error occurred while fetching internetaken: {Message}",
                ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error occurred while fetching internetaken: {Message}",
                ex.Message);
            throw;
        }
    }

    public async Task<Actor> GetActorAsync(string? uuid)
    {
        _logger.LogInformation("Fetching actor {Uuid}", uuid);

        var response = await _httpClient.GetAsync($"actoren/{uuid}");
        response.EnsureSuccessStatusCode();

        var actor = await response.Content.ReadFromJsonAsync<Actor>();

        if (actor == null)
        {
            _logger.LogInformation("Actor not found {Uuid}", uuid);
            throw new Exception("Actor not found");
        }

        _logger.LogInformation("Successfully retrieved actor {Uuid}", uuid);

        return actor;
    }

    // public async Task<Klantcontact> GetKlantcontactAsync(Guid uuid)
    // {
    //     _logger.LogInformation("Fetching klantcontact {Uuid}", uuid);

    //     var response = await _httpClient.GetAsync($"klantcontacten/{uuid}?expand=leiddeTotInterneTaken,gingOverOnderwerpobjecten,hadBetrokkenen,hadBetrokkenen.digitaleAdressen");
    //     response.EnsureSuccessStatusCode();

    //     var jsonString = await response.Content.ReadAsStringAsync();
    //     _logger.LogInformation("API Response: {JsonString}", jsonString);



    //     var klantcontact = await response.Content.ReadFromJsonAsync<Klantcontact>();

    //     _logger.LogInformation("Onderwerpobjecten count: {Count}", klantcontact?.GingOverOnderwerpobjecten?.Count ?? 0);
    //     foreach (var obj in klantcontact?.GingOverOnderwerpobjecten ?? [])
    //     {
    //         _logger.LogInformation("Onderwerpobject: {Uuid}, Identificator: {Id}",
    //             obj.Uuid,
    //             SecureLogging.SanitizeAndTruncate(obj.Onderwerpobjectidentificator?.ObjectId));
    //     }

    //     return klantcontact;
    // }

    public async Task<Klantcontact> GetKlantcontactAsync(Guid uuid)
    {
        _logger.LogInformation("Fetching klantcontact {Uuid}", uuid);

        var response = await _httpClient.GetAsync($"klantcontacten/{uuid}?expand=leiddeTotInterneTaken,gingOverOnderwerpobjecten,hadBetrokkenen,hadBetrokkenen.digitaleAdressen,hadBetrokkenen.wasPartij");
        response.EnsureSuccessStatusCode();

        var jsonString = await response.Content.ReadAsStringAsync();
        _logger.LogInformation("API Response: {JsonString}", jsonString);

        var klantcontact = await response.Content.ReadFromJsonAsync<Klantcontact>();

        _logger.LogInformation("Onderwerpobjecten count: {Count}", klantcontact?.GingOverOnderwerpobjecten?.Count ?? 0);
        foreach (var obj in klantcontact?.GingOverOnderwerpobjecten ?? [])
        {
            _logger.LogInformation("Onderwerpobject: {Uuid}, Identificator: {Id}",
                obj.Uuid,
                SecureLogging.SanitizeAndTruncate(obj.Onderwerpobjectidentificator?.ObjectId));
        }

        return klantcontact;
    }


    public async Task<List<Klantcontact>> QueryKlantcontactAsync(KlantcontactQuery query)
    {
        var queryString = query.BuildQueryString();
        var path = $"klantcontacten?{queryString}";
        var response = await _httpClient.GetAsync(path);
        response.EnsureSuccessStatusCode();
        var klantcontacten = await response.Content.ReadFromJsonAsync<KlantcontactResponse>();
        return klantcontacten?.Results ?? [];
    }


    public async Task DeleteKlantcontactAsync(Guid uuid)
    {
        await _httpClient.DeleteAsync($"klantcontacten/{uuid}");
    }

    public async Task<Actor?> QueryActorAsync(ActorQuery query)
    {
        var queryDictionary = HttpUtility.ParseQueryString(string.Empty);
        queryDictionary["actoridentificatorCodeObjecttype"] = query.ActoridentificatorCodeObjecttype;
        queryDictionary["actoridentificatorCodeRegister"] = query.ActoridentificatorCodeRegister;
        queryDictionary["actoridentificatorCodeSoortObjectId"] = query.ActoridentificatorCodeSoortObjectId;
        queryDictionary["actoridentificatorObjectId"] = query.ActoridentificatorObjectId;
        queryDictionary["soortActor"] = query.SoortActor.ToString();

        if (query.IndicatieActief.HasValue)
        {
            queryDictionary["indicatieActief"] = query.IndicatieActief.Value ? "true" : "false";
        }

        var response = await _httpClient.GetAsync($"actoren?{queryDictionary}");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadFromJsonAsync<ActorResponse>();

        return content?.Results?.FirstOrDefault();
    }

    public async Task<List<Internetaak>> QueryInterneTakenAsync(InterneTaakQuery interneTaakQueryParameters)
    {
        var queryString = interneTaakQueryParameters.BuildQueryString();
        var path = $"internetaken?{queryString}";
        var response = await GetInternetakenAsync(path);

        foreach (var item in response.Results)
        {
            if (item.ToegewezenAanActoren != null)
            {

                item.ToegewezenAanActoren = [.. (await Task.WhenAll(
                        item.ToegewezenAanActoren
                                .Select(async a =>
                                {
                                   if (!string.IsNullOrEmpty(a.Uuid))
                                   {
                                       return (await GetActorAsync(a.Uuid));
                                   }
                                   return null;
                                }).Where(x=> x != null)
                    ))];
            }

            if (item.AanleidinggevendKlantcontact?.Uuid != null)
            {
                item.AanleidinggevendKlantcontact = await GetKlantcontactAsync(item.AanleidinggevendKlantcontact.Uuid);
            }

        }

        return response.Results;

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

    public async Task<List<Klantcontact>> GetKlantcontactenByOnderwerpobjectIdentificatorObjectIdAsync(string objectId)
    {
        try
        {
            _logger.LogInformation("Fetching klantcontacten with onderwerpobject__onderwerpobjectidentificatorObjectId={ObjectId}", objectId);

            // Query om klantcontacten te vinden die verwijzen naar dit objectId via onderwerpobjectidentificator
            var response = await _httpClient.GetAsync($"klantcontacten?onderwerpobject__onderwerpobjectidentificatorObjectId={objectId}");
            response.EnsureSuccessStatusCode();

            //  var response = await _httpClient.GetAsync($"partijen?partijIdentificator__objectId={bsn}");
            // response.EnsureSuccessStatusCode();


            var klantcontactResponse = await response.Content.ReadFromJsonAsync<KlantcontactResponse>();
            var klantcontacten = klantcontactResponse?.Results ?? [];

            _logger.LogInformation("Found {Count} klantcontacten referring to objectId {ObjectId}",
                klantcontacten.Count, objectId);

            return klantcontacten;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving klantcontacten by onderwerpobject ObjectId: {Message}", ex.Message);
            return [];
        }
    }

    public async Task<List<Partij>> GetPartijenByBsnAsync(string bsn)
    {
        try
        {
            _logger.LogInformation("Fetching partijen with partijIdentificator__objectId={ObjectId}", bsn);

            var response = await _httpClient.GetAsync($"partijen?partijIdentificator__objectId={bsn}");
            response.EnsureSuccessStatusCode();

            var partijResponse = await response.Content.ReadFromJsonAsync<PartijResponse>();
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



    public async Task<Internetaak> GetInternetaakByIdAsync(Guid uuid)
    {
        var response = await _httpClient.GetAsync($"internetaken/{uuid}");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<Internetaak>();
        return result!;

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

    public async Task<InternetakenResponse> GetAllInternetakenAsync(InterneTaakQuery query)
    {
        var queryString = query.BuildQueryString();
        var response = await _httpClient.GetAsync($"/klantinteracties/api/v1/internetaken?{queryString}");

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            return JsonSerializer.Deserialize<InternetakenResponse>(content, options)
                ?? new InternetakenResponse { Results = new List<Internetaak>(), Count = 0 };
        }

        throw new HttpRequestException($"Failed to fetch internetaken: {response.ReasonPhrase}", null, response.StatusCode);
    }

    public async Task<Internetaak> PatchInterneTaak(JsonContent request, string uuid)
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

    public async Task<Internetaak> PatchInternetaakStatusAsync(InternetakenPatchStatusRequest request, string uuid)
    {
        return await PatchInterneTaak(JsonContent.Create(request), uuid);
    }
    public async Task<Internetaak> PatchInternetaakActorAsync(InternetakenPatchActorsRequest request, string uuid)
    {
        return await PatchInterneTaak(JsonContent.Create(request), uuid);
    }

    private class KlantcontactResponse
    {
        public int Count { get; set; }
        public string? Next { get; set; }
        public string? Previous { get; set; }
        public List<Klantcontact> Results { get; set; } = [];
    }

    private class PartijResponse
    {
        public int Count { get; set; }
        public string? Next { get; set; }
        public string? Previous { get; set; }
        public List<Partij> Results { get; set; } = [];
    }

    private class ActorResponse
    {
        public int Count { get; set; }
        public string? Next { get; set; }
        public string? Previous { get; set; }
        public List<Actor> Results { get; set; } = [];
    }

}
