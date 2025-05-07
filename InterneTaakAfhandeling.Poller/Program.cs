using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using InterneTaakAfhandeling.Poller.Features;
using InterneTaakAfhandeling.Poller.Services.Emailservices.SmtpMailService;
using InterneTaakAfhandeling.Poller.Services.Emailservices.Content;
using InterneTaakAfhandeling.Poller.Data;
using InterneTaakAfhandeling.Poller.Services.NotifierState; 
using InterneTaakAfhandeling.Common.Extensions;

class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            // Build configuration
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
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
                .AddITAApiClients(configuration) 
                .AddScoped<IEmailService, EmailService>()
                .AddScoped<IEmailContentService, EmailContentService>()
                .AddScoped<IInternetakenProcessor, InternetakenNotifier>()
                .AddScoped<INotifierStateService, NotifierStateService>();

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

            Console.WriteLine($"{message} {DateTimeOffset.UtcNow}");

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