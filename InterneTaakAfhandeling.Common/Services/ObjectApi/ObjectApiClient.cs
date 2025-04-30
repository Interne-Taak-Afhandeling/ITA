using System.Net.Http.Headers;
using System.Net.Http.Json;
using InterneTaakAfhandeling.Common.Extensions;
using InterneTaakAfhandeling.Common.Services.ObjectApi.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace InterneTaakAfhandeling.Common.Services.ObjectApi;

public interface IObjectApiClient
{
    Task<List<ObjectRecord>> GetObjectsByIdentificatie(string identificatie);
}

public class ObjectApiClient : IObjectApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ObjectApiClient> _logger;
    private readonly ObjectApiOptions _options;

    public ObjectApiClient(
        HttpClient httpClient,
        IOptions<ObjectApiOptions> options,
        ILogger<ObjectApiClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        _httpClient.BaseAddress = new Uri(_options.BaseUrl);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", _options.ApiKey);
        
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
