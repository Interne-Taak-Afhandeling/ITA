
using InterneTaakAfhandeling.Common.Services.ObjectApi.Models;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;

namespace InterneTaakAfhandeling.Common.Services.ObjectApi;

public interface IObjectApiClient
{
    Task<List<ObjectRecord<MedewerkerObjectData>>> GetObjectsByIdentificatie(string identificatie);
    Task<LogboekData> CreateLogboekForInternetaak(Guid internetaakId);
    Task<LogboekData?> GetLogboek(Guid internetaakId);
}

public class ObjectApiClient(
    HttpClient httpClient,
    ILogger<ObjectApiClient> logger,IOptions<LogboekOptions> logboekOptions) : IObjectApiClient
{
    private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    private readonly ILogger<ObjectApiClient> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly LogboekOptions _logboekOptions = logboekOptions.Value;  
     
    public async Task<List<ObjectRecord<MedewerkerObjectData>>> GetObjectsByIdentificatie(string identificatie)
    {
        var truncated = TruncateId(identificatie);
        _logger.LogInformation("Fetching objects for identificatie {Identificatie}", truncated);

        try
        {
            var response = await _httpClient.GetAsync($"objects?ordering=record__data__identificatie&data_attr=identificatie__exact__{identificatie}");
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<ObjectResponse<MedewerkerObjectData>>();

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


    public async Task<LogboekData?> GetLogboek(Guid internetaakId)
    {
        try
        {
            //todo: get objecttype from config
            //var objecttype = "https://objecttypen.dev.kiss-demo.nl/api/v2/objecttypes/5ff0821a-4846-4bd4-ada2-b1f72505eacb";

            var response = await _httpClient.GetAsync($"objects?data_attr=heeftBetrekkingOp__objectId__exact__{internetaakId}&type={_logboekOptions.Type}");
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<ObjectResponse<LogboekData>>();

            if (result?.Results == null || result.Results.Count == 0)
            {
                _logger.LogInformation("No Logboek found for internetaak {internetaakId}", internetaakId);
                return null;
            }

            if (result.Results.Count > 1)
            {
                _logger.LogWarning("Multiple Logboeken found for internetaak {internetaakId}", internetaakId);
                return null;
            }

            return result.Results.Single().Record?.Data;
                
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error fetching Logboek for internetaak {internetaakId}", internetaakId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while fetching a Logboek for internetaak {internetaakId}", internetaakId);
            throw;
        }
    }



    public async Task<LogboekData> CreateLogboekForInternetaak(Guid internetaakId)
    {
        try
        {
            var request = new LogboekModels
            { 
                Type =  _logboekOptions.Type, 
                Record = new LogboekRecord
                {
                    StartAt = DateTime.Now.ToString("yyyy-MM-dd"),
                    TypeVersion =  _logboekOptions.TypeVersion, 
                    Data = new LogboekData
                    {
                        HeeftBetrekkingOp = new ObjectIdentificator
                        {
                            CodeObjecttype = _logboekOptions.CodeObjectType, 
                            CodeRegister =_logboekOptions.CodeRegister, 
                            CodeSoortObjectId = _logboekOptions.CodeSoortObjectId, 
                            ObjectId = internetaakId.ToString()
                        },

                        Activiteiten = []
                    }
                }
            };

            var response = await _httpClient.PostAsJsonAsync("objects", request);
           
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<ObjectResult<LogboekData>>();

            if (result?.Record?.Data == null)
            {
                throw new Exception("...");
            }
            
            return result.Record.Data;
            
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
