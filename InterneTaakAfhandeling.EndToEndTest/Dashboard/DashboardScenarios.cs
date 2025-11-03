using InterneTaakAfhandeling.Common.Services;
using InterneTaakAfhandeling.Common.Services.ObjectApi;
using InterneTaakAfhandeling.Common.Services.ObjectApi.Models;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using InterneTaakAfhandeling.EndToEndTest.Infrastructure;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;

namespace InterneTaakAfhandeling.EndToEndTest.Dashboard
{
    [TestClass]
    public class DashboardScenarios : ITAPlaywrightTest
    {

        [TestMethod("Data creation and navigation to details page of contactverzoek")]
        public async Task User_CanClickContactverzoekToViewDetails_FromDashboard()
        {
            await Step("Ensure test data exists via API");
            await TestDataHelper.CreateContactverzoek();

            await Step("Navigate to home page");
            await Page.GotoAsync("/");

            await Step("Wait for dashboard to load");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            var testOnderwerp = "Test_Contact_from_ITA_E2E_test";

            await Step("Click on contactverzoek to view details");
            var onderwerpElement = Page.Locator($"text={testOnderwerp}");
            await Expect(onderwerpElement).ToBeVisibleAsync(new() { Timeout = 10000 });

            var testRow = Page.GetByRole(AriaRole.Row).Filter(new() { HasText = testOnderwerp });
            var detailsLink = testRow.GetByRole(AriaRole.Link).Filter(new() { HasText = "Klik hier" });
            await detailsLink.ClickAsync();

            await Step("Verify details page loads");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("Verify contactverzoek details are accessible");
            var onderwerpInDetails = Page.Locator($"text={testOnderwerp}");
            await Expect(onderwerpInDetails).ToBeVisibleAsync(new() { Timeout = 10000 });
        }

        [TestMethod("Test delete functionality for contactverzoek")]
        public async Task TestDeleteContactverzoek()
        {
            await Step("Test deleting existing test data");

            // First create some test data to ensure there's something to delete
            await Step("Create test data first");
            await TestDataHelper.CreateContactverzoek();

            await Step("Now delete the test internetaak");
            await TestDataHelper.DeleteTestInternetaak();

            await Step("Delete the test klantcontact");
            await TestDataHelper.DeleteTestKlantcontact();

            await Step("Verify deletion completed");
            // You can add verification here if needed
        }
    }
}
