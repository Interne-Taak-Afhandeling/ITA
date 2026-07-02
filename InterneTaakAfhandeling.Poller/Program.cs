using InterneTaakAfhandeling.Common.Extensions;
using InterneTaakAfhandeling.Common.Services.Emailservices.Content;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.Poller.Data;
using InterneTaakAfhandeling.Poller.Features;
using InterneTaakAfhandeling.Poller.Features.NieuweInternetaakNotificatie;
using InterneTaakAfhandeling.Poller.Features.VerlopenContactverzoekHerinnering;
using InterneTaakAfhandeling.Poller.Services.NotifierState;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

internal class Program
{
    private static async Task Main(string[] args)
    {
        try
        {
            // Build configuration
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true)
                .AddJsonFile($"appsettings.{environment}.json", true, true)
                .AddEnvironmentVariables()
                .AddUserSecrets<Program>()
                .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            var connectionString = configuration.GetConnectionString("DefaultConnection")
                                   ?? throw new InvalidOperationException(
                                       "Connection string 'DefaultConnection' not found in configuration.");

            // Setup DI
            var services = new ServiceCollection()
                .AddSingleton<IConfiguration>(configuration)
                .AddLogging(builder =>
                {
                    builder.ClearProviders();
                    builder.AddSerilog(dispose: true);
                })
                .AddHttpClient() // Add HttpClient factory
                .AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString))
                .AddITAApiClients(configuration)
                .AddSmtpClients(configuration)
                .AddSingleton<VerlopenContactverzoekHerinneringsTemplateService>()
                .AddScoped<IVerlopenInternetakenProcessor, VerlopenInternetakenProcessor>()
                .AddScoped<INieuweInternetakenProcessor, InternetakenNotifier>()
                .AddScoped<INotifierStateService, NotifierStateService>()
                .AddScoped<IContactmomentenService, ContactmomentenService>()
                .AddKeyedScoped<IPollerJob, NieuweInternetaakNotificatiePollerJob>("nieuwe-internetaak-notificatie")
                .AddKeyedScoped<IPollerJob, VerlopenContactverzoekHerinneringPollerJob>("verlopen-contactverzoek-herinnering");

            var serviceProvider = services.BuildServiceProvider();

            // Run database migrations
            using (var scope = serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                dbContext.Database.Migrate();
            }

            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

            var message = configuration["PollerMessage"] ?? "Poller executed at";
            Console.WriteLine($"{message} {DateTimeOffset.UtcNow}");

            var pollerMode = Environment.GetEnvironmentVariable("POLLER_MODE") ?? "nieuwe-internetaak-notificatie";
            logger.LogInformation("Poller started in mode: {PollerMode}", pollerMode);

            using var runScope = serviceProvider.CreateScope();
            var job = runScope.ServiceProvider.GetKeyedService<IPollerJob>(pollerMode);

            if (job is null)
            {
                logger.LogError("Unknown POLLER_MODE '{PollerMode}' — no job registered for this mode", pollerMode);
                return;
            }

            await job.ExecuteAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
            throw;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
