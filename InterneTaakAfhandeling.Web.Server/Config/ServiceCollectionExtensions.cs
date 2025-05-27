using InterneTaakAfhandeling.Common.Extensions;
using InterneTaakAfhandeling.Web.Server.Authentication;
using InterneTaakAfhandeling.Web.Server.Features;
using InterneTaakAfhandeling.Web.Server.Features.AssignInternetaakToMyself;
using InterneTaakAfhandeling.Web.Server.Features.CreateKlantContact;
using InterneTaakAfhandeling.Web.Server.Middleware;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.Web.Server.Services;

namespace InterneTaakAfhandeling.Web.Server.Config
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterServices(this IServiceCollection services, IConfiguration configuration)
        {

            services.AddControllers();
            services.AddRouting(options =>
            {
                options.LowercaseUrls = true;
                options.LowercaseQueryStrings = true;
            });
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            services.AddSingleton<ResourcesConfig>();

            services.AddAuth(options =>
          {
              options.Authority = GetRequiredConfigValue(configuration, "OIDC_AUTHORITY");
              options.ClientId = GetRequiredConfigValue(configuration, "OIDC_CLIENT_ID");
              options.ClientSecret = GetRequiredConfigValue(configuration, "OIDC_CLIENT_SECRET");
              options.ITASystemAccessRole = GetRequiredConfigValue(configuration, "OIDC_ITA_SYSTEM_ACCESS_ROLE");
              options.NameClaimType = configuration["OIDC_NAME_CLAIM_TYPE"];
              options.RoleClaimType = configuration["OIDC_ROLE_CLAIM_TYPE"];
              options.ObjectregisterMedewerkerIdClaimType = configuration["OIDC_OBJECTREGISTER_MEDEWERKER_ID_CLAIM_TYPE"];
              options.EmailClaimType = configuration["OIDC_EMAIL_CLAIM_TYPE"];
          });


            services.AddITAApiClients(configuration);

            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ICreateKlantContactService, CreateKlantContactService>();
            services.AddScoped<IInternetakenService, InternetakenService>();
            services.AddScoped<IContactmomentenService, ContactmomentenService>();
            services.AddScoped<IInterneTakenOverviewService, InterneTakenOverviewService>();

            services.AddScoped<IInternetakenService, InternetakenService>();
            services.AddScoped<IKlantcontactService, KlantcontactService>();
            services.AddScoped<ICreateKlantContactService, CreateKlantContactService>();
            services.AddScoped<IAssignInternetaakToMyselfService, AssignInternetaakToMyselfService>();

            services.AddExceptionHandler<ExceptionToProblemDetailsMapper>();

            services.AddProblemDetails();


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
