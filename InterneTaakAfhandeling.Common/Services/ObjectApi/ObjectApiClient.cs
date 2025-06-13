
using InterneTaakAfhandeling.Common.Services.ObjectApi.Models;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace InterneTaakAfhandeling.Common.Services.ObjectApi;

public interface IObjectApiClient
{
    Task<List<ObjectRecord>> GetObjectsByIdentificatie(string identificatie);
    Task<ObjectRecord> CreateLogboekForInternetaak(Guid internetaakId);
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



    public async Task<ObjectRecord> CreateLogboekForInternetaak(Guid internetaakId)
    {
        try
        {
            var request = new CreateLogboekRequest
            {
                //add to configuration
                Type = "https://objecttypen.dev.kiss-demo.nl/api/v2/objecttypes/5ff0821a-4846-4bd4-ada2-b1f72505eacb",
                Record = new LogboekRecord
                {
                    StartAt = DateTime.Now.ToString("yyyy-MM-dd"),
                    TypeVersion = "1",  //add to configuration
                    Data = new LogboekData
                    {
                        HeeftBetrekkingOp = new ObjectIdentificator
                        {
                            CodeObjecttype = "internetaak", //add to configuration
                            CodeRegister = "openklant",     //add to configuration
                            CodeSoortObjectId = "uuid",     //add to configuration
                            ObjectId = internetaakId.ToString()
                        },

                        Activiteiten = []
                    }
                }
            };

            var response = await _httpClient.PostAsJsonAsync("objects", request);
           
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<ObjectResult>();

            if (result?.Record == null)
            {
                throw new Exception("...");
            }
            
            return result.Record;
            
        }
        catch (HttpRequestException ex)
        {

            //todo: try to read the content to get useful info on the failure:  var contents = await response.Content.ReadAsStringAsync();
            _logger.LogError(ex, "Error ...");
            throw;

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while ...");
            throw;
        }
    }



    const int TruncatedLength = 3;

    private static string TruncateId(string id) =>
        id.Length < TruncatedLength ? "***"
        : $"{id.AsSpan()[..TruncatedLength]}***";
}
