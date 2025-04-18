using System.Net.Http.Headers;
using InterneTaakAfhandeling.Web.Server.Middleware;
using InterneTaakAfhandeling.Web.Server.Services.ObjectApi.Models; 

namespace InterneTaakAfhandeling.Web.Server.Services.ObjectApi;

public interface IObjectApiClient
{
    Task<List<ObjectRecord>> GetObjectsByEmail(string email);
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

    public async Task<List<ObjectRecord>> GetObjectsByEmail(string email)
    { 

        try
        {
            var response = await _httpClient.GetAsync($"objects?ordering=record__data__email&data_attr=email__exact__{email}");
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<ObjectResponse>();
            
            if (result?.Results == null || result.Results.Count == 0)
            {
               throw new ConflictException("No object records found for the current user.", code: "NO_OBJECT_RECORDS_FOUND");
            }
            
            return result.Results
                .Select(r => r.Record)
                .ToList();
        } 
        catch (Exception)
        { 
            throw;
        }
    }
}
