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
        // Remove any stale auth state from a previous run
        if (File.Exists(ITAPlaywrightTest.AuthStatePath))
        {
            File.Delete(ITAPlaywrightTest.AuthStatePath);
        }

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

        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new()
        {
            Headless = !ITAPlaywrightTest.IsRunningLocally()
        });

        await using var browserContext = await browser.NewContextAsync(new()
        {
            BaseURL = configuration["TestSettings:TEST_BASE_URL"] ?? "https://ita.test.icatt.nl"
        });
        var page = await browserContext.NewPageAsync();

        var otpHelper = ITAPlaywrightTest.s_uniqueOtpHelper ?? new UniqueOtpHelper(totpSecret);
        var loginHelper = new AzureAdLoginHelper(page, username, password, otpHelper);

        // AzureAdLoginHelper.LoginAsync now handles the ICATT identity-broker chooser screen
        // internally (using the same #social-oidc-icatt selector previously raced here via
        // ClickIcattIfPromptedAsync). Running both concurrently risked a double-click /
        // corrupted session (e.g. two navigations racing each other), so the separate race
        // has been removed in favor of the single, built-in handling in LoginAsync.
        await loginHelper.LoginAsync();

        // Fail fast if the app session was not established: saving state without it makes every
        // test redirect to the Keycloak login page and time out individually.
        var uitloggenLink = page.GetByRole(AriaRole.Link, new() { Name = "Uitloggen" });
        try
        {
            await uitloggenLink.WaitForAsync(new() { Timeout = 30000 });
        }
        catch (TimeoutException)
        {
            throw new InvalidOperationException(
                "Global login finished but the app's logged-in marker (\"Uitloggen\" link) is not visible, " +
                $"so no usable session would be saved. Current page: url='{page.Url}', title='{await page.TitleAsync()}'.");
        }

        // Save authenticated state so all tests can reuse it
        await browserContext.StorageStateAsync(new()
        {
            Path = ITAPlaywrightTest.AuthStatePath
        });
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
