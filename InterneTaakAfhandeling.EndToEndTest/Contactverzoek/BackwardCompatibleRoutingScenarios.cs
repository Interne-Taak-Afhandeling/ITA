using InterneTaakAfhandeling.EndToEndTest.Infrastructure;
using Microsoft.Playwright;

namespace InterneTaakAfhandeling.EndToEndTest.Contactverzoek
{
    /// <summary>
    /// E2E test for backward-compatible routing (feature #299, task #375).
    /// This test verifies that old deeplinks with the interne-taaknummer (/contactverzoek/{nummer})
    /// still work after introducing the new /contactmoment/{nummer} route.
    /// 
    /// Commented out because this test requires all feature tasks (#372, #373, #375) to be merged
    /// into the feature branch before it can run successfully.
    /// Finalize and uncomment at feature-level verification.
    /// </summary>
    [TestClass]
    [DoNotParallelize]
    public class BackwardCompatibleRoutingScenarios : ITAPlaywrightTest
    {
        [TestMethod("Navigatie via oud interne-taaknummer op oude route /contactverzoek werkt")]
        public async Task OudeRoute_MetInterneTaaknummer_ToontContactverzoekMetContactmomentNummer()
        {
            await Step("Given een internetaak met een aanleidinggevend klantcontact");
            var testOnderwerp = $"Test_BackwardRoute_{Guid.NewGuid().ToString()[..8]}";
            var contactmomentUuid = await TestDataHelper.CreateContactverzoek(testOnderwerp, attachZaak: false);
            RegisterCleanup(async () => await TestDataHelper.DeleteContactverzoekAsync(contactmomentUuid.ToString()));

            await Step("Look up the internetaak nummer and contactmoment nummer");
            var internetaakUuid = await TestDataHelper.GetInternetaakUuidFromContactmomentAsync(contactmomentUuid);
            Assert.IsNotNull(internetaakUuid, "Internetaak UUID should be found");

            var internetaak = await TestDataHelper.GetInternetaakByIdAsync(internetaakUuid.Value);
            Assert.IsNotNull(internetaak.Nummer, "Internetaak nummer should be found");

            var contactmomentNummer = internetaak.AanleidinggevendKlantcontact?.Nummer;
            Assert.IsNotNull(contactmomentNummer, "AanleidinggevendKlantcontact.Nummer should be available");

            await Step($"When de medewerker navigeert naar /contactverzoek/{internetaak.Nummer} via een oude e-maillink");
            await Page.GotoAsync($"/contactverzoek/{internetaak.Nummer}");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("Then wordt het contactverzoek correct getoond");
            await Expect(Page.Locator($"text={testOnderwerp}")).ToBeVisibleAsync();

            await Step($"And de heading toont 'Contactverzoek {contactmomentNummer}'");
            await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = $"Contactverzoek {contactmomentNummer}" })).ToBeVisibleAsync();
        }
    }
}
