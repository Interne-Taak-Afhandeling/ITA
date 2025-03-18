using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using InterneTaakAfhandeling.Poller.Services.Openklant.Models; 

namespace InterneTaakAfhandeling.Poller.Services.Openklant;

public interface IOpenKlantApiClient
{
    Task<InternetakenResponse?> GetInternetakenAsync(string path);
    Task<Actor> GetActorAsync(string uuid);
    Task<Klantcontact> GetKlantcontactAsync(string uuid);
    Task<Betrokkene> GetBetrokkeneAsync(string uuid);
    Task<DigitaleAdres> GetDigitaleAdresAsync(string uuid); // Added method signature
}

public class OpenKlantApiClient : IOpenKlantApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OpenKlantApiClient> _logger;
    private readonly string _baseUrl;

    public OpenKlantApiClient(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<OpenKlantApiClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _baseUrl = _configuration.GetValue<string>("OpenKlantApi:BaseUrl")
            ?? throw new InvalidOperationException("OpenKlantApi:BaseUrl configuration is missing");
        _httpClient.BaseAddress = new Uri(_baseUrl);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token",
            _configuration.GetValue<string>("OpenKlantApi:ApiKey") ?? throw new InvalidOperationException("OpenKlantApi:ApiKey configuration is missing"));
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

        var response = await _httpClient.GetAsync($"klantcontacten/{uuid}?expand=leiddeTotInterneTaken,gingOverOnderwerpobjecten");
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
}
