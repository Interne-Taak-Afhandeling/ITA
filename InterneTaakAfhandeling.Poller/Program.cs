using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using InterneTaakAfhandeling.Poller.Features;
using InterneTaakAfhandeling.Poller.Services.Openklant;
using InterneTaakAfhandeling.Poller.Services.Emailservices.SmtpMailService;

 

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
        .AddHttpClient<IOpenKlantApiClient, OpenKlantApiClient>() 
        .Services
        .AddSingleton<IEmailService, EmailService>()
        .AddSingleton<IInternetakenProcessor, InternetakenNotifier>()
        .BuildServiceProvider();

    // Get services
    var logger = services.GetRequiredService<ILogger<Program>>();
    var processor = services.GetRequiredService<IInternetakenProcessor>();

    logger.LogInformation("Starting ITA Poller application");

    try
    {
        await processor.ProcessInternetakenAsync();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Application error occurred");
        throw;
    }
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