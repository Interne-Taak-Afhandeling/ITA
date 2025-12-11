using System.Text.RegularExpressions;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.EndToEndTest.Infrastructure;
using ITA.InterneTaakAfhandeling.EndToEndTest.Helpers;
using Microsoft.Playwright;

namespace InterneTaakAfhandeling.EndToEndTest.Dashboard
{
    [TestClass]
    [DoNotParallelize]
    public class ContactverzoekScenarios : ITAPlaywrightTest
    {
        // Test Methods

        [TestMethod("Detail validation of Contactverzoek detail page")]
        public async Task User_ClickContactverzoekToViewDetails_FromDashboard()
        {
            var testOnderwerp = "Test_Contact_from_ITA_E2E_test_without_ZAAK";
            await SetupContactverzoek(testOnderwerp, attachZaak: false);

            await NavigateToContactverzoekDetails(testOnderwerp);
            await VerifyBasicContactverzoekFields(testOnderwerp);
            await VerifyActionTabsArePresent();
            await VerifyMetadataFields();
        }

        [TestMethod("Validation of details page of contactverzoek that has ZAAK connected to it")]
        public async Task User_ClickContactverzoekToViewDetailsWithZAAK_FromDashboard()
        {
            var testOnderwerp = "Test_Contact_from_ITA_E2E_test_with_ZAAK";
            await SetupContactverzoek(testOnderwerp, attachZaak: true);

            await NavigateToContactverzoekDetails(testOnderwerp);
            await VerifyBasicContactverzoekFields(testOnderwerp);
            await VerifyZaakIsVisible(TestDataConstants.Zaken.TestZaakIdentificatie);
            await VerifyActionTabsArePresent();
            await VerifyMetadataFields();
        }

        [TestMethod("Validation of fields and registering Contactmoment, Contact opnemen gelukt")]
        public async Task User_RegisterContactmoment_ContactOpnemenGelukt()
        {
            var testOnderwerp = "Test_Contact_opnemen_gelukt";
            await SetupContactverzoek(testOnderwerp, attachZaak: false);

            await NavigateToContactverzoekDetails(testOnderwerp);
            await NavigateToContactmomentRegistrerenTab();

            await Step("Verify 'Contact opnemen gelukt' is selected by default");
           await Expect(Page.GetByRole(AriaRole.Radio, new() { Name = "Contact opnemen gelukt" })).ToBeCheckedAsync();
          await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await VerifyValidationErrors_ContactGelukt();
            await FillContactmomentForm_ContactGelukt();
            await VerifyContactmomentSavedSuccessfully("Contact gelukt", "Test information for contactmoment");
        }

        [TestMethod("Validation of fields and registering Contactmoment, Contact opnemen niet gelukt")]
        public async Task User_RegisterContactmoment_ContactOpnemenNietGelukt()
        {
            var testOnderwerp = "Test_Contact_from_ITA_E2E_test_without_ZAAK";
            await SetupContactverzoek(testOnderwerp, attachZaak: false);

            await NavigateToContactverzoekDetails(testOnderwerp);
            await NavigateToContactmomentRegistrerenTab();
            
            await Step("Select 'Contact opnemen niet gelukt'");
            await Page.GetByRole(AriaRole.Radio, new() { Name = "Contact opnemen niet gelukt" }).ClickAsync();
       
            await VerifyValidationErrors_ContactNietGelukt();
            await FillContactmomentForm_ContactNietGelukt();
            await VerifyContactmomentSavedSuccessfully("Contact niet gelukt");
        }

        [TestMethod("Validation of contactverzoek with BSN connected partij")]
        public async Task User_ViewContactverzoek_WithBSNPartij()
        {
            var testOnderwerp = "Test_Contact_with_BSN_Partij";

            await Step("Setup contactverzoek with BSN partij");
            var uuid = await TestDataHelper.CreateContactverzoekWithAfdelingMedewerkerAndPartij(
                onderwerp: testOnderwerp,
                bsn: TestDataConstants.Partijen.TestBsn,
                attachZaak: false
            );

           RegisterCleanup(async () => await TestDataHelper.DeleteContactverzoekAsync(uuid.ToString()));

           await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await NavigateToContactverzoekDetails(testOnderwerp);

            await Step("Verify klantnaam has a value");
            var klantnaamValue = await Page.GetKlantnaamValue().InnerTextAsync();
            Assert.IsFalse(string.IsNullOrWhiteSpace(klantnaamValue) || klantnaamValue == "-", 
                "Klantnaam should have a value");

            await Step("Verify basic fields are visible");
            await Expect(Page.Locator($"text={testOnderwerp}")).ToBeVisibleAsync();
            await Expect(Page.GetKlantnaamLabel()).ToBeVisibleAsync();
        }

//  Setup & Navigation Helpers

        private async Task<Guid> SetupContactverzoek(string onderwerp, bool attachZaak)
        {
            await Step("Setup test data via API");
            var uuid = await TestDataHelper.CreateContactverzoek(onderwerp, attachZaak);
            RegisterCleanup(async () => await TestDataHelper.DeleteContactverzoekAsync(uuid.ToString()));
            return uuid;
        }

        private async Task NavigateToContactverzoekDetails(string onderwerp)
        {
            await Step("Navigate to home page");
            await Page.GotoAsync("/");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step($"Click on contactverzoek '{onderwerp}'");
      
             var detailsLink = Page.GetDetailsLink(onderwerp);
             await detailsLink.WaitForAsync(new() { State = WaitForSelectorState.Visible });
             await detailsLink.ClickAsync();
        }

        private async Task NavigateToContactmomentRegistrerenTab()
        {
            await Step("Navigate to Contactmoment Registreren tab");
            await Page.GetContactmomentRegistrerenTab().ClickAsync();
        }

    // Verification Helpers

        private async Task VerifyBasicContactverzoekFields(string onderwerp)
        {
            await Step("Verify contactverzoek is visible");
            await Expect(Page.Locator($"text={onderwerp}")).ToBeVisibleAsync();

            await Step("Verify contact information fields");
            await Expect(Page.GetKlantnaamLabel()).ToBeVisibleAsync();
            await Expect(Page.GetKlantnaamValue()).ToHaveTextAsync("-");

            await Expect(Page.GetTelefoonnummerLabel()).ToBeVisibleAsync();
            await Expect(Page.GetTelefoonnummerValue()).ToHaveTextAsync("-");

            await Expect(Page.GetEmailLabel()).ToBeVisibleAsync();
            await Expect(Page.GetEmailValue()).ToHaveTextAsync("-");

            await Step("Verify question and information fields");
            await Expect(Page.GetVraagLabel()).ToBeVisibleAsync();
            await Expect(Page.GetVraagValue(onderwerp)).ToHaveTextAsync(onderwerp);

            await Expect(Page.GetInformatieVoorBurgerLabel()).ToBeVisibleAsync();
             await Page.GetInformatieVoorBurgerValue().WaitForAsync(new() { State = WaitForSelectorState.Visible });
   
            await Expect(Page.GetInformatieVoorBurgerValue()).ToHaveTextAsync(
                "This is a test contact request created during an end-to-end test run.");
        }

        private async Task VerifyZaakIsVisible(string zaakIdentificatie)
        {
            await Step($"Verify ZAAK '{zaakIdentificatie}' is visible");
            await Expect(Page.Locator($"text={zaakIdentificatie}")).ToBeVisibleAsync();
        }

        private async Task VerifyActionTabsArePresent()
        {
            await Step("Verify action tabs are present");
            await Expect(Page.GetContactmomentRegistrerenTab()).ToBeVisibleAsync();
            await Expect(Page.GetDoorsturenTab()).ToBeVisibleAsync();
            await Expect(Page.GetAlleenToelichtingTab()).ToBeVisibleAsync();
        }

        private async Task VerifyMetadataFields()
        {
            await Step("Verify metadata fields");
            await Expect(Page.GetAangemaaktDoorLabel()).ToBeVisibleAsync();
            await Expect(Page.GetAangemaaktDoorValue()).ToBeVisibleAsync();

            await Expect(Page.GetBehandelaarLabel()).ToBeVisibleAsync();
            await Expect(Page.GetBehandelaarValue()).ToBeVisibleAsync();

            await Expect(Page.GetStatusLabel()).ToBeVisibleAsync();
            await Expect(Page.GetStatusValue()).ToBeVisibleAsync();

            await Expect(Page.GetKanaalLabel()).ToBeVisibleAsync();
            await Expect(Page.GetKanaalValue()).ToBeVisibleAsync();

            await Expect(Page.GetInterneToelichtingLabel()).ToBeVisibleAsync();
            var toelichtingValue = Page.GetInterneToelichtingValue();
            if (await toelichtingValue.CountAsync() > 0)
            {
                await Expect(toelichtingValue).ToBeVisibleAsync();
            }
        }

    // Form Validation & Filling Helpers

        private async Task VerifyValidationErrors_ContactGelukt()
        {
            await Step("Verify validation: Afsluiten question is required");
            await Page.GetContactmomentOpslaanButton().ClickAsync();

            var radioButton = Page.Locator("input[type='radio'][name*='afsluiten']").First;
            await Expect(radioButton).ToHaveJSPropertyAsync("validity.valueMissing", true);

            await Step("Select 'Nee' for afsluiten");
            await Page.GetByLabel("Nee").ClickAsync();

            await Step("Verify validation: Kanaal is required");
            await Page.GetContactmomentOpslaanButton().ClickAsync();
            var kanaalSelect = Page.Locator("#kanalen");
            await Expect(kanaalSelect).ToHaveJSPropertyAsync("validity.valueMissing", true);
            await Step("Select Kanaal");
            await kanaalSelect.SelectOptionAsync(new[] { "Telefoon" });

            await Step("Verify validation: Informatie field is required for 'Contact gelukt'");
            await Page.GetContactmomentOpslaanButton().ClickAsync();
            var informatieField = Page.Locator("#informatie-burger");
           await Expect(informatieField).ToHaveJSPropertyAsync("validity.valueMissing", true);
        }

        private async Task VerifyValidationErrors_ContactNietGelukt()
        {
            await Step("Verify validation: Afsluiten question is required");
            await Page.GetContactmomentOpslaanButton().ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            var radioButton = Page.Locator("input[type='radio'][name*='afsluiten']").First;
           await Expect(radioButton).ToHaveJSPropertyAsync("validationMessage", "Please select one of these options.");

            await Step("Select 'Nee' for afsluiten");
            await Page.GetByLabel("Nee").ClickAsync();
            
            await Step("Verify validation: Kanaal is required");
            await Page.GetContactmomentOpslaanButton().ClickAsync();
            var kanaalSelect = Page.Locator("#kanalen");
            await Expect(kanaalSelect).ToHaveJSPropertyAsync("validity.valueMissing", true);

            await Step("Select Kanaal");
            await kanaalSelect.SelectOptionAsync(new[] { "Telefoon" });

            await Step("Verify Informatie field is NOT required for 'Contact niet gelukt'");
            var informatieField = Page.Locator("#informatie-burger");
            var isRequired = await informatieField.GetAttributeAsync("required");
            Assert.IsNull(isRequired, "Informatie field should not be required for 'Contact opnemen niet gelukt'");
        }

        private async Task FillContactmomentForm_ContactGelukt()
        {
            await Step("Fill form for 'Contact gelukt'");
            var informatieField = Page.Locator("#informatie-burger");
            await informatieField.FillAsync("Test information for contactmoment");
            await Page.GetContactmomentOpslaanButton().ClickAsync();
        }

        private async Task FillContactmomentForm_ContactNietGelukt()
        {
            await Step("Save form for 'Contact niet gelukt' (no informatie required)");
            await Page.GetContactmomentOpslaanButton().ClickAsync();
        }

        private async Task VerifyContactmomentSavedSuccessfully(string logboekText, string? expectedInformatieText = null)
        {
            await Step("Verify success message");
            await Expect(Page.GetByText("Contactmoment succesvol bijgewerkt")).ToBeVisibleAsync();

            await Step("Wait and refresh to load Logboek");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step($"Verify '{logboekText}' is displayed in Logboek");
            await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Logboek contactverzoek" })).ToBeVisibleAsync();
            await Expect(Page.GetByText(logboekText)).ToBeVisibleAsync();

            if (expectedInformatieText != null)
            {
                await Step("Verify entered information is visible in Logboek");
                await Expect(Page.GetByRole(AriaRole.Paragraph)
                    .Filter(new() { HasText = expectedInformatieText })).ToBeVisibleAsync();
            }
        }

    }
}