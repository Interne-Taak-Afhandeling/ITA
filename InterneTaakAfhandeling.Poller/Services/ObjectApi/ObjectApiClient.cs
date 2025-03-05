using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using InterneTaakAfhandeling.Poller.Services.ObjectApi.Models; 

namespace InterneTaakAfhandeling.Poller.Services.ObjectApi;

public interface IObjectApiClient
{
    Task<string> GetObjectByIdentificatieAsync(string identificatie);
}

public class ObjectApiClient : IObjectApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ObjectApiClient> _logger;
    private readonly string _baseUrl;
    private readonly string _apiKey;

    public ObjectApiClient(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<ObjectApiClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _baseUrl = _configuration.GetValue<string>("ObjectApi:BaseUrl")
            ?? throw new InvalidOperationException("ObjectApi:BaseUrl configuration is missing");
        
        _apiKey = _configuration.GetValue<string>("ObjectApi:ApiKey")
            ?? throw new InvalidOperationException("ObjectApi:ApiKey configuration is missing");

        _httpClient.BaseAddress = new Uri(_baseUrl);

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", _apiKey);
    }

    public async Task<string> GetObjectByIdentificatieAsync(string identificatie)
    {
        _logger.LogInformation("Fetching object for identificatie {Identificatie}", identificatie);

        var response = await _httpClient.GetAsync($"objects?ordering=record__data__identificatie&data_attr=identificatie__exact__{identificatie}");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ObjectResponse>(json);

        if (result?.Results?.FirstOrDefault()?.Uuid == null)
        {
            throw new Exception("Object not found");
        }

        _logger.LogInformation("Successfully retrieved object {Uuid} for identificatie {Identificatie}", 
            result.Results.First().Uuid, identificatie);

        return result.Results.First().Record.Data.Email;
    }
 
}
