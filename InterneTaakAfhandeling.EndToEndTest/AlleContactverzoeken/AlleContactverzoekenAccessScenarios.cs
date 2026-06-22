using System.Text.RegularExpressions;
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

            await Step("Verify the page is displayed with correct heading");
            var heading = Page.GetByRole(AriaRole.Heading, new() { Name = "Alle contactverzoeken", Level = 1 });
            await Expect(heading).ToBeVisibleAsync();

            await Step("Verify we are not on the forbidden page");
            var forbiddenHeading = Page.GetByRole(AriaRole.Heading, new() { Name = "Toegang geweigerd" });
            await Expect(forbiddenHeading).Not.ToBeVisibleAsync();
        }

        [TestMethod("Alle contactverzoeken router-links gebruiken het contactmoment-nummer in de URL (Feature #299)")]
        public async Task AlleContactverzoeken_RouterLinks_GebruikenContactmomentNummer_InUrl()
        {
            await Step("Given een internetaak met een aanleidinggevend klantcontact");
            var testOnderwerp = $"Test_AlleCV_RouterLink_{Guid.NewGuid().ToString()[..8]}";
            var contactmomentUuid = await TestDataHelper.CreateContactverzoek(testOnderwerp, attachZaak: false);
            RegisterCleanup(async () => await TestDataHelper.DeleteContactverzoekAsync(contactmomentUuid.ToString()));

            await Step("Look up the contactmoment nummer");
            var internetaakUuid = await TestDataHelper.GetInternetaakUuidFromContactmomentAsync(contactmomentUuid);
            Assert.IsNotNull(internetaakUuid, "Internetaak UUID should be found");

            var internetaak = await TestDataHelper.GetInternetaakByIdAsync(internetaakUuid.Value);
            var contactmomentNummer = internetaak.AanleidinggevendKlantcontact?.Nummer;
            Assert.IsNotNull(contactmomentNummer, "AanleidinggevendKlantcontact.Nummer should be available");

            await Step("Navigate to /alle-contactverzoeken");
            await Page.GotoAsync("/alle-contactverzoeken");

            await Step("Click the details link for the test contactverzoek");
            var detailsLink = Page.GetDetailsLink(testOnderwerp);
            await detailsLink.WaitForAsync(new() { State = WaitForSelectorState.Visible });
            await detailsLink.ClickAsync();

            await Step("Verify URL uses /contactmoment/ with the contactmoment number");
            await Expect(Page).ToHaveURLAsync(new Regex($"/contactmoment/{Regex.Escape(contactmomentNummer)}"));

            await Step("Verify heading shows the contactmoment number");
            await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = $"Contactverzoek {contactmomentNummer}" })).ToBeVisibleAsync();
        }

        // NOT IMPLEMENTED — scenarios excluded from E2E coverage (Feature #299):
        //
        // "Fout als klantcontact-nummer null is" (#372) and
        // "Foutmelding als contactmoment-nummer ontbreekt" (#373):
        //   AanleidinggevendKlantcontact.Nummer is always present for valid Open Klant data.
        //   The null state cannot be triggered from the test environment.
        //
        // All e-mail scenarios (#374 — onderwerp, deeplink, doorsturen, fout bij ontbrekend nummer):
        //   Playwright tests only what is rendered in the browser. SMTP-delivered e-mail
        //   content is not accessible and cannot be asserted.

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
