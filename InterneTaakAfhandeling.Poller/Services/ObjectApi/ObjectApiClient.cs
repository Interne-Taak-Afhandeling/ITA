using System.Net.Http.Headers;
using System.Text.Json;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using InterneTaakAfhandeling.Poller.Services.ObjectApi.Models; 

namespace InterneTaakAfhandeling.Poller.Services.ObjectApi;

public interface IObjectApiClient
{
    Task<ObjectResponse> GetObjectByIdentificatie(string identificatie);
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

    public async Task<ObjectResponse> GetObjectByIdentificatie(string identificatie)
    {
        _logger.LogInformation("Fetching object for identificatie {Identificatie}", identificatie);

        try
        {
            var response = await _httpClient.GetAsync($"objects?ordering=record__data__identificatie&data_attr=identificatie__exact__{identificatie}");

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<ObjectResponse>();

            if (result?.Results == null || !result.Results.Any())
            {
                _logger.LogWarning("No object found for identificatie {ObjectId}", identificatie);
                return new ObjectResponse { Results = new List<ObjectResult>() };
            }

            if (result.Results.Count > 1)
            {
                _logger.LogWarning("Multiple objects found for identificatie {Identificatie}. Expected exactly one match.", identificatie);
            }

            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error fetching object for identificatie {Identificatie}", identificatie);
            return new ObjectResponse { Results = new List<ObjectResult>() };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while fetching object for identificatie {Identificatie}", identificatie);
            return new ObjectResponse { Results = new List<ObjectResult>() };
        }
    }
 
}
