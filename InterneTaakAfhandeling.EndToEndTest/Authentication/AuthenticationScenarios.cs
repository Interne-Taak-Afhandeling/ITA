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
            

            await Page.WaitForLoadStateAsync();
            
            var currentUrl = Page.Url;
            Assert.IsFalse(currentUrl.Contains("login.microsoftonline.com"), 
                "User should be logged in and not redirected to Microsoft login page");
        
            var pageTitle = await Page.TitleAsync();
            var bodyText = await Page.Locator("body").TextContentAsync();
            Assert.IsTrue(!string.IsNullOrWhiteSpace(bodyText), 
                "Page should have content and not be blank");

            await Step("Stay on home page for 2 seconds to verify successful login");
            await Task.Delay(2000); // Wait 2 seconds to see the home page
            
            await Step("Verify 'Mijn werkvoorraad' heading is present");
            var werkvoorraadHeading = Page.GetByRole(AriaRole.Heading, new() { Name = "Mijn werkvoorraad" });
            await Expect(werkvoorraadHeading).ToBeVisibleAsync();

        }

        
    }
}