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
        private readonly ZakenApiOptions _options;
        private readonly ILogger<ZakenApiClient> _logger;
        private const string DefaultCrs = "EPSG:4326";

        public ZakenApiClient(
            HttpClient httpClient,
            IOptions<ZakenApiOptions> options,
            ILogger<ZakenApiClient> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _httpClient.BaseAddress = new Uri(_options.BaseUrl);
            _httpClient.DefaultRequestHeaders.Add("Accept-Crs", DefaultCrs); 
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GenerateToken());
        }

        public async Task<Zaak?> GetZaakAsync(string uuid)
        {
            var response = await _httpClient.GetAsync($"zaken/api/v1/zaken/{uuid}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Zaak>();
        }
        public string GenerateToken()
        {
            var secretKey = _options.JwtSecretKey;
            var client_id = _options.ClientId;
            var iss = _options.ClientId;

            // Convert secret key to bytes  
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Set issued-at (iat) timestamp  
            var issuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            // Create JWT payload  
            var claims = new List<Claim>
           {
               new ("client_id", client_id),
               new("iat", issuedAt.ToString(), ClaimValueTypes.Integer64)
           };

            var token = new JwtSecurityToken(
                claims: claims,
                signingCredentials: credentials
            );

            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(token);
        }

    }
}
