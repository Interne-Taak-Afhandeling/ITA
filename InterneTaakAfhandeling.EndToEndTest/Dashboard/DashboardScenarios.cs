using InterneTaakAfhandeling.EndToEndTest.Infrastructure;
using Microsoft.Playwright;

namespace InterneTaakAfhandeling.EndToEndTest.Dashboard
{
    [TestClass]
    public class DashboardScenarios : ITAPlaywrightTest
    {
        // NOTE: This is a proof of concept test to validate data creation and cleanup functionality
        [TestMethod("Data creation and navigation to details page of contactverzoek that has ZAAK connected to it")]
        public async Task User_CanClickContactverzoekToViewDetails_FromDashboard()
        {
            var testOnderwerp = "Test_Contact_from_ITA_E2E_test";

            await Step("Ensure test data exists via API");
            var contactverzoekWithZaak = await TestDataHelper.CreateContactverzoek(testOnderwerp, attachZaak: true);

            await Step("Navigate to home page");
            await Page.GotoAsync("/");

            await Step("Wait for dashboard to load");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("Click on contactverzoek to view details");
            var onderwerpElement = Page.Locator($"text={testOnderwerp}");
            await Expect(onderwerpElement).ToBeVisibleAsync();

            var testRow = Page.GetByRole(AriaRole.Row).Filter(new() { HasText = testOnderwerp });
            var detailsLink = testRow.GetByRole(AriaRole.Link).Filter(new() { HasText = "Klik hier" });
            await detailsLink.ClickAsync();

            await Step("Verify details page loads");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("Verify contactverzoek details are accessible");
            var onderwerpInDetails = Page.Locator($"text={testOnderwerp}");
            await Expect(onderwerpInDetails).ToBeVisibleAsync(new() { Timeout = 10000 });
            var zaakElement = Page.Locator($"text={"ZAAK-2023-002"}");
            await Expect(zaakElement).ToBeVisibleAsync(new() { Timeout = 10000 });

            await Step("Delete the test klantcontact and contactverzoek");
            await TestDataHelper.DeleteTestKlantcontact(contactverzoekWithZaak);
        }
    }
}
