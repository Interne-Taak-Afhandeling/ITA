using InterneTaakAfhandeling.EndToEndTest.Infrastructure;
using Microsoft.Playwright;

namespace InterneTaakAfhandeling.EndToEndTest.Contactverzoek
{
    [TestClass]
    [DoNotParallelize]
    public class AlleContactverzoekenNavigatieScenarios : ITAPlaywrightTest
    {
        [TestMethod("Beheerder ziet 'Alle contactverzoeken' in de navigatie")]
        public async Task Beheerder_ZietAlleContactverzoeken_InNavigatie()
        {
            await Step("Given een medewerker met de rol 'Beheerder' is ingelogd");
            await Page.GotoAsync("/");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("When de navigatie wordt weergegeven");
            var navLink = Page.GetByRole(AriaRole.Link, new() { Name = "Alle contactverzoeken" });

            await Step("Then is de optie 'Alle contactverzoeken' zichtbaar");
            await Expect(navLink).ToBeVisibleAsync();
        }

        // TODO: Scenario 2 — "Medewerker zonder beheerderrol ziet 'Alle contactverzoeken' niet"
        // Requires a second test user WITHOUT the "Beheerder" role.
        // Currently only one test account (with Beheerder role) is configured.
        // To implement: add TEST_USERNAME_NON_ADMIN / TEST_PASSWORD_NON_ADMIN to user-secrets,
        // create a login flow for that user, and assert:
        //   await Expect(navLink).Not.ToBeVisibleAsync();
    }
}
