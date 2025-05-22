using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using InterneTaakAfhandeling.Common.Extensions;
using InterneTaakAfhandeling.Common.Helpers;
using InterneTaakAfhandeling.Common.Services.ZakenApi.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace InterneTaakAfhandeling.Common.Services.ZakenApi
{
    public interface IZakenApiClient
    {
        Task<Zaak?> GetZaakAsync(string uuid);
        Task<Zaak?> GetZaakByIdentificatieAsync(string identificatie);
    }

    public class ZakenApiClient : IZakenApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ZakenApiClient> _logger;

        public ZakenApiClient(
            HttpClient httpClient,
            ILogger<ZakenApiClient> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Zaak?> GetZaakAsync(string uuid)
        {
            var response = await _httpClient.GetAsync($"zaken/api/v1/zaken/{uuid}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Zaak>();
        }

        public async Task<Zaak?> GetZaakByIdentificatieAsync(string identificatie)
        {
            if (string.IsNullOrWhiteSpace(identificatie))
            {
                throw new ArgumentException("Identificatie is vereist", nameof(identificatie));
            }

            try
            {
                var queryString = $"?identificatie={Uri.EscapeDataString(identificatie)}";
                var safeIdentificatie = SecureLogging.SanitizeAndTruncate(identificatie, 50);
                _logger.LogInformation($"Zoeken naar zaak met identificatie: {safeIdentificatie}, URL: zaken/api/v1/zaken{queryString}");

                var response = await _httpClient.GetAsync($"zaken/api/v1/zaken{queryString}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Zoekopdracht naar zaak met identificatie {safeIdentificatie} retourneerde status {response.StatusCode}");

                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        return null;
                    }

                    response.EnsureSuccessStatusCode();
                }

                var results = await response.Content.ReadFromJsonAsync<ZaakResults>();

                if (results?.Results == null || results.Results.Count == 0)
                {
                    _logger.LogInformation($"Geen zaken gevonden met identificatie: {safeIdentificatie}");
                    return null;
                }

                if (results.Results.Count > 1)
                {
                    _logger.LogWarning($"Meerdere zaken gevonden met identificatie: {safeIdentificatie}, de eerste wordt gebruikt");
                }

                return results.Results[0];
            }
            catch (Exception ex)
            {
                var safeIdentificatie = SecureLogging.SanitizeAndTruncate(identificatie, 50);
                _logger.LogError(ex, $"Fout bij ophalen van zaak met identificatie: {safeIdentificatie}");
                throw;
            }
        }
    }

    public class ZaakResults
    {
        public int Count { get; set; }
        public string? Next { get; set; }
        public string? Previous { get; set; }
        public List<Zaak> Results { get; set; } = new List<Zaak>();
    }
}