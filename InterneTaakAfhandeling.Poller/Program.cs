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

            // Setup DI
            var services = new ServiceCollection()
            .AddSingleton<IConfiguration>(configuration)
            .AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddSerilog(dispose: true);
            })
            .AddHttpClient() // Add HttpClient factory
            .AddScoped<IOpenKlantApiClient, OpenKlantApiClient>()  
            .AddScoped<IObjectApiClient, ObjectApiClient>()  
            .AddScoped<IEmailService, EmailService>()
            .AddScoped<IEmailContentService,EmailContentService>() 
            .AddScoped<IInternetakenProcessor, InternetakenNotifier>() 
            .AddScoped<IZakenApiClient, ZakenApiClient>()
            .BuildServiceProvider();

            // Get services
            var logger = services.GetRequiredService<ILogger<Program>>();
            var processor = services.GetRequiredService<IInternetakenProcessor>();

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