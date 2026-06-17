using EndToEndTest.Common.Infrastructure;
using InterneTaakAfhandeling.EndToEndTest.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[assembly: DoNotParallelize]

namespace InterneTaakAfhandeling.EndToEndTest;

[TestClass]
public static class GlobalSetup
{
    [AssemblyInitialize]
    public static async Task SetupSharedAuth(TestContext context)
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<ITAPlaywrightTest>()
            .AddEnvironmentVariables()
            .Build();

        var username = configuration["TestSettings:TEST_USERNAME"];
        var password = configuration["TestSettings:TEST_PASSWORD"];
        var totpSecret = configuration["TestSettings:TEST_TOTP_SECRET"];

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(totpSecret))
        {
            return; // No credentials configured, skip shared auth
        }

        var playwright = await Playwright.CreateAsync();
        var browser = await playwright.Chromium.LaunchAsync(new()
        {
            Headless = !ITAPlaywrightTest.IsRunningLocally()
        });

        var browserContext = await browser.NewContextAsync(new()
        {
            BaseURL = configuration["TestSettings:TEST_BASE_URL"] ?? "https://ita.test.icatt.nl"
        });
        var page = await browserContext.NewPageAsync();

        var otpHelper = ITAPlaywrightTest.s_uniqueOtpHelper ?? new UniqueOtpHelper(totpSecret);
        var loginHelper = new AzureAdLoginHelper(page, username, password, otpHelper);
        await loginHelper.LoginAsync();

        // Save authenticated state so all tests can reuse it
        await browserContext.StorageStateAsync(new()
        {
            Path = ITAPlaywrightTest.AuthStatePath
        });

        await browser.CloseAsync();
        playwright.Dispose();
    }

    [AssemblyCleanup]
    public static void Cleanup()
    {
        if (File.Exists(ITAPlaywrightTest.AuthStatePath))
        {
            File.Delete(ITAPlaywrightTest.AuthStatePath);
        }
    }
}
