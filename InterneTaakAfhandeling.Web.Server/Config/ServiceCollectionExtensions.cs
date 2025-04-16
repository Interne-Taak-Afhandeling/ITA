using InterneTaakAfhandeling.Web.Server.Authentication;
using InterneTaakAfhandeling.Web.Server.Features;
using InterneTaakAfhandeling.Web.Server.Services.ObjectApi;
using InterneTaakAfhandeling.Web.Server.Services.OpenKlantApi;

namespace InterneTaakAfhandeling.Web.Server.Config
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            services.AddSingleton<ResourcesConfig>();

            services.AddAuth(options =>
          {
              options.Authority = GetRequiredConfigValue(configuration,"OIDC_AUTHORITY");
              options.ClientId = GetRequiredConfigValue(configuration, "OIDC_CLIENT_ID");
              options.ClientSecret = GetRequiredConfigValue(configuration, "OIDC_CLIENT_SECRET");
              options.ITASystemAccessRole = GetRequiredConfigValue(configuration, "OIDC_ITA_SYSTEM_ACCESS_ROLE");
              options.NameClaimType = configuration["OIDC_NAME_CLAIM_TYPE"];
              options.RoleClaimType = configuration["OIDC_ROLE_CLAIM_TYPE"];
              options.IdClaimType = configuration["OIDC_ID_CLAIM_TYPE"];
          });


            services.AddScoped<IOpenKlantApiClient,OpenKlantApiClient>();
            services.AddScoped<IObjectApiClient,ObjectApiClient>();

            return services;
        }

        static string GetRequiredConfigValue(IConfiguration configuration, string key)
        {
            var value = configuration[key];
            if (string.IsNullOrEmpty(value))
            {
                throw new InvalidOperationException($"Missing required configuration value for '{key}'");
            }
            return value;
        }
    }
}
