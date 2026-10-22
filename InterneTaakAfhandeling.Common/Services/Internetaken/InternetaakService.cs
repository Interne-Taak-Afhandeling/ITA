using System.Net.Http.Json;
using System.Text.Json;
using InterneTaakAfhandeling.Common.Services.Actors;
using InterneTaakAfhandeling.Common.Services.Klantcontacten;
using InterneTaakAfhandeling.Common.ApiClients.OpenKlantApi;
using InterneTaakAfhandeling.Common.ApiClients.OpenKlantApi.Models;
using Microsoft.Extensions.Logging;

namespace InterneTaakAfhandeling.Common.Services.Internetaken
{
    public interface IInternetaakService
    {
        Task<List<Internetaak>> QueryInterneTakenAsync(InterneTaakQuery interneTaakQueryParameters);
        Task<InternetakenResponse?> GetInternetakenAsync(string path);
        Task<Internetaak> GetInternetaakByIdAsync(Guid uuid);
    }

    public class InternetaakService(
        IOpenKlantApiClient openKlantApiClient,
        IActorService actorService,
        IKlantcontactService klantcontactService,
        ILogger<InternetaakService> logger) : IInternetaakService
    {
        private readonly IOpenKlantApiClient _openKlantApiClient = openKlantApiClient;
        private readonly IActorService _actorService = actorService;
        private readonly IKlantcontactService _klantcontactService = klantcontactService;
        private readonly ILogger<InternetaakService> _logger = logger;

        public async Task<List<Internetaak>> QueryInterneTakenAsync(InterneTaakQuery interneTaakQueryParameters)
        {
            var queryString = interneTaakQueryParameters.BuildQueryString();
            var path = $"internetaken?{queryString}";
            var response = await GetInternetakenAsync(path);

            foreach (var item in response.Results)
            {
                if (item.ToegewezenAanActoren != null)
                {

                    item.ToegewezenAanActoren = [.. (await Task.WhenAll(
                        item.ToegewezenAanActoren
                            .Select(async a =>
                            {
                                if (!string.IsNullOrEmpty(a.Uuid))
                                {
                                    return (await _actorService.GetActorAsync(a.Uuid));
                                }
                                return null;
                            }).Where(x=> x != null)
                    ))];
                }

                if (item.AanleidinggevendKlantcontact?.Uuid != null)
                {
                    item.AanleidinggevendKlantcontact = await _klantcontactService.GetKlantcontactAsync(item.AanleidinggevendKlantcontact.Uuid);
                }

            }

            return response.Results;

        }
       
        public async Task<InternetakenResponse?> GetInternetakenAsync(string path)
        {
            try
            {
                _logger.LogInformation("Fetching internetaken from OpenKlant API");

                var response = await _openKlantApiClient.GetAsyncHttpResponse(path);

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return new InternetakenResponse { Results = [] };

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadFromJsonAsync<InternetakenResponse>();

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
        
        public async Task<Internetaak> GetInternetaakByIdAsync(Guid uuid)
        {
            var result = await _openKlantApiClient.GetAsync<Internetaak>($"internetaken/{uuid}");
            return result!;
        }
    }
}
