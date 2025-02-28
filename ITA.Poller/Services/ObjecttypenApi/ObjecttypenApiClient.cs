using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ITA.Poller.Services.ObjecttypenApi;

public interface IObjecttypenApiClient
{
    Task<string> GetObjectIdAsync(string objectTypeId);
}

public class ObjecttypenApiClient : IObjecttypenApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ObjecttypenApiClient> _logger;
    private readonly string _baseUrl;

    public ObjecttypenApiClient(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<ObjecttypenApiClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _baseUrl = _configuration.GetValue<string>("ObjecttypenApi:BaseUrl")
            ?? throw new InvalidOperationException("ObjecttypenApi:BaseUrl configuration is missing");
        
        var apiKey = _configuration.GetValue<string>("ObjecttypenApi:ApiKey")
            ?? throw new InvalidOperationException("ObjecttypenApi:ApiKey configuration is missing");

        _httpClient.BaseAddress = new Uri(_baseUrl);

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", apiKey);
    }

     public async Task<string> GetObjectIdAsync(string objectTypeId)
    {
        try
        {
            _logger.LogInformation("Fetching object ID for object type {ObjectTypeId}", objectTypeId);

            var response = await _httpClient.GetAsync($"objecttypes/{objectTypeId}");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ObjecttypenResponse>(json);

            if (result?.ObjectId == null)
            {
                throw new InvalidOperationException("ObjectId not found in response");
            }

            _logger.LogInformation("Successfully retrieved object ID {ObjectId} for object type {ObjectTypeId}", 
                result.ObjectId, objectTypeId);

            return result.ObjectId;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error occurred while fetching object ID for type {ObjectTypeId}: {Message}",
                objectTypeId, ex.Message);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON deserialization error occurred while fetching object ID for type {ObjectTypeId}: {Message}",
                objectTypeId, ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while fetching object ID for type {ObjectTypeId}: {Message}",
                objectTypeId, ex.Message);
            throw;
        }
    }

    private class ObjecttypenResponse
    {
        public string? ObjectId { get; set; }
    }
}
