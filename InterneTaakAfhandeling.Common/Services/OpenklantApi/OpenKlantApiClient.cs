
using System.Net.Http.Json;
using System.Text.Json;
using System.Web;
using InterneTaakAfhandeling.Common.Services.OpenklantApi.Models;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using InterneTaakAfhandeling.Web.Server.Features.Internetaken;
using Microsoft.Extensions.Logging;

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
    Task<Internetaken?> QueryInterneTaakAsync(InterneTaakQuery interneTaakQueryParameters);
}

public class OpenKlantApiClient(
    HttpClient httpClient,
    ILogger<OpenKlantApiClient> logger) : IOpenKlantApiClient
{
    private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    private readonly ILogger<OpenKlantApiClient> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

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
}
