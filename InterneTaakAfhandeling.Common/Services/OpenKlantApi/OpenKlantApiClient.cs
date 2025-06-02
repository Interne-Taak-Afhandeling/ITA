using System;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using InterneTaakAfhandeling.Common.Exceptions;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using InterneTaakAfhandeling.Web.Server.Features.Internetaken;
using Microsoft.Extensions.Logging;

namespace InterneTaakAfhandeling.Common.Services.OpenKlantApi;

public interface IOpenKlantApiClient
{
    Task<InternetakenResponse?> GetInternetakenAsync(string path);
    Task<Actor> GetActorAsync(string uuid);
    Task<Actor?> CreateActorAsync(ActorRequest request);
    Task<Klantcontact> GetKlantcontactAsync(string uuid);
    Task<Betrokkene> CreateBetrokkeneAsync(BetrokkeneRequest request);
    Task<List<Internetaak>> GetOutstandingInternetakenByToegewezenAanActor(string uuid);
    Task<Actor?> QueryActorAsync(ActorQuery query);
    Task<Klantcontact> CreateKlantcontactAsync(KlantcontactRequest request);
    Task<ActorKlantcontact> CreateActorKlantcontactAsync(ActorKlantcontactRequest request);
    Task<List<Klantcontact>> GetKlantcontactenByOnderwerpobjectIdentificatorObjectIdAsync(string objectId);
    Task<List<Internetaak>> QueryInterneTakenAsync(InterneTaakQuery interneTaakQueryParameters);

    Task<Onderwerpobject> CreateOnderwerpobjectAsync(KlantcontactOnderwerpobjectRequest request);
    Task<Onderwerpobject> UpdateOnderwerpobjectAsync(string uuid, KlantcontactOnderwerpobjectRequest request);
    Task<Onderwerpobject?> GetOnderwerpobjectAsync(string uuid);
    Task<Internetaak> PutInternetaakAsync(InternetakenUpdateRequest internetakenUpdateRequest, string uuid);
    Task<Internetaak> PatchInternetaakAsync(InternetakenPatchRequest internetakenUpdateRequest, string uuid);

    Task<Internetaak?> GetInternetaakByIdAsync(string uuid);
    Task<InternetakenResponse> GetAllInternetakenAsync(InterneTaakQuery query);

}

public partial class OpenKlantApiClient(
    HttpClient httpClient,
    ILogger<OpenKlantApiClient> logger) : IOpenKlantApiClient
{
    private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    private readonly ILogger<OpenKlantApiClient> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

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

    public async Task<Actor?> CreateActorAsync(ActorRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("actoren", request);
            response.EnsureSuccessStatusCode();

            var actor = await response.Content.ReadFromJsonAsync<Actor>();
            return actor;
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

    public async Task<Onderwerpobject> UpdateOnderwerpobjectAsync(string uuid, KlantcontactOnderwerpobjectRequest request)
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

    public async Task<Onderwerpobject?> GetOnderwerpobjectAsync(string uuid)
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

    public async Task<Actor> GetActorAsync(string uuid)
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

    public async Task<Klantcontact> GetKlantcontactAsync(string uuid)
    {
        if (Guid.TryParse(uuid, out Guid parsedUuid))
        {
            _logger.LogInformation("Fetching klantcontact {parsedUuid}", parsedUuid);
        }

        var response = await _httpClient.GetAsync($"klantcontacten/{uuid}?expand=leiddeTotInterneTaken,gingOverOnderwerpobjecten,hadBetrokkenen,hadBetrokkenen.digitaleAdressen");
        response.EnsureSuccessStatusCode();

        var jsonString = await response.Content.ReadAsStringAsync();
        _logger.LogInformation("API Response: {JsonString}", jsonString);


       
        var klantcontact = await response.Content.ReadFromJsonAsync<Klantcontact>();

        _logger.LogInformation("Onderwerpobjecten count: {Count}", klantcontact?.GingOverOnderwerpobjecten?.Count ?? 0);
        foreach (var obj in klantcontact?.GingOverOnderwerpobjecten ?? [])
        {
            _logger.LogInformation("Onderwerpobject: {Uuid}, Identificator: {Id}",
                obj.Uuid,
                obj.Onderwerpobjectidentificator?.ObjectId);
        }

        return klantcontact;
    }

    public async Task<List<Internetaak>> GetOutstandingInternetakenByToegewezenAanActor(string uuid)
    {
        List<Internetaak> content = [];
        var page = $"internetaken?toegewezenAanActor__uuid={uuid}&status=te_verwerken";
        while (!string.IsNullOrEmpty(page))
        {
            var response = await _httpClient.GetAsync(page);
            response.EnsureSuccessStatusCode();
            var currentContent = await response.Content.ReadFromJsonAsync<InternetakenResponse>();

            await Task.WhenAll(currentContent?.Results?.Select(async x =>
            {
                x.AanleidinggevendKlantcontact = await GetKlantcontactAsync(x.AanleidinggevendKlantcontact?.Uuid ?? string.Empty);
            }) ?? []);
            content.AddRange(currentContent?.Results ?? []);
            page = currentContent?.Next?.Replace(_httpClient.BaseAddress?.AbsoluteUri ?? string.Empty, string.Empty);
        }

        return content?.OrderBy(x => x.ToegewezenOp).ToList() ?? [];
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
        var queryString = string.Join("&",
            interneTaakQueryParameters.GetType().GetProperties()
                .Where(prop => prop.GetValue(interneTaakQueryParameters) != null)
                .Select(prop => $"{HttpUtility.UrlEncode(prop.Name.ToLower())}={HttpUtility.UrlEncode(prop.GetValue(interneTaakQueryParameters)?.ToString())}"));

        var path = $"internetaken?{queryString}";
        var response = await GetInternetakenAsync(path);

        if (response?.Results == null || response.Results.Count == 0)
        {
            throw new InvalidOperationException($"No internetaken found with the provided query parameters.");
        }

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

            item.AanleidinggevendKlantcontact = await GetKlantcontactAsync(item.AanleidinggevendKlantcontact?.Uuid ?? string.Empty);
        }

        return response.Results;

    }

    public async Task<Betrokkene> CreateBetrokkeneAsync(BetrokkeneRequest request)
    {
        try
        {

            if (Guid.TryParse(request.WasPartij.Uuid, out Guid parsedWasPartijUuid))
            {
                if (Guid.TryParse(request.HadKlantcontact.Uuid, out Guid parsedHadKlantcontactUuid))
                {
                    _logger.LogInformation("Creating betrokkene with partij {parsedWasPartijUuid} for klantcontact {parsedHadKlantcontactUuid}",
                        parsedWasPartijUuid, parsedHadKlantcontactUuid);
                }
            }

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
    public async Task<Internetaak?> GetInternetaakByIdAsync(string uuid)
    {
        var response = await _httpClient.GetAsync($"internetaken/{uuid}");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<Internetaak>();

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

    public async Task<InternetakenResponse> GetAllInternetakenAsync(InterneTaakQuery query)
    {
        var queryString = BuildQueryString(query);
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

        throw new HttpRequestException($"Failed to fetch internetaken: {response.StatusCode} - {response.ReasonPhrase}");
    }





    public async Task<Internetaak> PutInternetaakAsync(InternetakenUpdateRequest internetakenUpdateRequest, string uuid)
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


    public async Task<Internetaak> PatchInternetaakAsync(InternetakenPatchRequest request, string uuid)
    {
        try
        {
            var response = await _httpClient.PatchAsync($"internetaken/{uuid}", JsonContent.Create(request));
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadFromJsonAsync<Internetaak>();

            return content ?? throw new InvalidOperationException("Failed to update Internetaken. The response content is null.");
        }
        catch (Exception e)
        {
            throw new InvalidOperationException($"Failed to update Internetaken: {e}");
        }
    }




    private string BuildQueryString(InterneTaakQuery query)
    {
        var parameters = new List<string>();

        if (!string.IsNullOrEmpty(query.Status))
            parameters.Add($"status={Uri.EscapeDataString(query.Status)}");

        if (query.Page.HasValue)
            parameters.Add($"page={query.Page.Value}");

        if (query.PageSize.HasValue)
            parameters.Add($"pageSize={query.PageSize.Value}");

        if (!string.IsNullOrEmpty(query.Nummer))
            parameters.Add($"nummer={Uri.EscapeDataString(query.Nummer)}");

        if (!string.IsNullOrEmpty(query.AanleidinggevendKlantcontact_Url))
            parameters.Add($"aanleidinggevendKlantcontact__url={Uri.EscapeDataString(query.AanleidinggevendKlantcontact_Url)}");

        if (query.AanleidinggevendKlantcontact_Uuid.HasValue)
            parameters.Add($"aanleidinggevendKlantcontact__uuid={query.AanleidinggevendKlantcontact_Uuid.Value}");

        if (!string.IsNullOrEmpty(query.Actoren__Naam))
            parameters.Add($"actoren__naam={Uri.EscapeDataString(query.Actoren__Naam)}");

        if (!string.IsNullOrEmpty(query.Klantcontact__Nummer))
            parameters.Add($"klantcontact__nummer={Uri.EscapeDataString(query.Klantcontact__Nummer)}");

        if (query.Klantcontact__Uuid.HasValue)
            parameters.Add($"klantcontact__uuid={query.Klantcontact__Uuid.Value}");

        if (!string.IsNullOrEmpty(query.ToegewezenAanActor__Url))
            parameters.Add($"toegewezenAanActor__url={Uri.EscapeDataString(query.ToegewezenAanActor__Url)}");

        if (query.ToegewezenAanActor__Uuid.HasValue)
            parameters.Add($"toegewezenAanActor__uuid={query.ToegewezenAanActor__Uuid.Value}");

        if (query.ToegewezenOp.HasValue)
            parameters.Add($"toegewezenOp={query.ToegewezenOp.Value:yyyy-MM-ddTHH:mm:ss.fffZ}");

        return string.Join("&", parameters);
    }

    private class KlantcontactResponse
    {
        public int Count { get; set; }
        public string? Next { get; set; }
        public string? Previous { get; set; }
        public List<Klantcontact> Results { get; set; } = [];
    }


}