using System.Net.Http.Headers;
using InterneTaakAfhandeling.Common.Services.ObjectApi;
using InterneTaakAfhandeling.Common.Services.ObjectApi.Models;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.Common.Services.ZakenApi;
using InterneTaakAfhandeling.Common.Services.Zgw;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

            // Register ZGW token provider and authentication handler for the ZakenApiClient
            var zaakSysteemBaseUrl = configuration.GetValue<string>("ZaakSysteem:BaseUrl") ?? throw new Exception("ZaakSysteem:BaseUrl configuration value is missing");
            var zaakSysteemKey = configuration.GetValue<string>("ZaakSysteem:Key") ?? throw new Exception("ZaakSysteem:Key configuration value is missing");
            var zaakSysteemClientId = configuration.GetValue<string>("ZaakSysteem:ClientId") ?? throw new Exception("ZaakSysteem:ClientId configuration value is missing");
            var userIdClaimType = configuration.GetValue<string>("OIDC_OBJECTREGISTER_MEDEWERKER_ID_CLAIM_TYPE");

            services.AddSingleton(new ZgwTokenProvider(zaakSysteemKey, zaakSysteemClientId, userIdClaimType));
            services.AddTransient<ZgwAuthenticationHandler>();

            services.AddHttpClient<IZakenApiClient, ZakenApiClient>(client =>
            {
                var defaultCrs = "EPSG:4326";
                client.BaseAddress = new Uri(zaakSysteemBaseUrl);
                client.DefaultRequestHeaders.Add("Accept-Crs", defaultCrs);
            })
            .AddHttpMessageHandler<ZgwAuthenticationHandler>();

            // Register logboek options
            services.AddOptions<LogboekOptions>()
                .Bind(configuration.GetSection("LogBoekOptions"))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddOptions<AfdelingOptions>()
              .Bind(configuration.GetSection("AfdelingOptions"))
              .ValidateDataAnnotations()
              .ValidateOnStart();

            services.AddOptions<GroepOptions>()
             .Bind(configuration.GetSection("GroepOptions"))
             .ValidateDataAnnotations()
             .ValidateOnStart();

            return services;
        }
    }
}
