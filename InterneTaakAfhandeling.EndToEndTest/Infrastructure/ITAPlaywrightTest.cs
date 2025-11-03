using InterneTaakAfhandeling.Common.Services.ObjectApi.Models;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;
using System;
using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Encodings.Web;
using EndToEndTest.Common.Infrastructure;

namespace InterneTaakAfhandeling.EndToEndTest.Infrastructure
{
    /// <summary>
    /// ITA-specific Playwright test class.
    /// Inherits from BasePlaywrightTest and provides ITA-specific configuration.
    /// </summary>
    [TestClass]
    public class ITAPlaywrightTest : BasePlaywrightTest
    {
        private const string StoragePath = "./auth.json";

        public TestDataHelper TestDataHelper { get; }
        public OpenKlantApiClient OpenKlantApiClient { get; }
        // public TestCleanupHelper TestCleanupHelper { get; }


        public ITAPlaywrightTest()
        {
            // Configure browser to show automatically in local development (like KISS-frontend)
            var isLocal = IsRunningLocally();
            if (isLocal && string.IsNullOrEmpty(Environment.GetEnvironmentVariable("HEADED")))
            {
                Environment.SetEnvironmentVariable("HEADED", "1");
            }


            var openklantApiBaseUrl = GetRequiredConfig("OpenKlantApi:BaseUrl");
            var openklantApiKey = GetRequiredConfig("OpenKlantApi:ApiKey");

            var objectenApiBaseUrl = GetRequiredConfig("ObjectApi:BaseUrl");
            var objectenApiKey = GetRequiredConfig("ObjectApi:ApiKey");

            var services = new ServiceCollection();
            services.AddOptions();
            services.Configure<AfdelingOptions>(s_configuration.GetSection("AfdelingOptions"));
            services.Configure<GroepOptions>(s_configuration.GetSection("GroepOptions"));
            services.Configure<LogboekOptions>(s_configuration.GetSection("LogboekOptions"));
            var serviceProvider = services.BuildServiceProvider();
            var afdelingOptions = serviceProvider.GetRequiredService<IOptions<AfdelingOptions>>();
            var groepOptions = serviceProvider.GetRequiredService<IOptions<GroepOptions>>();
            var logboekOptions = serviceProvider.GetRequiredService<IOptions<LogboekOptions>>();

            var username = s_configuration["TestSettings:TEST_USERNAME"];

            TestDataHelper = new TestDataHelper(
                openklantApiBaseUrl,
                openklantApiKey,
                objectenApiBaseUrl,
                objectenApiKey,
                logboekOptions,
                afdelingOptions,
                groepOptions,
                username);

            // Initialize TestCleanupHelper with OpenKlantApiClient
            services.AddLogging();
            var serviceProviderWithLogging = services.BuildServiceProvider();
            var logger = serviceProviderWithLogging.GetRequiredService<ILogger<OpenKlantApiClient>>();

            var openKlantHttpClient = new System.Net.Http.HttpClient
            {
                BaseAddress = new System.Uri(openklantApiBaseUrl)
            };
            openKlantHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", openklantApiKey);
            OpenKlantApiClient = new OpenKlantApiClient(openKlantHttpClient, logger);

            // TestCleanupHelper = new TestCleanupHelper(openKlantApiClient);

        }



        /// <summary>
        /// Detects if tests are running locally (not in CI/CD)
        /// </summary>
        private static bool IsRunningLocally()
        {
            // Check common CI environment variables
            var ciEnvironments = new[]
            {
                "CI","GITHUB_ACTIONS", "AZURE_PIPELINES"           };

            return !ciEnvironments.Any(env => !string.IsNullOrEmpty(Environment.GetEnvironmentVariable(env)));
        }

        private static readonly Microsoft.Extensions.Configuration.IConfiguration s_configuration = BuildConfiguration();

        private static Microsoft.Extensions.Configuration.IConfiguration BuildConfiguration()
        {
            // Load .env file if it exists
            var envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
            if (File.Exists(envPath))
            {
                DotNetEnv.Env.Load(envPath);
            }

            return new ConfigurationBuilder()
                .AddUserSecrets<ITAPlaywrightTest>()
                .AddEnvironmentVariables()
                .Build();
        }


        // Initialize UniqueOtpHelper if TOTP secret is available
        private static readonly UniqueOtpHelper? s_uniqueOtpHelper = CreateOtpHelperIfConfigured();

        // this is used to build afdelingOptions test report 
        private static readonly ConcurrentDictionary<string, string> s_testReports = [];

        private readonly List<string> _steps = [];

        // clean up actions that are registered by the tests
        private readonly List<Func<Task>> _cleanupActions = [];

        protected static Microsoft.Extensions.Configuration.IConfiguration Configuration => s_configuration;



        /// <summary>
        /// This is run before each test
        /// </summary>
        /// <returns></returns>
        [TestInitialize]
        public virtual async Task TestInitialize()
        {
            // Handle Azure AD authentication if credentials are configured
            await HandleAuthenticationAsync();

            // start tracing (after authentication to keep credentials out of traces)
            await Context.Tracing.StartAsync(new()
            {
                Title = $"{TestContext.FullyQualifiedTestClassName}.{TestContext.TestName}",
                Screenshots = true,
                Snapshots = true,
                Sources = true,
            });
        }

        /// <summary>
        /// Start afdelingOptions test step. This ends up in the test report
        /// </summary>
        /// <param name="description"></param>
        /// <returns></returns>
        protected async Task Step(string description)
        {
            _steps.Add($"{DateTime.Now:HH:mm:ss.fff} - {description}");
            await Task.CompletedTask;
        }

        /// <summary>
        /// This is run after each test
        /// </summary>
        /// <returns></returns>
        [TestCleanup]
        public async Task TestCleanup()
        {

            var fileName = $"{TestContext.FullyQualifiedTestClassName}.{TestContext.TestName}.zip";
            var fullPath = Path.Combine(Environment.CurrentDirectory, "playwright-traces", fileName);

            Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "playwright-traces"));

            try
            {
                // stop tracing and save afdelingOptions zip file in the output directory
                await Context.Tracing.StopAsync(new()
                {
                    Path = fullPath
                });
            }
            catch (PlaywrightException ex) when (ex.Message.Contains("Must start tracing"))
            {
                fileName = ""; // No trace file available
            }
            catch (Exception)
            {
                fileName = ""; // No trace file available
            }

            // Get the descriptive test name from the TestMethod attribute
            var descriptiveTestName = GetDescriptiveTestName();

            // Extract the actual class name from the fully qualified class name
            var actualClassName = TestContext.FullyQualifiedTestClassName?.Split('.').LastOrDefault() ?? "Unknown";

            // Enhanced HTML report with descriptive test name and error handling
            var traceSection = !string.IsNullOrEmpty(fileName)
                ? $"""<p><strong>Trace:</strong> <a target="_blank" href="https://trace.playwright.dev/?trace=./playwright-traces/{fileName}">View Playwright Trace</a></p>"""
                : $"""<p><strong>Trace:</strong> <em>Not available (test failed during initialization)</em></p>""";

            // For failed tests, we'll show basic failure info
            var errorSection = TestContext.CurrentTestOutcome == UnitTestOutcome.Failed
                ? $"""
        <div class="error-details">
            <p><strong>Status:</strong> Test Failed</p>
            <p><strong>Note:</strong> Check console output for detailed error information</p>
        </div>
        """ : "";

            var html = $"""
    <div data-outcome="{TestContext.CurrentTestOutcome}" data-test-name="{HtmlEncoder.Default.Encode(descriptiveTestName)}" data-class-name="{HtmlEncoder.Default.Encode(actualClassName)}">
        <div class="test-header">
            <h4>{HtmlEncoder.Default.Encode(descriptiveTestName)}</h4>
        </div>
        <div class="test-details">
            {traceSection}
        </div>
        {errorSection}
        <details class="test-steps">
            <summary>Test Steps ({_steps.Count})</summary>
            <ol>{string.Join("", _steps.Select(step => $"""
                <li>{HtmlEncoder.Default.Encode(step)}</li>
            """))}
            </ol>
        </details>
    </div>
    """;

            // Always add the test report, even for failed tests
            s_testReports.TryAdd(TestContext.TestName!, html);

            // Run cleanup actions in reverse order
            foreach (var cleanup in ((IEnumerable<Func<Task>>)_cleanupActions).Reverse())
            {
                try
                {
                    await cleanup();
                }
                catch (Exception)
                {
                    // Silently handle cleanup failures
                }
            }
        }

        private static string ExtractClassName(string html)
        {
            // Try to get the class name from the data-class-name attribute we store
            var classNameMatch = System.Text.RegularExpressions.Regex.Match(html, @"data-class-name=""([^""]+)""");
            if (classNameMatch.Success && !string.IsNullOrEmpty(classNameMatch.Groups[1].Value))
            {
                return classNameMatch.Groups[1].Value;
            }

            // If no class name found, return "Unknown Class"
            return "Unknown Class";
        }

        private string GetDescriptiveTestName()
        {
            try
            {
                var testMethod = GetType().GetMethod(TestContext.TestName!);
                var testMethodAttribute = testMethod?.GetCustomAttribute<TestMethodAttribute>();

                if (testMethodAttribute != null && !string.IsNullOrEmpty(testMethodAttribute.DisplayName))
                {
                    return testMethodAttribute.DisplayName;
                }

                return TestContext.TestName ?? "Unknown Test";
            }
            catch (Exception)
            {
                return TestContext.TestName ?? "Unknown Test";
            }
        }

        /// <summary>
        /// This is run after all tests in afdelingOptions test class are done
        /// <summary>
        /// Provides the ITA-specific base URL from configuration
        /// </summary>
        /// <returns></returns>
        protected override string GetBaseUrl()
        {
            var baseUrl = Configuration["TestSettings:TEST_BASE_URL"];
            if (string.IsNullOrEmpty(baseUrl))
            {
                throw new InvalidOperationException(
                    "TEST_BASE_URL is not configured. Please set it in user secrets or environment variables.");
            }
            return baseUrl;
        }
    }
}
