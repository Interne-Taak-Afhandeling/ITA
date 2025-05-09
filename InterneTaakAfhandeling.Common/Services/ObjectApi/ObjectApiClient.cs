
using InterneTaakAfhandeling.Common.Services.ObjectApi.Models;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace InterneTaakAfhandeling.Common.Services.ObjectApi;

public interface IObjectApiClient
{
    Task<List<ObjectRecord>> GetObjectsByIdentificatie(string identificatie);
}

public class ObjectApiClient(
    HttpClient httpClient,
    ILogger<ObjectApiClient> logger) : IObjectApiClient
{
    private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    private readonly ILogger<ObjectApiClient> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<List<ObjectRecord>> GetObjectsByIdentificatie(string identificatie)
    {
        var truncated = TruncateId(identificatie);
        _logger.LogInformation("Fetching objects for identificatie {Identificatie}", truncated);

        try
        {
            var response = await _httpClient.GetAsync($"objects?ordering=record__data__identificatie&data_attr=identificatie__exact__{identificatie}");
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<ObjectResponse>();

            if (result?.Results == null || result.Results.Count == 0)
            {
                _logger.LogWarning("No objects found for identificatie {Identificatie}", truncated);
                return [];
            }


            return result.Results
                .Where(r => r?.Record != null)
                .Select(r => r.Record)
                .ToList();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error fetching objects for identificatie {Identificatie}", truncated);
            return [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while fetching objects for identificatie {Identificatie}", truncated);
            return [];
        }
    }

    const int TruncatedLength = 3;

    private static string TruncateId(string id) =>
        id.Length < TruncatedLength ? "***"
        : $"{id.AsSpan()[..TruncatedLength]}***";
}
