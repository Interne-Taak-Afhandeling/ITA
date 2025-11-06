using InterneTaakAfhandeling.Common.Services;
using InterneTaakAfhandeling.Common.Services.ObjectApi;
using InterneTaakAfhandeling.Common.Services.ObjectApi.Models;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using InterneTaakAfhandeling.EndToEndTest.Infrastructure;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;

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

            await Step("Verify user is logged in and home page loads");

            // Check we're not redirected to Microsoft login
            var currentUrl = Page.Url;
            Assert.IsFalse(currentUrl.Contains("login.microsoftonline.com"),
                "User should be logged in and not redirected to Microsoft login page");

            var pageTitle = await Page.TitleAsync();
            var bodyText = await Page.Locator("body").TextContentAsync();
            Assert.IsTrue(!string.IsNullOrWhiteSpace(bodyText),
                "Page should have content and not be blank");

            await Step("Verify 'Mijn werkvoorraad' heading is present");
            var werkvoorraadHeading = Page.GetByRole(Microsoft.Playwright.AriaRole.Heading, new() { Name = "Mijn werkvoorraad" });
            await Expect(werkvoorraadHeading).ToBeVisibleAsync();

            await Step("Wait on homepage for 3 seconds");
            await Task.Delay(3000);

        }


    }
}