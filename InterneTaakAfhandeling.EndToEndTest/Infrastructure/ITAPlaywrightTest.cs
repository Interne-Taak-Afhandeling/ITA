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
