using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using ITA.Poller.Services.Openklant.Models;

namespace ITA.Poller.Services.Openklant;

public interface IOpenKlantApiClient
{
    Task<Internetaken?> GetInternetakenAsync();
}

public class OpenKlantApiClient : IOpenKlantApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OpenKlantApiClient> _logger;

    public OpenKlantApiClient(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<OpenKlantApiClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));


        _httpClient.BaseAddress = new Uri(_configuration.GetValue<string>("OpenKlantApi:BaseUrl")
            ?? throw new InvalidOperationException("OpenKlantApi:BaseUrl configuration is missing"));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token",
            _configuration.GetValue<string>("OpenKlantApi:ApiKey") ?? throw new InvalidOperationException("OpenKlantApi:ApiKey configuration is missing"));



    }

    public async Task<Internetaken?> GetInternetakenAsync()
    {
        try
        {
            _logger.LogInformation("Fetching internetaken from OpenKlant API");

            var response = await _httpClient.GetAsync("internetaken");
            response.EnsureSuccessStatusCode();


            var content = await response.Content.ReadFromJsonAsync<Internetaken>();

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
}
