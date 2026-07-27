using System;
using InterneTaakAfhandeling.EndToEndTest.Infrastructure;
using Microsoft.Playwright;

namespace InterneTaakAfhandeling.EndToEndTest.Contactverzoek
{
    // Feature #514 — Toegang tot Contactverzoeken beperkt tot eigen afdeling/groep
    // Task #544 — Phase 2 E2E-verificatie (implementatie is al gemerged, zie ITA-514)
    [TestClass]
    [DoNotParallelize]
    public class ContactverzoekToegangsbeperkingenScenarios : ITAPlaywrightTest
    {
        // The only afdeling confirmed present in the test objectenregister today (used
        // throughout TestDataHelper).
        private const string BestaandeAfdeling = "Burgerzaken_ibz";

        [TestMethod("Beheerder kan Contactverzoek van elke afdeling inzien")]
        public async Task Beheerder_CanAccessContactverzoek_VanAndereAfdeling()
        {
            // Any afdeling proves the point here — the beheerder-exception in
            // ContactverzoekAutorisatieGuardService bypasses the afdeling/groep check entirely,
            // so we use the one afdeling confirmed to exist in the test objectenregister.
            await Step("Given een Contactverzoek toegewezen aan afdeling 'Burgerzaken_ibz'");
            var onderwerp = $"Test_Toegang_Beheerder_{Guid.NewGuid().ToString()[..8]}";
            var (contactmomentUuid, klantcontactNummer, _) = await TestDataHelper.CreateContactverzoekWithAfdelingOnlyAndContactDatum(
                onderwerp, DateTime.UtcNow, BestaandeAfdeling);
            RegisterCleanup(async () => await TestDataHelper.DeleteContactverzoekAsync(contactmomentUuid.ToString()));

            await Step("When de beheerder het Contactverzoek opent via de detail-URL");
            await Page.GotoAsync($"/contactmoment/{klantcontactNummer}");

            await Step("Then wordt de Contactverzoek-detailpagina getoond met inhoudelijke gegevens");
            await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = $"Contactverzoek {klantcontactNummer}" })).ToBeVisibleAsync();
            await Expect(Page.Locator($"text={onderwerp}")).ToBeVisibleAsync();
        }
    }
}
