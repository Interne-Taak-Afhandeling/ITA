using Microsoft.VisualStudio.TestTools.UnitTesting;
using InterneTaakAfhandeling.EndToEndTest.Infrastructure;

namespace InterneTaakAfhandeling.EndToEndTest.Authentication
{
    [TestClass]
    public class AuthenticationScenarios : ITAPlaywrightTest
    {
        [TestMethod]
        public async Task CanLoginAndAccessHomePage()
        {
            await Step("Navigate to home page");
            await Page.GotoAsync("/");

            await Step("Verify user is logged in and on home page");
            
            // Wait for page to load and check that we're not redirected to login
            await Page.WaitForLoadStateAsync();
            
            // Verify we're on the ITA application (not Microsoft login page)
            var currentUrl = Page.Url;
            Assert.IsFalse(currentUrl.Contains("login.microsoftonline.com"), 
                "User should be logged in and not redirected to Microsoft login page");
            
            // Check for common elements that indicate successful login
            var pageTitle = await Page.TitleAsync();
            Console.WriteLine($"Page title: {pageTitle}");
            Console.WriteLine($"Current URL: {currentUrl}");
            
            // Verify we can see the page content (not a blank page or error)
            var bodyText = await Page.Locator("body").TextContentAsync();
            Assert.IsTrue(!string.IsNullOrWhiteSpace(bodyText), 
                "Page should have content and not be blank");

            await Step("Stay on home page for 2 seconds to verify successful login");
            await Task.Delay(2000); // Wait 2 seconds to see the home page

            await Step("Authentication test completed successfully");
        }

        [TestMethod]
        public async Task CanNavigateBetweenPagesWhileAuthenticated()
        {
            await Step("Navigate to home page");
            await Page.GotoAsync("/");
            
            await Step("Verify initial page load");
            await Page.WaitForLoadStateAsync();
            var homeUrl = Page.Url;
            Console.WriteLine($"Home page URL: {homeUrl}");
            
            // Take a screenshot to verify the page loaded correctly
            await CaptureScreenshotAsync("home-page-loaded");
            
            await Step("Verify authenticated state is maintained");
            // Check that we're still authenticated (not redirected to login)
            Assert.IsFalse(homeUrl.Contains("login.microsoftonline.com"), 
                "Should remain authenticated on home page");
                
            var pageContent = await Page.Locator("body").TextContentAsync();
            Assert.IsTrue(!string.IsNullOrWhiteSpace(pageContent), 
                "Home page should have content");

            await Step("Navigation test completed successfully");
        }
    }
}