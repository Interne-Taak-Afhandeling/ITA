using Microsoft.AspNetCore.Builder;

namespace InterneTaakAfhandeling.Web.Server.Config
{
    public static class ConfigurationExtensions
    {
        public static WebApplicationBuilder ConfigureForEnvironment(this WebApplicationBuilder builder)
        {
            if (builder.Environment.EnvironmentName == "Docker")
            {
                builder.Configuration.AddUserSecrets<Program>();
            }

            return builder;
        }
    }
}