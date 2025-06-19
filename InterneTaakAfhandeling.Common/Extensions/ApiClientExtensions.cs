using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using InterneTaakAfhandeling.Common.Services.ObjectApi;
using InterneTaakAfhandeling.Common.Services.ObjectApi.Models;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.Common.Services.ZakenApi;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace InterneTaakAfhandeling.Common.Extensions
{

    public static class ApiClientExtensions
    {
        
       

        public static IServiceCollection AddITAApiClients(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpClient<IOpenKlantApiClient, OpenKlantApiClient>(client =>
            {
                var openKlantApiBaseUrl = configuration.GetValue<string>("OpenKlantApi:BaseUrl") ?? throw new Exception("OpenKlantApi:BaseUrl configuration value is missing");
                var openKlantApiKey = configuration.GetValue<string>("OpenKlantApi:ApiKey") ?? throw new Exception("OpenKlantApi:ApiKey configuration value is missing");

                client.BaseAddress = new Uri(openKlantApiBaseUrl);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", openKlantApiKey);
            });

            services.AddHttpClient<IObjectApiClient, ObjectApiClient>(client =>
            {
                var objectApiBaseUrl = configuration.GetValue<string>("ObjectApi:BaseUrl") ?? throw new Exception("ObjectApi:BaseUrl configuration value is missing");
                var objectApiKey = configuration.GetValue<string>("ObjectApi:ApiKey") ?? throw new Exception("ObjectApi:ApiKey configuration value is missing");
                client.BaseAddress = new Uri(objectApiBaseUrl);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", objectApiKey);
                client.DefaultRequestHeaders.Add("Content-Crs", "EPSG:4326");
            });

            services.AddHttpClient<IZakenApiClient, ZakenApiClient>(client =>
            {
                var zaakSysteemBaseUrl = configuration.GetValue<string>("ZaakSysteem:BaseUrl") ?? throw new Exception("ZaakSysteem:BaseUrl configuration value is missing");
                var zaakSysteemKey = configuration.GetValue<string>("ZaakSysteem:Key") ?? throw new Exception("ZaakSysteem:Key configuration value is missing");
                var zaakSysteemClientId = configuration.GetValue<string>("ZaakSysteem:ClientId") ?? throw new Exception("ZaakSysteem:ClientId configuration value is missing");
                var DefaultCrs = "EPSG:4326";

               client.BaseAddress = new Uri(zaakSysteemBaseUrl);
                client.DefaultRequestHeaders.Add("Accept-Crs", DefaultCrs);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GenerateZakenApiToken(zaakSysteemKey, zaakSysteemClientId));
            });
            // registered logboek options 
            services.Configure<LogboekOptions>(configuration.GetSection("LogBoekOption"));
                
            
            return services;
        } 
 

          public static string GenerateZakenApiToken(string JwtSecretKey, string ClientId)
        { 

            // Convert secret key to bytes  
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Set issued-at (iat) timestamp  
            var issuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            // Create JWT payload  
            var claims = new List<Claim>
           {
               new ("client_id", ClientId),
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