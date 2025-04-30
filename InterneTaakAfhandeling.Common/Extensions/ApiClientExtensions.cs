using InterneTaakAfhandeling.Common.Services.ObjectApi;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.Common.Services.ZakenApi;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InterneTaakAfhandeling.Common.Extensions
{

    public static class ApiClientExtensions
    {
        public static IServiceCollection AddITAApiClients(this IServiceCollection services, IConfiguration configuration)
        {

            services.ConfigureITAApiClientOptions(configuration);


            services.AddHttpClient<IOpenKlantApiClient, OpenKlantApiClient>();


            services.AddHttpClient<IObjectApiClient, ObjectApiClient>();


            services.AddHttpClient<IZakenApiClient, ZakenApiClient>();

            return services;
        }

        public static IServiceCollection ConfigureITAApiClientOptions(
            this IServiceCollection services,
            IConfiguration configuration)
        {

            services.Configure<OpenKlantApiOptions>(options =>
            {
                options.BaseUrl = configuration.GetValue<string>("OpenKlantApi:BaseUrl") ??
                                  throw new InvalidOperationException("OpenKlantApi BaseUrl configuration is missing");

                options.ApiKey = configuration.GetValue<string>("OpenKlantApi:ApiKey") ??
                                 throw new InvalidOperationException("OpenKlantApi ApiKey configuration is missing");

            });


            services.Configure<ObjectApiOptions>(options =>
            {
                options.BaseUrl = configuration.GetValue<string>("ObjectApi:BaseUrl") ??
                                  throw new InvalidOperationException("ObjectApi BaseUrl configuration is missing");

                options.ApiKey = configuration.GetValue<string>("ObjectApi:ApiKey") ??
                                 throw new InvalidOperationException("ObjectApi ApiKey configuration is missing");

            });


            services.Configure<ZakenApiOptions>(options =>
            {

                options.BaseUrl = configuration.GetValue<string>("ZaakSysteem:BaseUrl") ??
                                  throw new InvalidOperationException("ZakenApi BaseUrl configuration is missing");
 

                options.JwtSecretKey = configuration.GetValue<string>("ZaakSysteem:Key") ??
                                          throw new InvalidOperationException("ZakenApi Key configuration is missing");

                options.ClientId = configuration.GetValue<string>("ZaakSysteem:ClientId")
                                    ?? throw new InvalidOperationException("ZakenApi ClientId configuration is missing");
            });

            return services;
        }
    }
}