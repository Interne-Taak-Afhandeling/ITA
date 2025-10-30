using InterneTaakAfhandeling.EndToEndTest.Infrastructure;
using InterneTaakAfhandeling.EndToEndTest.Helpers;
using Microsoft.Playwright;
using ITA.InterneTaakAfhandeling.EndToEndTest.Helpers;

namespace ITA.InterneTaakAfhandeling.EndToEndTest.SharedCode
{

    [TestClass]
    public class AnonymousContactVerzoekScenarios : ITAPlaywrightTest
    {
        [TestMethod(" Contactverzoek creation and search vin ITA Website")]
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

            await Step("And user selects medewerker from dropdown");
            await Page.GetMedewerkerRadioButton().ClickAsync();

            await Step("And user fills medewerker combobox and selects 'ICATT Integratietest'");
            await Page.GetByText("Medewerker").First.ClickAsync();
            await Page.GetByRole(AriaRole.Combobox, new() { Name = "Medewerker *" }).ClickAsync();
            await Page.GetByRole(AriaRole.Combobox, new() { Name = "Medewerker *" }).FillAsync("icatt");
            await Page.GetByText("ICATT Integratietest").ClickAsync();

            await Step("And user selects afdeling from dropdown");
            await Page.GetByLabel("Afdeling / groep").SelectOptionAsync("[object Object]");

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

            // Register cleanup
            // RegisterCleanup(async () =>
            // {
            //     await TestCleanupHelper.CleanupPostKlantContacten(klantContactPostResponse);
            // });


            await Step("Navigate to ITA homepage and wait");
            await Page.GotoAsync("/");

            await Step("Wait on ITA homepage for 3 seconds");
            await Task.Delay(3000);
        }
    }
}
