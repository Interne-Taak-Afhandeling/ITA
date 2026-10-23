using InterneTaakAfhandeling.Common.Services.Email.Content;
using InterneTaakAfhandeling.Common.Services.Email.Recipients;
using InterneTaakAfhandeling.Common.Services.Email.SmtpMailService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InterneTaakAfhandeling.Common.Extensions;

public static class SmtpClientExtensions
{
    public static IServiceCollection AddSmtpClients(this IServiceCollection services, IConfiguration configuration)
    {
                     
        services.Configure<SmtpSettings>(configuration.GetSection("Email:SmtpSettings"));
        services.AddOptions<SmtpSettings>()
            .Bind(configuration.GetSection("Email:SmtpSettings"));
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IEmailContentService, EmailContentService>();
        services.AddSingleton<IActorEmailResolutionService, ActorEmailResolutionService>();
        return services;
    }
}
