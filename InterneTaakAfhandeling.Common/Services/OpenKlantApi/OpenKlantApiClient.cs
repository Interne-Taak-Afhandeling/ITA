using System.Net.Http.Json;
using System.Text.Json;
using System.Web;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using InterneTaakAfhandeling.Web.Server.Features.Internetaken;
using Microsoft.Extensions.Logging;

namespace InterneTaakAfhandeling.Common.Services.OpenKlantApi;

public interface IOpenKlantApiClient
{
    Task<InternetakenResponse?> GetInternetakenAsync(string path);
    Task<Actor> GetActorAsync(string uuid);
    Task<Actor?> GetActorByEmail(string userEmail);
    Task<Actor?> CreateActorAsync(ActorRequest request);
    Task<Klantcontact> GetKlantcontactAsync(string uuid);
    Task<Betrokkene> GetBetrokkeneAsync(string uuid);
    Task<Betrokkene> CreateBetrokkeneAsync(BetrokkeneRequest request);
    Task<DigitaleAdres> GetDigitaleAdresAsync(string uuid);
    Task<List<Internetaken>> GetOutstandingInternetakenByToegewezenAanActor(string uuid);
    Task<Actor?> QueryActorAsync(ActorQuery query);
    Task<Klantcontact> CreateKlantcontactAsync(KlantcontactRequest request);
    Task<ActorKlantcontact> CreateActorKlantcontactAsync(ActorKlantcontactRequest request);
    Task<Onderwerpobject> CreateOnderwerpobjectAsync(Onderwerpobject request);
    Task<Internetaken?> GetInternetaak(string uuid);
    Task<List<Klantcontact>> GetKlantcontactenByOnderwerpobjectIdentificatorObjectIdAsync(string objectId);
    Task<Internetaken?> QueryInterneTaakAsync(InterneTaakQuery interneTaakQueryParameters);
    Task<Internetaken> UpdateInternetakenAsync(InternetakenUpdateRequest internetakenUpdateRequest, string uuid);
    Task<Internetaken?> GetInternetakenByIdAsync(string uuid);
}

public class OpenKlantApiClient(
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

    public async Task<Actor?> GetActorByEmail(string userEmail)
    {
        if (string.IsNullOrEmpty(userEmail))
        {
            throw new ConflictException("Email must not be empty",
                                        code: "EMPTY_EMAIL_ADDRESS");
        }

        try
        {
            var response = await _httpClient.GetAsync($"actoren?actoridentificatorObjectId={userEmail}");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadFromJsonAsync<ActorResponse>();
            return content?.Results?.FirstOrDefault();
        }
        catch (Exception ex)
        {
            // Specifieke exception handling en logging kan hier worden toegevoegd
            throw new ConflictException($"Error retrieving actor by email: {ex.Message}",
                                        code: "ACTOR_RETRIEVAL_ERROR");
        }
    }

    public async Task<Onderwerpobject> CreateOnderwerpobjectAsync(Onderwerpobject request)
    {
        try
        {
            var minimalRequest = new
            {
                klantcontact = request.Klantcontact != null ? new { uuid = request.Klantcontact.Uuid } : null,
                wasKlantcontact = request.WasKlantcontact != null ? new { uuid = request.WasKlantcontact.Uuid } : null,
                onderwerpobjectidentificator = request.Onderwerpobjectidentificator
            };

            var response = await _httpClient.PostAsJsonAsync("onderwerpobjecten", minimalRequest);
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

        // Log specifiek de gingOverOnderwerpobjecten
        _logger.LogInformation("Onderwerpobjecten count: {Count}", klantcontact?.GingOverOnderwerpobjecten?.Count ?? 0);
        foreach (var obj in klantcontact?.GingOverOnderwerpobjecten ?? [])
        {
            _logger.LogInformation("Onderwerpobject: {Uuid}, Identificator: {Id}",
                obj.Uuid,
                obj.Onderwerpobjectidentificator?.ObjectId);
        }

        return klantcontact;
    }

    public async Task<Betrokkene> GetBetrokkeneAsync(string uuid)
    {
        _logger.LogInformation("Fetching betrokkene {Uuid}", uuid);

        var response = await _httpClient.GetAsync($"betrokkenen/{uuid}");
        response.EnsureSuccessStatusCode();

        var betrokkene = await response.Content.ReadFromJsonAsync<Betrokkene>();

        if (betrokkene == null)
        {
            _logger.LogInformation("Betrokkene not found {Uuid}", uuid);
            throw new Exception("Betrokkene not found");
        }

        _logger.LogInformation("Successfully retrieved betrokkene {Uuid}", uuid);

        return betrokkene;
    }

    public async Task<DigitaleAdres> GetDigitaleAdresAsync(string uuid)
    {
        _logger.LogInformation("Fetching digitale adres {Uuid}", uuid);

        var response = await _httpClient.GetAsync($"digitaleadressen/{uuid}");
        response.EnsureSuccessStatusCode();

        var digitaleAdres = await response.Content.ReadFromJsonAsync<DigitaleAdres>();

        if (digitaleAdres == null)
        {
            _logger.LogInformation("Digitale adres not found {Uuid}", uuid);
            throw new Exception("Digitale adres not found");
        }

        _logger.LogInformation("Successfully retrieved digitale adres {Uuid}", uuid);

        return digitaleAdres;
    }

    public async Task<List<Internetaken>> GetOutstandingInternetakenByToegewezenAanActor(string uuid)
    {
        List<Internetaken> content = [];
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

    public async Task<Internetaken?> QueryInterneTaakAsync(InterneTaakQuery interneTaakQueryParameters)
    {
        var queryString = string.Join("&",
            interneTaakQueryParameters.GetType().GetProperties()
                .Where(prop => prop.GetValue(interneTaakQueryParameters) != null)
                .Select(prop => $"{HttpUtility.UrlEncode(prop.Name.ToLower())}={HttpUtility.UrlEncode(prop.GetValue(interneTaakQueryParameters)?.ToString())}"));

        var path = $"internetaken?{queryString}";
        var response = await GetInternetakenAsync(path);
        if (response?.Results?.Count > 0)
        {
            var internetaken = response.Results.FirstOrDefault();
            if (internetaken?.ToegewezenAanActoren != null)
            {

                internetaken.ToegewezenAanActoren = [.. (await Task.WhenAll(
                        internetaken.ToegewezenAanActoren
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

            if (internetaken != null)
            {
                internetaken.AanleidinggevendKlantcontact = await GetKlantcontactAsync(internetaken.AanleidinggevendKlantcontact?.Uuid ?? string.Empty);
                return internetaken;
            }
            return null;
        }
        else
        {
            throw new InvalidOperationException($"No internetaken found with the provided query parameters.");
        }
    }



    public async Task<Internetaken?> GetInternetaak(string uuid)
    {
        var response = await _httpClient.GetAsync($"internetaken/{uuid}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Internetaken>();
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
    public async Task<Internetaken?> GetInternetakenByIdAsync(string uuid)
    {
        var response = await _httpClient.GetAsync($"internetaken/{uuid}");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<Internetaken>();
        
    }

    public async Task<Internetaken> UpdateInternetakenAsync(InternetakenUpdateRequest internetakenUpdateRequest, string uuid)
    {
        try
        {
            var response = await _httpClient.PutAsync($"internetaken/{uuid}", JsonContent.Create(internetakenUpdateRequest));
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadFromJsonAsync<Internetaken>();

            return content ?? throw new InvalidOperationException("Failed to update Internetaken. The response content is null.");
        }
        catch (Exception e)
        {
            throw new InvalidOperationException($"Failed to update Internetaken: {e}");
        }
    }

    private class KlantcontactResponse
    {
        public int Count { get; set; }
        public string? Next { get; set; }
        public string? Previous { get; set; }
        public List<Klantcontact> Results { get; set; } = [];
    }


    public class ConflictException : Exception
    {
        public string? Code { get; set; }

        public ConflictException(string message) : base(message) { }

        public ConflictException(string message, string code) : base(message)
        {
            Code = code;
        }
    }

    public class ITAException
    {
        public string? Code { get; set; }

        public string? Message { get; set; }
    }
}