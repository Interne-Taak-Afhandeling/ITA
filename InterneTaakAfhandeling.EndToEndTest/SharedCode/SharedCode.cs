using InterneTaakAfhandeling.EndToEndTest.Infrastructure;
using InterneTaakAfhandeling.EndToEndTest.Helpers;
using Microsoft.Playwright;
using ITA.InterneTaakAfhandeling.EndToEndTest.Helpers;

namespace ITA.InterneTaakAfhandeling.EndToEndTest.SharedCode
{

    [TestClass]
    public class AnonymousContactVerzoekScenarios : ITAPlaywrightTest
    {
        [TestMethod("2. Contactverzoek creation and search via email for an afdeling")]
        public async Task AnonymousContactVerzoekEmailAfdeling()
        {
            await Step("Given the user is on the KISS demo environment for data creation");
            await Page.GotoAsync("https://dev.kiss-demo.nl");

            await Step("When the user starts a new Contactmoment");
            await Page.CreateNewContactmomentAsync();

            await Step("Wait for page to be fully loaded");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("And clicks on Contactverzoek-pane");
            await Page.CreateNewcontactVerzoekAsync();

            await Step("And Afdeling radiobutton is pre-selected");
            var radio = Page.GetAfdelingRadioButton();
            await radio.CheckAsync();
            Assert.IsTrue(await radio.IsCheckedAsync());

            await Step("And user selects 'parkeren' from dropdown list of field afdeling");
            await Page.GetAfdelingCombobox().FillAsync("Parkeren");
            await Page.GetByText("Parkeren").ClickAsync();

            await Step("And enters 'test automation' in interne toelichting voor medewerker");
            await Page.GetInterneToelichtingTextbox().FillAsync("test automation");

            await Step("And enter 'testautomation@example.com' in field e-mailadres");
            await Page.GetEmailTextbox().FillAsync("testautomation@info.nl");

            await Step("And click on afronden");
            await Page.GetAfrondenButton().ClickAsync();

            await Step("And user fills in 'Hoe gaat het' in the specific vraag field");
            await Page.GetSpecifiekeVraagTextbox().FillAsync("automation test specific vraag");

            await Step("select channel from the list");
            await Page.GetByLabel("Kanaal").SelectOptionAsync(["Balie"]);

            await Step("And user clicks on Opslaan button");

            var klantContactPostResponse = await Page.RunAndWaitForResponseAsync(async () =>
            {
                await Page.GetOpslaanButton().ClickAsync();
            },
                response => response.Url.Contains("/postklantcontacten")
            );

            // // Register cleanup
            // RegisterCleanup(async () =>
            // {
            //     await TestCleanupHelper.CleanupPostKlantContacten(klantContactPostResponse);
            // });

            await Step("user starts a new Contactmoment and navigates to contactverzoek tab");
            await Page.CreateNewContactmomentAsync();
            await Page.GetContactverzoekenLink().ClickAsync();

            await Step("enter email address in field Telefoonnummer of e-mailadres");
            await Page.GetEmailTextbox().FillAsync("testautomation@info.nl");

            await Step("And clicks the Zoeken button");

            await Page.GetZoekenButton().ClickAsync();

            await Step("And contactverzoek details are displayed");

            var matchingRow = Page.Locator("table.overview tbody tr").Filter(new()
            {
                Has = Page.GetByText("automation test specific vraag")
            });

            await matchingRow.First.GetByRole(AriaRole.Button).PressAsync("Enter");

            var contactDetails = Page.GetByText("testautomation@info.nl").First;
            await contactDetails.WaitForAsync(new() { State = WaitForSelectorState.Visible });

            Assert.IsTrue(await contactDetails.IsVisibleAsync(), "The contactverzoek details are not displayed.");
            Assert.IsTrue(await contactDetails.IsVisibleAsync(), "The contactverzoek details are not displayed.");

            await Step("Navigate to ITA homepage and wait");
            await Page.GotoAsync("/");
            
            await Step("Wait on ITA homepage for 3 seconds");
            await Task.Delay(3000);
        }
    }
}
