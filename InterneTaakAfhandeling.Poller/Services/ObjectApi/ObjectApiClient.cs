using System.Net.Http.Headers;
using System.Text.Json;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using InterneTaakAfhandeling.Poller.Services.ObjectApi.Models; 

namespace InterneTaakAfhandeling.Poller.Services.ObjectApi;

public interface IObjectApiClient
{
    Task<List<ObjectRecord>> GetObjectsByIdentificatie(string identificatie);
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

    public async Task<List<ObjectRecord>> GetObjectsByIdentificatie(string identificatie)
    {
        _logger.LogInformation("Fetching objects for identificatie {Identificatie}", identificatie);

        try
        {
            var response = await _httpClient.GetAsync($"objects?ordering=record__data__identificatie&data_attr=identificatie__exact__{identificatie}");
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<ObjectResponse>();
            
            if (result?.Results == null || result.Results.Count == 0)
            {
                _logger.LogWarning("No objects found for identificatie {Identificatie}", identificatie);
                return [];
            }

            
            return result.Results
                .Where(r => r?.Record != null)
                .Select(r => r.Record)
                .ToList();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error fetching objects for identificatie {Identificatie}", identificatie);
            return [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while fetching objects for identificatie {Identificatie}", identificatie);
            return [];
        }
    }
}
