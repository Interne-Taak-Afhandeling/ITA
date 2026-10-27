using InterneTaakAfhandeling.Common.ApiClients;
using InterneTaakAfhandeling.Common.ApiClients.ObjectApi;
using InterneTaakAfhandeling.Common.ApiClients.ObjectApi.Models;
using InterneTaakAfhandeling.Common.ApiClients.OpenKlantApi;
using InterneTaakAfhandeling.Common.ApiClients.OpenKlantApi.Models;
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

            // Wait for the heading first — proves the SPA has rendered
            await Step("Verify 'Mijn werkvoorraad' heading is present");
            var werkvoorraadHeading = Page.GetByRole(Microsoft.Playwright.AriaRole.Heading, new() { Name = "Mijn werkvoorraad" });
            await Expect(werkvoorraadHeading).ToBeVisibleAsync();

            var bodyText = await Page.Locator("body").TextContentAsync();
            Assert.IsTrue(!string.IsNullOrWhiteSpace(bodyText),
                "Page should have content and not be blank");
        }


    }
}