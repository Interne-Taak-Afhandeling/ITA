using System.Net.Http.Json;
using InterneTaakAfhandeling.Common.Services.ObjectApi.Models; 
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace InterneTaakAfhandeling.Common.Services.ObjectApi;

public interface IObjectApiClient
{
    Task<List<ObjectRecord<MedewerkerObjectData>>> GetObjectsByIdentificatie(string identificatie);
    Task<ObjectResult<LogboekData>> CreateLogboekForInternetaak(Guid internetaakId);
    Task<ObjectResult<LogboekData>?> GetLogboek(Guid internetaakId);
    Task<LogboekData> UpdateLogboek(ObjectPatchModel<LogboekData> logboekData, Guid logboekDataUuid);
}

public class ObjectApiClient(
    HttpClient httpClient,
    ILogger<ObjectApiClient> logger,
    IOptions<LogboekOptions> logboekOptions) : IObjectApiClient
{
    private const int TruncatedLength = 3;
    private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    private readonly LogboekOptions _logboekOptions = logboekOptions.Value;
    private readonly ILogger<ObjectApiClient> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<List<ObjectRecord<MedewerkerObjectData>>> GetObjectsByIdentificatie(string identificatie)
    {
        var truncated = TruncateId(identificatie);
        _logger.LogInformation("Fetching objects for identificatie {Identificatie}", truncated);

        try
        {
            var response = await _httpClient.GetAsync(
                $"objects?ordering=record__data__identificatie&data_attr=identificatie__exact__{identificatie}");
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<ObjectModels<MedewerkerObjectData>>();

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
            _logger.LogError(ex, "Unexpected error occurred while fetching objects for identificatie {Identificatie}",
                truncated);
            return [];
        }
    }


    public async Task<ObjectResult<LogboekData>?> GetLogboek(Guid internetaakId)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"objects?data_attr=heeftBetrekkingOp__objectId__exact__{internetaakId}&type={_logboekOptions.Type}&typeVersion={_logboekOptions.TypeVersion}");
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<ObjectModels<LogboekData>>();

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

            return result.Results.Single();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error fetching Logboek for internetaak {internetaakId}", internetaakId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while fetching a Logboek for internetaak {internetaakId}",
                internetaakId);
            throw;
        }
    }


    public async Task<ObjectResult<LogboekData>> CreateLogboekForInternetaak(Guid internetaakId)
    {

        HttpResponseMessage? response = null;

        try
        {
            var request = new LogboekModel
            {
                Type = _logboekOptions.Type,
                Record = new LogboekRecord
                {
                    StartAt = DateOnly.FromDateTime(DateTime.Now),
                    TypeVersion = _logboekOptions.TypeVersion,
                    Data = new LogboekData
                    {
                        HeeftBetrekkingOp = new ObjectIdentificator
                        {
                            CodeObjecttype = _logboekOptions.InternetaakCodeObjectType,
                            CodeRegister = _logboekOptions.InternetaakCodeRegister,
                            CodeSoortObjectId = _logboekOptions.InternetaakCodeSoortObjectId,
                            ObjectId = internetaakId.ToString()
                        },

                        Activiteiten = []
                    }
                }
            };

            response = await _httpClient.PostAsJsonAsync("objects", request);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<ObjectResult<LogboekData>>();

            if (result?.Record?.Data == null) throw new Exception("No Logboek found");

            return result;
        }
        catch (HttpRequestException ex)
        {
            var errorResponse = response != null ? await response.Content.ReadAsStringAsync() : "";

            _logger.LogError(ex, "Error creating logboek for internetaak {internetaakId}. Error response {errorResponse}", internetaakId, errorResponse);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while ...");
            throw;
        }
    }

    public async Task<LogboekData> UpdateLogboek(ObjectPatchModel<LogboekData> logboekPatch, Guid logboekUuid)
    {
        try
        {
            var response = await _httpClient.PatchAsJsonAsync($"objects/{logboekUuid}", logboekPatch);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<ObjectResult<LogboekData>>();

            if (result?.Record?.Data == null) throw new Exception("No Logboek found");

            return result.Record.Data;
        }

        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while updateing the logboek for ");
            throw;
        }
    }


    private static string TruncateId(string id)
    {
        return id.Length < TruncatedLength
            ? "***"
            : $"{id.AsSpan()[..TruncatedLength]}***";
    }
}