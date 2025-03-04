using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using ITA.Poller.Features;
using ITA.Poller.Services.Openklant;
using ITA.Poller.Services.Emailservices.SmtpMailService;
using ITA.Poller.Services.ObjectApi;
using ITA.Poller.Services.Emailservices.Content;
using ITA.Poller.Services.Contact; 

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
            .AddScoped<IContactService,ContactService>()
            .AddScoped<IInternetakenProcessor, InternetakenNotifier>()
            .BuildServiceProvider();

            // Get services
            var logger = services.GetRequiredService<ILogger<Program>>();
            var processor = services.GetRequiredService<IInternetakenProcessor>();

            logger.LogInformation("Starting ITA Poller application");

            await processor.ProcessInternetakenAsync();


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