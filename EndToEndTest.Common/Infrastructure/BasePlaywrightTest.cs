using Microsoft.Playwright;
using Microsoft.Playwright.MSTest;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Configuration;
using System.Collections.Concurrent;
using System.Text.Encodings.Web;
using System.Reflection;

namespace EndToEndTest.Common.Infrastructure
{
    /// <summary>
    /// Base class for Playwright-based end-to-end tests with Azure AD authentication.
    /// Inherit this class in each test class. This does the following:<br/>
    /// 1. Makes sure the user is logged in before each test starts<br/>
    /// 2. Makes sure Playwright records traces for each test<br/>
    /// 3. Exposes a <see cref="Step(string)"/> method to define test steps. These show up in the Playwright traces and in the test report.<br/>
    /// 4. Builds a html test report after all tests in a test class are done.
    /// </summary>
    [TestClass]
    public abstract class BasePlaywrightTest : PageTest
    {
        private const string StoragePath = "./auth.json";

        static BasePlaywrightTest()
        {
            // Configure browser to show automatically in local development
            var isLocal = IsRunningLocally();
            if (isLocal && string.IsNullOrEmpty(Environment.GetEnvironmentVariable("HEADED")))
            {
                Environment.SetEnvironmentVariable("HEADED", "1");
            }
        }

        /// <summary>
        /// Detects if tests are running locally (not in CI/CD)
        /// </summary>
        private static bool IsRunningLocally()
        {
            // Check common CI environment variables
            var ciEnvironments = new[]
            {
                "CI", "GITHUB_ACTIONS", "AZURE_PIPELINES"
            };

            return !ciEnvironments.Any(env => !string.IsNullOrEmpty(Environment.GetEnvironmentVariable(env)));
        }

        private static readonly IConfiguration s_configuration = BuildConfiguration();

        private static IConfiguration BuildConfiguration()
        {
            // Load .env file if it exists
            var envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
            if (File.Exists(envPath))
            {
                DotNetEnv.Env.Load(envPath);
            }

            return new ConfigurationBuilder()
                .AddUserSecrets<BasePlaywrightTest>()
                .AddEnvironmentVariables()
                .Build();
        }

        // Initialize UniqueOtpHelper if TOTP secret is available
        private static readonly UniqueOtpHelper? s_uniqueOtpHelper = CreateOtpHelperIfConfigured();

        // this is used to build a test report 
        private static readonly ConcurrentDictionary<string, string> s_testReports = [];

        private readonly List<string> _steps = [];

        // clean up actions that are registered by the tests
        private readonly List<Func<Task>> _cleanupActions = [];

        protected static IConfiguration Configuration => s_configuration;

        public BasePlaywrightTest()
        {
        }

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
        /// Start a test step. This ends up in the test report
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
                // stop tracing and save a zip file in the output directory
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
                ? $"""<p><strong>Trace:</strong> <a target="_blank" href="https://trace.playwright.dev/?trace=https://interne-taak-afhandeling.github.io/ITA/{fileName}">View Playwright Trace</a></p>"""
                : $"""<p><strong>Trace:</strong> <em>Not available (test failed during initialization)</em></p>""";

            // For failed tests
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

        private static string ExtractOutcome(string html)
        {
            var outcomeMatch = System.Text.RegularExpressions.Regex.Match(html, @"data-outcome=""([^""]+)""");
            return outcomeMatch.Success ? outcomeMatch.Groups[1].Value : "Unknown";
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
        /// This is run after all tests in a test class are done
        /// </summary>
        /// <returns></returns>
        [ClassCleanup(InheritanceBehavior.BeforeEachDerivedClass)]
        public static async Task ClassCleanup()
        {
            if (s_testReports.Count == 0)
            {
                return;
            }

            // Create directory first
            var tracesDir = Path.Combine(Environment.CurrentDirectory, "playwright-traces");
            Directory.CreateDirectory(tracesDir);

            // Group tests by class name first, then by outcome
            var testsByClass = s_testReports
                .GroupBy(kvp => ExtractClassName(kvp.Value))
                .OrderBy(group => group.Key)
                .ToList();

            var totalTests = s_testReports.Count;
            var passedTests = s_testReports.Count(kvp => ExtractOutcome(kvp.Value) == "Passed");
            var failedTests = s_testReports.Count(kvp => ExtractOutcome(kvp.Value) == "Failed");

            var html = GenerateHtmlReport(testsByClass, totalTests, passedTests, failedTests);

            var htmlPath = Path.Combine(Environment.CurrentDirectory, "playwright-traces", "index.html");
            await File.WriteAllTextAsync(htmlPath, html);

            await Task.CompletedTask;
        }

        private static string GenerateHtmlReport(List<IGrouping<string, KeyValuePair<string, string>>> testsByClass, int totalTests, int passedTests, int failedTests)
        {
            return $$"""
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <meta http-equiv="Content-Security-Policy" content="default-src 'none'; style-src https://unpkg.com/simpledotcss@2.3.3/simple.min.css 'sha256-l0D//z1BZPnhAdIJ0lA8dsfuil0AB4xBpnOa/BhNVoU=' 'unsafe-inline';">
    <title>ITA E2E Test Report</title>
    <link rel="stylesheet" href="https://unpkg.com/simpledotcss@2.3.3/simple.min.css">
    <style>
        [data-outcome=Failed] { border-left: 4px solid #dc3545; }
        [data-outcome=Passed] { border-left: 4px solid #28a745; }
        
        .test-group { margin-bottom: 2rem; }
        .class-group { margin-bottom: 3rem; border: 2px solid var(--border); border-radius: 10px; overflow: hidden; }
        .class-header { 
            background: linear-gradient(135deg, var(--accent), var(--accent-bg)); 
            color: var(--accent-text);
            padding: 1.5rem; 
            margin: 0;
            border-bottom: 2px solid var(--border);
        }
        .class-header h2 { margin: 0; color: inherit; }
        .class-content { padding: 1.5rem; }
        
        .outcome-group { margin-bottom: 2rem; }
        .group-header { 
            background: var(--accent-bg); 
            padding: 1rem; 
            border-radius: 8px; 
            margin-bottom: 1rem;
        }
        
        .test-item { 
            background: var(--bg); 
            border: 1px solid var(--border); 
            border-radius: 6px; 
            padding: 1rem; 
            margin-bottom: 0.5rem; 
        }
        
        .test-header { 
            display: flex; 
            justify-content: space-between; 
            align-items: center; 
        }
        
        .test-header h4 { 
            margin: 0; 
            flex-grow: 1; 
        }
        
        .test-details { 
            margin-top: 0.5rem; 
        }
        
        .test-steps { 
            margin-top: 1rem; 
        }
        
        .test-steps ol { 
            margin: 0.5rem 0; 
            padding-left: 1.5rem; 
        }
        
        .summary-box { 
            background: var(--accent-bg); 
            padding: 2rem; 
            border-radius: 10px; 
            margin-bottom: 2rem; 
            text-align: center; 
        }
        
        .summary-stats { 
            display: flex; 
            justify-content: space-around; 
            flex-wrap: wrap; 
            gap: 1rem; 
            margin-top: 1rem; 
        }
        
        .stat { 
            text-align: center; 
        }
        
        .stat-value { 
            font-size: 2rem; 
            font-weight: bold; 
            display: block; 
        }
        
        .passed { color: #28a745; }
        .failed { color: #dc3545; }
        .total { color: var(--text); }
        
        .error-details { 
            background: #f8d7da; 
            border: 1px solid #f5c6cb; 
            color: #721c24; 
            padding: 0.75rem; 
            border-radius: 4px; 
            margin-top: 0.5rem; 
        }
    </style>
</head>
<body>
    <header>
        <h1>🧪 ITA End-to-End Test Report</h1>
        <p>Generated on {{DateTime.Now:yyyy-MM-dd HH:mm:ss}}</p>
    </header>

    <main>
        <div class="summary-box">
            <h2>Test Summary</h2>
            <div class="summary-stats">
                <div class="stat">
                    <span class="stat-value total">{{totalTests}}</span>
                    <span>Total Tests</span>
                </div>
                <div class="stat">
                    <span class="stat-value passed">{{passedTests}}</span>
                    <span>Passed</span>
                </div>
                <div class="stat">
                    <span class="stat-value failed">{{failedTests}}</span>
                    <span>Failed</span>
                </div>
            </div>
        </div>

        {{string.Join("", testsByClass.Select(classGroup => $"""
        <div class="class-group">
            <div class="class-header">
                <h2>{HtmlEncoder.Default.Encode(classGroup.Key)}</h2>
            </div>
            <div class="class-content">
                {string.Join("", classGroup.GroupBy(kvp => ExtractOutcome(kvp.Value)).Select(outcomeGroup => $"""
                <div class="outcome-group">
                    <div class="group-header">
                        <h3>{outcomeGroup.Key} Tests ({outcomeGroup.Count()})</h3>
                    </div>
                    {string.Join("", outcomeGroup.Select(kvp => $"""
                    <div class="test-item">
                        {kvp.Value}
                    </div>
                    """))}
                </div>
                """))}
            </div>
        </div>
        """))}}
    </main>

    <footer>
        <p>Report generated by End-to-End Test Suite</p>
    </footer>
</body>
</html>
""";
        }

        /// <summary>
        /// Override this method to provide project-specific context options
        /// </summary>
        /// <returns></returns>
        public override BrowserNewContextOptions ContextOptions()
        {
            var baseUrl = GetBaseUrl();

            return new(base.ContextOptions())
            {
                BaseURL = baseUrl,
                StorageStatePath = null,
                ViewportSize = new() { Width = 1920, Height = 1080 },
            };
        }

        /// <summary>
        /// Override this method to provide project-specific base URL
        /// </summary>
        /// <returns></returns>
        protected abstract string GetBaseUrl();

        protected void RegisterCleanup(Func<Task> cleanupFunc)
        {
            _cleanupActions.Add(cleanupFunc);
        }

        private static UniqueOtpHelper? CreateOtpHelperIfConfigured()
        {
            var totpSecret = s_configuration["TestSettings:TEST_TOTP_SECRET"];
            return string.IsNullOrEmpty(totpSecret) ? null : new UniqueOtpHelper(totpSecret);
        }

        private async Task HandleAuthenticationAsync()
        {
            var username = s_configuration["TestSettings:TEST_USERNAME"];
            var password = s_configuration["TestSettings:TEST_PASSWORD"];

            // If no credentials are configured, skip authentication
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                return;
            }

            // Clear any existing auth state to force fresh login every time
            if (File.Exists(StoragePath))
            {
                File.Delete(StoragePath);
            }

            // login helper and perform fresh login every time
            var loginHelper = new AzureAdLoginHelper(Page, username, password, s_uniqueOtpHelper ?? throw new InvalidOperationException("TOTP helper not initialized"));
            await loginHelper.LoginAsync();
        }
    }
}
