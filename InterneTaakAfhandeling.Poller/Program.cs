using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

class Program
{
    static async Task Main()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();

        // Retrieve the message from the configuration; fallback if not found
        string message = configuration["PollerMessage"] ?? "Poller executed at";

        Console.WriteLine($"{message} {DateTime.UtcNow}");
        await Task.Delay(1000);
    }
}
