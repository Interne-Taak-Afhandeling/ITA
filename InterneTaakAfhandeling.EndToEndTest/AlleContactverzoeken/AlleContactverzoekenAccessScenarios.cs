using InterneTaakAfhandeling.EndToEndTest.Infrastructure;
using Microsoft.Playwright;

namespace InterneTaakAfhandeling.EndToEndTest.AlleContactverzoeken
{
    [TestClass]
    [DoNotParallelize]
    public class AlleContactverzoekenAccessScenarios : ITAPlaywrightTest
    {
        [TestMethod("Beheerder opent de pagina Alle contactverzoeken via directe URL")]
        public async Task Beheerder_CanAccessAlleContactverzoeken_ViaDirecteUrl()
        {
            await Step("Navigate directly to /alle-contactverzoeken");
            await Page.GotoAsync("/alle-contactverzoeken");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("Verify the page is displayed with correct heading");
            var heading = Page.GetByRole(AriaRole.Heading, new() { Name = "Alle contactverzoeken", Level = 1 });
            await Expect(heading).ToBeVisibleAsync();

            await Step("Verify we are not on the forbidden page");
            var forbiddenHeading = Page.GetByRole(AriaRole.Heading, new() { Name = "Toegang geweigerd" });
            await Expect(forbiddenHeading).Not.ToBeVisibleAsync();
        }

        // TODO: Requires a non-admin test user account to be configured in the test infrastructure.
        // The current ITAPlaywrightTest base class only supports a single authenticated user (Beheerder).
        // [TestMethod("Medewerker zonder beheerderrol wordt geblokkeerd bij directe URL")]
        // public async Task NietBeheerder_WordtGeblokkeerd_BijDirecteUrlAlleContactverzoeken()
        // {
        //     await Step("Navigate directly to /alle-contactverzoeken as non-admin");
        //     await Page.GotoAsync("/alle-contactverzoeken");
        //     await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        //
        //     await Step("Verify redirect to forbidden page");
        //     var forbiddenHeading = Page.GetByRole(AriaRole.Heading, new() { Name = "Toegang geweigerd" });
        //     await Expect(forbiddenHeading).ToBeVisibleAsync();
        // }

        // TODO: Requires unauthenticated browser context (no stored auth session).
        // The current ITAPlaywrightTest base class always authenticates before each test.
        // [TestMethod("Niet-ingelogde gebruiker wordt doorgestuurd naar inlogpagina")]
        // public async Task NietIngelogd_WordtDoorgestuurd_NaarInlogpagina()
        // {
        //     await Step("Navigate directly to /alle-contactverzoeken without authentication");
        //     await Page.GotoAsync("/alle-contactverzoeken");
        //     await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        //
        //     await Step("Verify redirect to login page");
        //     Assert.IsTrue(Page.Url.Contains("login.microsoftonline.com") || Page.Url.Contains("/login"),
        //         "User should be redirected to login page");
        // }
    }
}
