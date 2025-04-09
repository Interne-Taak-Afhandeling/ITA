using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using InterneTaakAfhandeling.Poller.Features;
using InterneTaakAfhandeling.Poller.Services.Openklant;
using InterneTaakAfhandeling.Poller.Services.Emailservices.SmtpMailService;
using InterneTaakAfhandeling.Poller.Services.ObjectApi;
using InterneTaakAfhandeling.Poller.Services.Emailservices.Content;
using InterneTaakAfhandeling.Poller.Services.ZakenApi;
using InterneTaakAfhandeling.Poller.Data;

class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            // Build configuration  
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .AddUserSecrets<Program>()
                .Build();

            Log.Logger = new LoggerConfiguration()
                   .ReadFrom.Configuration(configuration)
                   .CreateLogger();

            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found in configuration.");

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
                .AddScoped<IOpenKlantApiClient, OpenKlantApiClient>()
                .AddScoped<IObjectApiClient, ObjectApiClient>()
                .AddScoped<IEmailService, EmailService>()
                .AddScoped<IEmailContentService, EmailContentService>()
                .AddScoped<IInternetakenProcessor, InternetakenNotifier>()
                .AddScoped<IZakenApiClient, ZakenApiClient>();

            var serviceProvider = services.BuildServiceProvider();

            // Run database migrations
            using (var scope = serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                dbContext.Database.Migrate();
            }

            // Get services
            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
            var processor = serviceProvider.GetRequiredService<IInternetakenProcessor>();

            // Retrieve the message from the configuration; fallback if not found
            string message = configuration["PollerMessage"] ?? "Poller executed at";

            Console.WriteLine($"{message} {DateTime.UtcNow}");

            logger.LogInformation("Starting ITA Poller application");

            await processor.NotifyAboutNewInternetakenAsync();
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