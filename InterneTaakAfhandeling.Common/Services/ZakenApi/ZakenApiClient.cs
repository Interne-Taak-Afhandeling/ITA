using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using InterneTaakAfhandeling.Common.Extensions;
using InterneTaakAfhandeling.Common.Services.ZakenApi.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace InterneTaakAfhandeling.Common.Services.ZakenApi
{
    public interface IZakenApiClient
    {
        Task<Zaak?> GetZaakAsync(string uuid);
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
      
    }
}
