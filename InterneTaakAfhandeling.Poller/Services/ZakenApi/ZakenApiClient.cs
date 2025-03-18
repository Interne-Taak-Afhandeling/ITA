using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection.PortableExecutable;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using InterneTaakAfhandeling.Poller.Services.Openklant;
using InterneTaakAfhandeling.Poller.Services.ZakenApi.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace InterneTaakAfhandeling.Poller.Services.ZakenApi
{
   public interface IZakenApiClient
    {
        Task<Zaak?> GetZaakAsync(string uuid);
    }
    public class ZakenApiClient : IZakenApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ZakenApiClient> _logger;
        private const string DefaultCrs = "EPSG:4326";

        public ZakenApiClient(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<ZakenApiClient> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            var baseUrl = _configuration.GetValue<string>("ZaakSysteem:BaseUrl")
                ?? throw new InvalidOperationException("ZaakSysteem:BaseUrl configuration is missing");
            _httpClient.BaseAddress = new Uri(baseUrl);  
           _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GenerateToken());
            _httpClient.DefaultRequestHeaders.Add("Accept-Crs", DefaultCrs);
           
        }

        public async Task<Zaak?> GetZaakAsync(string uuid)
        { 
                var response = await _httpClient.GetAsync($"zaken/api/v1/zaken/{uuid}");                
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<Zaak>();
         
             
        }
        public string GenerateToken()
        {

            var secretKey = _configuration["ZaakSysteem:Key"];
            var client_id = _configuration["ZaakSysteem:ClientId"];
            var iss = _configuration["ZaakSysteem:ClientId"];
            // Convert secret key to bytes
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Set issued-at (iat) timestamp
            var issuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();  

            // Create JWT payload
            var claims = new[]
            {
            new Claim("client_id", client_id),
            new Claim("iat", issuedAt.ToString(), ClaimValueTypes.Integer64)
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
