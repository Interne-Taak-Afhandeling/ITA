using System.Text.RegularExpressions;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.EndToEndTest.Infrastructure;
using Microsoft.Playwright;

namespace InterneTaakAfhandeling.EndToEndTest.Dashboard
{
    [TestClass]
    public class ContactverzoekScenarios : ITAPlaywrightTest
    {
        [TestMethod("Detail validation of Contactverzoek detail page")]
        public async Task User_ClickContactverzoekToViewDetails_FromDashboard()
        {
            var testOnderwerp = "Test_Contact_from_ITA_E2E_test_without_ZAAK";

            await Step("Ensure test data exists via API");
            var contactverzoekWithoutZaak = await TestDataHelper.CreateContactverzoek(testOnderwerp, attachZaak: false);
            RegisterCleanup(async () =>
            await TestDataHelper.DeleteContactverzoekAsync(contactverzoekWithoutZaak.ToString()));

            await Step("Navigate to home page");
            await Page.GotoAsync("/");

            await Step("Wait for dashboard to load");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("Click on contactverzoek to view details");
            var onderwerpElement = Page.Locator($"text={testOnderwerp}");
            await Expect(onderwerpElement).ToBeVisibleAsync();

            var detailsLink = Page.GetDetailsLink(testOnderwerp);
            await detailsLink.ClickAsync();

            await Step("Verify details page loads");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("Verify contactverzoek details are accessible");
            var onderwerpInDetails = Page.Locator($"text={testOnderwerp}");
            await Expect(onderwerpInDetails).ToBeVisibleAsync();

            await Step("Verify contactverzoek field values");
            await Expect(Page.GetKlantnaamLabel()).ToBeVisibleAsync();
            await Expect(Page.GetKlantnaamValue()).ToHaveTextAsync("-");

            await Expect(Page.GetTelefoonnummerLabel()).ToBeVisibleAsync();
            await Expect(Page.GetTelefoonnummerValue()).ToHaveTextAsync("-");

            await Expect(Page.GetEmailLabel()).ToBeVisibleAsync();
            await Expect(Page.GetEmailValue()).ToHaveTextAsync("-");

            await Step("Verify action tabs are present");
            await Expect(Page.GetContactmomentRegistrerenTab()).ToBeVisibleAsync();
            await Expect(Page.GetDoorsturenTab()).ToBeVisibleAsync();
            await Expect(Page.GetAlleenToelichtingTab()).ToBeVisibleAsync();

            await Step("Verify Vraag field");
            await Expect(Page.GetVraagLabel()).ToBeVisibleAsync();
            await Expect(Page.GetVraagValue(testOnderwerp)).ToHaveTextAsync(testOnderwerp);

            await Step("Verify Informatie voor burger / medewerker field");
            await Expect(Page.GetInformatieVoorBurgerLabel()).ToBeVisibleAsync();
            await Expect(Page.GetInformatieVoorBurgerValue()).ToHaveTextAsync("This is a test contact request created during an end-to-end test run.");

            await Expect(Page.GetAangemaaktDoorLabel()).ToBeVisibleAsync();
            await Expect(Page.GetAangemaaktDoorValue()).ToBeVisibleAsync();

            await Expect(Page.GetBehandelaarLabel()).ToBeVisibleAsync();
            await Expect(Page.GetBehandelaarValue()).ToBeVisibleAsync();

            await Expect(Page.GetStatusLabel()).ToBeVisibleAsync();
            await Expect(Page.GetStatusValue()).ToBeVisibleAsync();

            await Expect(Page.GetKanaalLabel()).ToBeVisibleAsync();
            await Expect(Page.GetKanaalValue()).ToBeVisibleAsync();

            await Expect(Page.GetInterneToelichtingLabel()).ToBeVisibleAsync();
            await Expect(Page.GetInterneToelichtingValue()).ToHaveTextAsync("Test contactverzoek from ITA E2E test");
        }

        [TestMethod("Validation of details page of contactverzoek that has ZAAK connected to it")]
        public async Task User_ClickContactverzoekToViewDetailsWithZAAK_FromDashboard()
        {
            var testOnderwerp = "Test_Contact_from_ITA_E2E_test_with_ZAAK";

            await Step("Ensure test data exists via API");
            var contactverzoekWithZaak = await TestDataHelper.CreateContactverzoek(testOnderwerp, attachZaak: true);
            RegisterCleanup(async () =>
            await TestDataHelper.DeleteContactverzoekAsync(contactverzoekWithZaak.ToString()));

            await Step("Navigate to home page");
            await Page.GotoAsync("/");

            await Step("Wait for dashboard to load");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("Click on contactverzoek to view details");
            var onderwerpElement = Page.Locator($"text={testOnderwerp}");
            await Expect(onderwerpElement).ToBeVisibleAsync();

            var detailsLink = Page.GetDetailsLink(testOnderwerp);
            await detailsLink.ClickAsync();

            await Step("Verify details page loads");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("Verify contactverzoek details are accessible");
            var onderwerpInDetails = Page.Locator($"text={testOnderwerp}");
            await Expect(onderwerpInDetails).ToBeVisibleAsync();
            var zaakElement = Page.Locator($"text={"ZAAK-2023-002"}");
            await Expect(zaakElement).ToBeVisibleAsync();

            await Step("Verify contactverzoek field values");
            await Expect(Page.GetKlantnaamLabel()).ToBeVisibleAsync();
            await Expect(Page.GetKlantnaamValue()).ToHaveTextAsync("-");

            await Expect(Page.GetTelefoonnummerLabel()).ToBeVisibleAsync();
            await Expect(Page.GetTelefoonnummerValue()).ToHaveTextAsync("-");

            await Expect(Page.GetEmailLabel()).ToBeVisibleAsync();
            await Expect(Page.GetEmailValue()).ToHaveTextAsync("-");

            await Step("Verify action tabs are present");
            await Expect(Page.GetContactmomentRegistrerenTab()).ToBeVisibleAsync();
            await Expect(Page.GetDoorsturenTab()).ToBeVisibleAsync();
            await Expect(Page.GetAlleenToelichtingTab()).ToBeVisibleAsync();

            await Step("Verify Vraag field");
            await Expect(Page.GetVraagLabel()).ToBeVisibleAsync();
            await Expect(Page.GetVraagValue(testOnderwerp)).ToHaveTextAsync(testOnderwerp);

            await Step("Verify Informatie voor burger / medewerker field");
            await Expect(Page.GetInformatieVoorBurgerLabel()).ToBeVisibleAsync();
            await Expect(Page.GetInformatieVoorBurgerValue()).ToHaveTextAsync("This is a test contact request created during an end-to-end test run.");

            await Step("Verify additional metadata fields");
            await Expect(Page.GetAangemaaktDoorLabel()).ToBeVisibleAsync();
            await Expect(Page.GetAangemaaktDoorValue()).ToBeVisibleAsync();

            await Expect(Page.GetBehandelaarLabel()).ToBeVisibleAsync();
            await Expect(Page.GetBehandelaarValue()).ToBeVisibleAsync();

            await Expect(Page.GetStatusLabel()).ToBeVisibleAsync();
            await Expect(Page.GetStatusValue()).ToBeVisibleAsync();

            await Expect(Page.GetKanaalLabel()).ToBeVisibleAsync();
            await Expect(Page.GetKanaalValue()).ToBeVisibleAsync();

            await Expect(Page.GetInterneToelichtingLabel()).ToBeVisibleAsync();
            await Expect(Page.GetInterneToelichtingValue()).ToBeVisibleAsync();

        }

        [TestMethod("Validation of fields and registering Contactmoment, Contact opnemen gelukt")]
        public async Task User_RegisterContactmoment_ContactOpnemenGelukt()
        {
            var testOnderwerp = "Test_Contact_from_ITA_E2E_test_without_ZAAK";

            await Step("Ensure test data exists via API");
            var contactverzoekWithoutZaak = await TestDataHelper.CreateContactverzoek(testOnderwerp, attachZaak: false);
            RegisterCleanup(async () =>
            await TestDataHelper.DeleteContactverzoekAsync(contactverzoekWithoutZaak.ToString()));

            await Step("Navigate to home page");
            await Page.GotoAsync("/");

            await Step("Wait for dashboard to load");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("Click on contactverzoek to view details");
            var onderwerpElement = Page.Locator($"text={testOnderwerp}");
            await Expect(onderwerpElement).ToBeVisibleAsync();

            var detailsLink = Page.GetDetailsLink(testOnderwerp);
            await detailsLink.ClickAsync();

            await Step("Verify details page loads");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("Navigate to Contactmoment Registreren tab");
            var contactmomentTab = Page.GetContactmomentRegistrerenTab();
            await contactmomentTab.ClickAsync();

            await Step("under Resultaat the option  Contact opnemen gelukt is selected");
            await Expect(Page.GetByText("Contact opnemen gelukt")).ToBeCheckedAsync();

            await Step("user clicks on 'Contactmoment opslaan");
            await Page.GetContactmomentOpslaanButton().ClickAsync();

            await Step("error message as Please select one of these options appears for question Wil je het contactmoment afsluiten?");
            var radioButton = Page.Locator("input[type='radio'][name*='afsluiten']").First;
            await Expect(radioButton).ToHaveJSPropertyAsync("validationMessage", "Please select one of these options.");

            await Step("when user selects Nee for Wil je het contactmoment afsluiten?");
            await Page.GetByLabel("Nee").ClickAsync();

            await Step("user clicks on 'Contactmoment opslaan");
            await Page.GetContactmomentOpslaanButton().ClickAsync();

            await Step("error message as Please select an item in the list or Selecteer een item in de lijst. is displayed for the field Kanaal");
            var kanaalSelect = Page.Locator("#kanalen");
            await Expect(kanaalSelect).ToHaveJSPropertyAsync("validationMessage", "Please select an item in the list.");

            await Step("when User selects a channel from the list in field Kanaal  ");
            await kanaalSelect.SelectOptionAsync(new[] { "Telefoon" });

            await Step("clicks on 'Contactmoment opslaan'");
            await Page.GetContactmomentOpslaanButton().ClickAsync();

            await Step("error message as Please fill in this field or Vul dit veld in is displayed for the field Informatie voor burger/bedrijf");
            var informatieField = Page.Locator("#informatie-burger");
            await Expect(informatieField).ToHaveJSPropertyAsync("validationMessage", "Please fill in this field.");

            await Step("when user enters some text in field Informatie voor burger/bedrijf");
            await informatieField.FillAsync("Test information for contactmoment");

            await Step("clicks on 'Contactmoment opslaan'");
            await Page.GetContactmomentOpslaanButton().ClickAsync();

            await Step("a Success message is displayed");
            await Expect(Page.GetByText("Contactmoment succesvol bijgewerkt")).ToBeVisibleAsync();

            await Step("wait for data to be processed");
            await Page.WaitForTimeoutAsync(2000); // Wait 2 seconds

            await Step("refresh page to ensure Logboek is fully loaded");
            await Page.ReloadAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("verify Contact gelukt is displayed in Logboek");
            await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Logboek contactverzoek" })).ToBeVisibleAsync();
            await Expect(Page.GetByText("Contact gelukt")).ToBeVisibleAsync();

            await Step("verify the entered information is visible in Logboek");
            await Expect(Page.GetByRole(AriaRole.Paragraph).Filter(new() { HasText = "Test information for contactmoment" })).ToBeVisibleAsync();
        }

        [TestMethod("Validation of fields and registering Contactmoment, Contact opnemen niet gelukt")]
        public async Task User_RegisterContactmoment_ContactOpnemenNietGelukt()
        {
            var testOnderwerp = "Test_Contact_from_ITA_E2E_test_without_ZAAK";

            await Step("Ensure test data exists via API");
            var contactverzoekWithoutZaak = await TestDataHelper.CreateContactverzoek(testOnderwerp, attachZaak: false);
            RegisterCleanup(async () =>
            await TestDataHelper.DeleteContactverzoekAsync(contactverzoekWithoutZaak.ToString()));

            await Step("Navigate to home page");
            await Page.GotoAsync("/");

            await Step("Wait for dashboard to load");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("Click on contactverzoek to view details");
            var onderwerpElement = Page.Locator($"text={testOnderwerp}");
            await Expect(onderwerpElement).ToBeVisibleAsync();

            var detailsLink = Page.GetDetailsLink(testOnderwerp);
            await detailsLink.ClickAsync();

            await Step("Verify details page loads");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("Navigate to Contactmoment Registreren tab");
            var contactmomentTab = Page.GetContactmomentRegistrerenTab();
            await contactmomentTab.ClickAsync();

            await Step("under Resultaat the option  Contact opnemen gelukt is selected");
            await Expect(Page.GetByText("Contact opneme gelukt")).ToBeCheckedAsync();

            await Step("user selects “Contact opnemen niet gelukt” under “Resultaat");
            await Page.GetByText("Contact opnemen niet gelukt").ClickAsync();

            await Step("user clicks on 'Contactmoment opslaan");
            await Page.GetContactmomentOpslaanButton().ClickAsync();

            await Step("error message as Please select one of these options appears for question Wil je het contactmoment afsluiten?");
            var radioButton = Page.Locator("input[type='radio'][name*='afsluiten']").First;
            await Expect(radioButton).ToHaveJSPropertyAsync("validationMessage", "Please select one of these options.");

            await Step("when user selects Nee for Wil je het contactmoment afsluiten?");
            await Page.GetByLabel("Nee").ClickAsync();

            await Step("user clicks on 'Contactmoment opslaan");
            await Page.GetContactmomentOpslaanButton().ClickAsync();

            await Step("error message as Please select an item in the list or Selecteer een item in de lijst. is displayed for the field Kanaal");
            var kanaalSelect = Page.Locator("#kanalen");
            await Expect(kanaalSelect).ToHaveJSPropertyAsync("validationMessage", "Please select an item in the list.");

            await Step("when User selects a channel from the list in field Kanaal  ");
            await kanaalSelect.SelectOptionAsync(new[] { "Telefoon" });

            await Step("verify that Informatie voor burger/bedrijf is NOT mandatory for niet gelukt");
            var informatieField = Page.Locator("#informatie-burger");
            var isRequired = await informatieField.GetAttributeAsync("required");
            Assert.IsNull(isRequired, "Informatie field should not be required for 'Contact opnemen niet gelukt'");

            await Step("user can save without filling Informatie voor burger/bedrijf");
            await Page.GetContactmomentOpslaanButton().ClickAsync();

            await Step("a Success message is displayed");
            await Expect(Page.GetByText("Contactmoment succesvol bijgewerkt")).ToBeVisibleAsync();

            await Step("wait for data to be processed");
            await Page.WaitForTimeoutAsync(2000);

            await Step("refresh page to ensure Logboek is fully loaded");
            await Page.ReloadAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("verify Contact niet gelukt is displayed in Logboek");
            await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Logboek contactverzoek" })).ToBeVisibleAsync();
            await Expect(Page.GetByText("Contact niet gelukt")).ToBeVisibleAsync();
        }

        [TestMethod("Validation of contactverzoek with BSN connected partij")]
        public async Task User_ViewContactverzoek_WithBSNPartij()
        {
            var testOnderwerp = "Test_Contact_with_BSN_Partij";

            await Step("Ensure test data exists via API with BSN partij");
            await TestDataHelper.CreateContactverzoekWithAfdelingMedewerkerAndPartij(
        onderwerp: testOnderwerp,
        bsn: "999992223",
        attachZaak: false
    );

            await Step("Wait for data to be fully propagated");
            await Page.WaitForTimeoutAsync(3000);

            await Step("Navigate to home page");
            await Page.GotoAsync("/");

            await Step("Wait for dashboard to load");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("Click on contactverzoek to view details");
            var onderwerpElement = Page.Locator($"text={testOnderwerp}");
            await Expect(onderwerpElement).ToBeVisibleAsync();

            var detailsLink = Page.GetDetailsLink(testOnderwerp);
            await detailsLink.ClickAsync();

            await Step("Verify details page loads");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("Check klant field displays klantnaam");

            var klantnaamCount = await Page.GetByText("Klantnaam").CountAsync();
            Assert.IsTrue(klantnaamCount > 0, "Klantnaam should be present on the page.");

            await Step("Verify contactverzoek details are accessible");
            var onderwerpInDetails = Page.Locator($"text={testOnderwerp}");
            await Expect(onderwerpInDetails).ToBeVisibleAsync();

            await Expect(Page.GetKlantnaamLabel()).ToBeVisibleAsync();
        }
    }
}
