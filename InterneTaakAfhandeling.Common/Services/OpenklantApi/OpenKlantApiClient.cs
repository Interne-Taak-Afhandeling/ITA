
using InterneTaakAfhandeling.Common.Services.OpenklantApi.Models;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using InterneTaakAfhandeling.Web.Server.Services.OpenKlantApi.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Specialized;
using System.Net.Http.Json;
using System.Text.Json;
using System.Web;

namespace InterneTaakAfhandeling.Common.Services.OpenKlantApi;

public interface IOpenKlantApiClient
{
    Task<InternetakenResponse?> GetInternetakenAsync(string path);
    Task<Actor> GetActorAsync(string uuid);
    Task<Klantcontact> GetKlantcontactAsync(string uuid);
    Task<Betrokkene> GetBetrokkeneAsync(string uuid);
    Task<DigitaleAdres> GetDigitaleAdresAsync(string uuid);
    Task<List<Internetaken>> GetOutstandingInternetakenByToegewezenAanActor(string uuid);
    Task<Actor?> QueryActorAsync(ActorQuery query);
    Task<Klantcontact> CreateKlantcontactAsync(KlantcontactRequest request);

    Task<ActorKlantcontact> CreateActorKlantcontactAsync(ActorKlantcontactRequest request);
    Task<Actor?> GetOrCreateActorByEmail(string email, string? naam = null);
    Task<Onderwerpobject> CreateOnderwerpobjectAsync(Onderwerpobject request);
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
            Console.WriteLine($"Error response: {responseContent}");
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


    public async Task<Actor?> GetOrCreateActorByEmail(string email, string? naam = null)
    {
        if (string.IsNullOrEmpty(email))
        {
            throw new ConflictException("Email must not be empty", code: "EMPTY_EMAIL_ADDRESS");
        }

        // Probeer eerst de actor op te halen
        var actor = await GetActorByEmail(email);

        // Als actor niet bestaat, maak een nieuwe aan
        if (actor == null)
        {
            // Maak een nieuwe actor request
            var actorRequest = new ActorRequest
            {
                Naam = naam ?? email, // Als naam niet opgegeven is, gebruik email als naam
                SoortActor = "medewerker",
                IndicatieActief = true,
                Actoridentificator = new ActorIdentificator
                {
                    ObjectId = email,
                    CodeObjecttype = "mdw",
                    CodeRegister = "msei",
                    CodeSoortObjectId = "email"
                },
                ActorIdentificatie = new ActorIdentificatie
                {
                    Emailadres = email
                }
            };

            // Maak de actor aan
            actor = await CreateActorAsync(actorRequest);
        }

        return actor;
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
        _logger.LogInformation("Fetching klantcontact {Uuid}", uuid);

        var response = await _httpClient.GetAsync($"klantcontacten/{uuid}?expand=leiddeTotInterneTaken,gingOverOnderwerpobjecten,hadBetrokkenen,hadBetrokkenen.digitaleAdressen");
        response.EnsureSuccessStatusCode();

        var klantcontact = await response.Content.ReadFromJsonAsync<Klantcontact>();

        if (klantcontact == null)
        {
            _logger.LogInformation("Klantcontact not found {Uuid}", uuid);
            throw new Exception("Klantcontact not found");
        }

        _logger.LogInformation("Successfully retrieved klantcontact {Uuid}", uuid);

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
        // while (!string.IsNullOrEmpty(page))
        // {
        var response = await _httpClient.GetAsync(page);
        response.EnsureSuccessStatusCode();
        var currentContent = await response.Content.ReadFromJsonAsync<InternetakenResponse>();

        await Task.WhenAll(currentContent?.Results?.Select(async x =>
        {


            x.AanleidinggevendKlantcontact = await GetKlantcontactAsync(x.AanleidinggevendKlantcontact?.Uuid ?? string.Empty);
        }) ?? []);
        content.AddRange(currentContent?.Results ?? []);
        //  page = currentContent?.Next?.Replace(_httpClient.BaseAddress.AbsoluteUri, string.Empty);
        //  }

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
