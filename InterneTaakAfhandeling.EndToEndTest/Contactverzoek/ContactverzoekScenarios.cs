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
           await Expect(Page.GetContactOpnemenGeluktRadio()).ToBeCheckedAsync();
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
            await Page.GetContactOpnemenNietGeluktRadio().ClickAsync();
       
            await VerifyValidationErrors_ContactNietGelukt();
            await FillContactmomentForm_ContactNietGelukt();
            await VerifyContactmomentSavedSuccessfully("Contact niet gelukt");
        }

        [TestMethod("Validation that Informatie is mandatory when closing contactverzoek, even for Contact niet gelukt")]
        public async Task User_CloseContactverzoek_InformatieRequiredForAllResults()
        {
            var testOnderwerp = "Test_Close_Requires_Informatie";
            await SetupContactverzoek(testOnderwerp, attachZaak: false);

            await NavigateToContactverzoekDetails(testOnderwerp);
            await NavigateToContactmomentRegistrerenTab();

            await Step("Select 'Contact opnemen niet gelukt'");
            await Page.GetByRole(AriaRole.Radio, new() { Name = "Contact opnemen niet gelukt" }).ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("Select Kanaal");
            var kanaalSelect = Page.Locator("#kanalen");
            await kanaalSelect.SelectOptionAsync(new[] { "Telefoon" });

            await Step("Select 'Ja' for afsluiten (to close the contactverzoek)");
            await Page.GetByLabel("Ja").ClickAsync();

            await Step("Try to save without informatie - should show validation error");
            await Page.GetContactmomentOpslaanButton().ClickAsync();

            await Step("Verify validation error on Informatie field");
            var informatieField = Page.Locator("#informatie-burger");
            await Expect(informatieField).ToHaveJSPropertyAsync("validity.valueMissing", true);

            await Step("Fill in Informatie field");
            await informatieField.FillAsync("Required information when closing");

            await Step("Submit form - should now trigger confirmation modal");
            await Page.GetContactmomentOpslaanButton().ClickAsync();

            await Step("Verify confirmation dialog is displayed");
            await Expect(Page.GetByRole(AriaRole.Dialog)).ToBeVisibleAsync();

            await Step("Confirm closure");
            await Page.GetOpslaanEnAfrondenButton().ClickAsync();

            await Step("Verify contactverzoek was closed successfully");
            await Expect(Page.GetByText("Contactmoment succesvol opgeslagen en afgerond")).ToBeVisibleAsync();
        }

        [TestMethod("Validation of Opslaan en afsluiten with confirmation and status verification")]
        public async Task User_SaveAndCloseContactverzoek_VerifiesStatusVerwerktAndAfgehandeldOp()
        {
            var testOnderwerp = "Test_Opslaan_en_afsluiten";
            var contactmomentUuid = await SetupContactverzoek(testOnderwerp, attachZaak: false);

            await NavigateToContactverzoekDetails(testOnderwerp);
            await NavigateToContactmomentRegistrerenTab();

            await Step("Verify 'Contact opnemen gelukt' is selected by default");
            await Expect(Page.GetByRole(AriaRole.Radio, new() { Name = "Contact opnemen gelukt" })).ToBeCheckedAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("Fill form fields");
            var kanaalSelect = Page.Locator("#kanalen");
            await kanaalSelect.SelectOptionAsync(new[] { "Telefoon" });
            
            var informatieField = Page.Locator("#informatie-burger");
            await informatieField.FillAsync("Test information for save and close");

            await Step("Select 'Ja' for afsluiten question");
            await Page.GetByLabel("Ja").ClickAsync();

            await Step("Click 'Contactmoment opslaan' button to trigger confirmation");
            await Page.GetContactmomentOpslaanButton().ClickAsync();

            await Step("Verify confirmation dialog is displayed");
            await Expect(Page.GetByRole(AriaRole.Dialog)).ToBeVisibleAsync();

            await Step("Click 'Opslaan & afronden' on confirmation dialog");
            await Page.GetOpslaanEnAfrondenButton().ClickAsync();

            await Step("Verify request is closed - success message");
            await Expect(Page.GetByText("Contactmoment succesvol opgeslagen en afgerond")).ToBeVisibleAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("Get internetaak UUID and verify status in OpenKlant");
            var internetaakUuid = await TestDataHelper.GetInternetaakUuidFromContactmomentAsync(contactmomentUuid);
            Assert.IsNotNull(internetaakUuid, "Internetaak UUID should be found");

            await VerifyInternetaakStatusInOpenKlant(internetaakUuid.Value, KnownInternetaakStatussen.Verwerkt, shouldHaveAfgehandeldOp: true);
        }

        [TestMethod("Validation of canceling Opslaan en afsluiten - request not closed")]
        public async Task User_CancelSaveAndCloseContactverzoek_RemainsOnDetailPage()
        {
            var testOnderwerp = "Test_Cancel_Opslaan_en_afsluiten";
            var contactmomentUuid = await SetupContactverzoek(testOnderwerp, attachZaak: false);

            await NavigateToContactverzoekDetails(testOnderwerp);
            await NavigateToContactmomentRegistrerenTab();

            await Step("Verify 'Contact opnemen gelukt' is selected by default");
            await Expect(Page.GetByRole(AriaRole.Radio, new() { Name = "Contact opnemen gelukt" })).ToBeCheckedAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("Fill form fields");
            var kanaalSelect = Page.Locator("#kanalen");
            await kanaalSelect.SelectOptionAsync(new[] { "Telefoon" });
            
            var informatieField = Page.Locator("#informatie-burger");
            await informatieField.FillAsync("Test information for cancel scenario");

            await Step("Select 'Ja' for afsluiten question");
            await Page.GetByLabel("Ja").ClickAsync();

            await Step("Click 'Contactmoment opslaan' button to trigger confirmation");
            await Page.GetContactmomentOpslaanButton().ClickAsync();

            await Step("Verify confirmation dialog is displayed");
            await Expect(Page.GetByRole(AriaRole.Dialog)).ToBeVisibleAsync();

            await Step("Click 'Annuleren' on confirmation dialog");
            await Page.GetAnnulerenDialogButton().ClickAsync();

            await Step("Verify dialog is closed");
            await Expect(Page.GetByRole(AriaRole.Dialog)).Not.ToBeVisibleAsync();

            await Step("Verify request is NOT closed - no success message shown");
            await Expect(Page.GetByText("Contactmoment succesvol opgeslagen en afgerond")).Not.ToBeVisibleAsync();

            await Step("Verify user remains on contactverzoek detail page");
            await Expect(Page.GetContactmomentRegistrerenTab()).ToBeVisibleAsync();
            await Expect(Page.Locator($"text={testOnderwerp}")).ToBeVisibleAsync();

            await Step("Verify status in OpenKlant remains 'te_verwerken' (not closed)");
            var internetaakUuid = await TestDataHelper.GetInternetaakUuidFromContactmomentAsync(contactmomentUuid);
            Assert.IsNotNull(internetaakUuid, "Internetaak UUID should be found");
            
            var internetaak = await TestDataHelper.GetInternetaakByIdAsync(internetaakUuid.Value);
            Assert.AreEqual("te_verwerken", internetaak.Status, "Status should remain 'te_verwerken' when canceled");
            Assert.IsNull(internetaak.AfgehandeldOp, "AfgehandeldOp should NOT be set when canceled");
        }

        [TestMethod("Validation that closed contactverzoek can still be updated")]
        public async Task User_UpdateClosedContactverzoek_CanStillAddContactmoment()
        {
            var testOnderwerp = $"Test_Update_Closed_{Guid.NewGuid().ToString().Substring(0, 8)}";
            var contactmomentUuid = await SetupContactverzoek(testOnderwerp, attachZaak: false);

            await NavigateToContactverzoekDetails(testOnderwerp);
            await NavigateToContactmomentRegistrerenTab();

            await Step("Fill form and close the contactverzoek");
            var kanaalSelect = Page.Locator("#kanalen");
            await kanaalSelect.SelectOptionAsync(new[] { "Telefoon" });
            
            var informatieField = Page.Locator("#informatie-burger");
            await informatieField.FillAsync("Initial contactmoment before closing");

            await Step("Select 'Ja' for afsluiten to close the contactverzoek");
            await Page.GetByLabel("Ja").ClickAsync();
            
            await Step("Click 'Contactmoment opslaan' button to trigger confirmation");
            await Page.GetContactmomentOpslaanButton().ClickAsync();

            await Step("Verify confirmation dialog is displayed");
            await Expect(Page.GetByRole(AriaRole.Dialog)).ToBeVisibleAsync();

            await Step("Click 'Opslaan & afronden' on confirmation dialog");
            await Page.GetOpslaanEnAfrondenButton().ClickAsync();

            await Step("Wait for close confirmation");
            await Expect(Page.GetByText("Contactmoment succesvol opgeslagen en afgerond")).ToBeVisibleAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("Get internetaak UUID and nummer for navigation");
            var internetaakUuid = await TestDataHelper.GetInternetaakUuidFromContactmomentAsync(contactmomentUuid);
            Assert.IsNotNull(internetaakUuid, "Internetaak UUID should be found");
            
            var internetaak = await TestDataHelper.GetInternetaakByIdAsync(internetaakUuid.Value);
            Assert.AreEqual(KnownInternetaakStatussen.Verwerkt, internetaak.Status, "Status should be verwerkt after closing");
            var internetaakNummer = internetaak.Nummer;

            await Step($"Navigate back to closed contactverzoek using nummer: {internetaakNummer}");
            await NavigateToContactverzoekByNummer(internetaakNummer!);

            await Step("Verify contactverzoek details page is displayed");
            await Expect(Page.Locator($"text={testOnderwerp}")).ToBeVisibleAsync();
            await Expect(Page.GetContactmomentRegistrerenTab()).ToBeVisibleAsync();

            await Step("Navigate to Contactmoment Registreren tab to add another contactmoment");
            await NavigateToContactmomentRegistrerenTab();

            await Step("Verify 'Contact opnemen gelukt' is selected by default");
            await Expect(Page.GetContactOpnemenGeluktRadio()).ToBeCheckedAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("Fill and save a new contactmoment on the closed contactverzoek");
            await Page.Locator("#kanalen").SelectOptionAsync(new[] { "E-mail" });
            await Page.Locator("#informatie-burger").FillAsync("Follow-up contactmoment on closed request");
            
            await Step("Select 'Nee' for afsluiten (don't close it again)");
            await Page.GetByLabel("Nee").ClickAsync();
            await Page.GetContactmomentOpslaanButton().ClickAsync();

            await Step("Verify new contactmoment was saved successfully");
            await Expect(Page.GetByText("Contactmoment succesvol bijgewerkt")).ToBeVisibleAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("Verify the new contactmoment appears in Logboek");
            await Expect(Page.GetByText("Follow-up contactmoment on closed request")).ToBeVisibleAsync();
        }

        [TestMethod("Validation that closed contactverzoek assigned to user appears in Mijn historie")]
        public async Task User_ViewClosedContactverzoekInMijnHistorie()
        {
            var testOnderwerp = $"Test_MijnHistorie_{Guid.NewGuid().ToString().Substring(0, 8)}";
            var contactmomentUuid = await SetupContactverzoek(testOnderwerp, attachZaak: false);

            await Step("Navigate to contactverzoek and assign to current user");
            await NavigateToContactverzoekDetails(testOnderwerp);
            await Page.GetToewijzenAanMezelfButton().ClickAsync();
            await Page.GetToewijzenAanMezelfDialogButton().ClickAsync();
            await Expect(Page.GetContactverzoekToegwezenMessage()).ToBeVisibleAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("Close the contactverzoek");
            await NavigateToContactmomentRegistrerenTab();
            var kanaalSelect = Page.Locator("#kanalen");
            await kanaalSelect.SelectOptionAsync(new[] { "Telefoon" });
            var informatieField = Page.Locator("#informatie-burger");
            await informatieField.FillAsync("Closing contactverzoek for Mijn historie test");
            await Page.GetByLabel("Ja").ClickAsync();
            await Page.GetContactmomentOpslaanButton().ClickAsync();
            await Expect(Page.GetByRole(AriaRole.Dialog)).ToBeVisibleAsync();
            await Page.GetOpslaanEnAfrondenButton().ClickAsync();
            await Expect(Page.GetByText("Contactmoment succesvol opgeslagen en afgerond")).ToBeVisibleAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("Navigate to Mijn historie page");
            await Page.GotoAsync("/historie");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("Verify page title is 'Mijn historie'");
            await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Mijn historie", Level = 1 })).ToBeVisibleAsync();

            await Step("Verify section heading is 'Mijn afgeronde contactverzoeken'");
            await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Mijn afgeronde contactverzoeken", Level = 2 })).ToBeVisibleAsync();

            await Step("Verify closed contactverzoek is displayed in the table");
            await Expect(Page.Locator($"text={testOnderwerp}")).ToBeVisibleAsync();
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

        [TestMethod("Finding a Zaak")]
        public async Task Finding_Zaak()
        {
            var testOnderwerp = "Test_Contact_from_ITA_E2E_test_without_ZAAK";
            await SetupContactverzoek(testOnderwerp, attachZaak: false);

            await NavigateToContactverzoekDetails(testOnderwerp);

            await Step("Click on the Pen-icon next to label 'Gekoppelde zaak'");
            await Page.GetGekoppeldeZaakKoppelenButton().ClickAsync();
            
            await Step("Verify zaak search pop-up box is displayed");
            await Expect(Page.GetKoppelAanZaakHeading()).ToBeVisibleAsync();
            await Expect(Page.GetKoppelAanZaakDescriptionText()).ToBeVisibleAsync();
            
            await Step("Verify user sees an input field Zaaknummer to fill in a zaak-number");
            await Expect(Page.GetZaaknummerTextbox()).ToBeVisibleAsync();
        }

        [TestMethod("Error message for invalid Zaak")]
        public async Task Error_Message_For_Invalid_Zaak()
        {
            var testOnderwerp = "Test_Contact_Invalid_Zaak";
            await SetupContactverzoek(testOnderwerp, attachZaak: false);

            await NavigateToContactverzoekDetails(testOnderwerp);

            await Step("Click on the Pen-icon next to label 'Gekoppelde zaak'");
            await Page.GetGekoppeldeZaakKoppelenButton().ClickAsync();
            
            await Step("Enter invalid Zaaknummer");
            await Page.GetZaaknummerTextbox().FillAsync("ZAAK 2003-008");
            
            await Step("Click on button 'Koppelen'");
            await Page.GetKoppelenButton().ClickAsync();
            
            await Step("Verify error message is displayed");
            await Expect(Page.GetGeenZaakGevondenMessage("ZAAK 2003-008")).ToBeVisibleAsync();
            
            await Step("Verify the popup remains visible");
            await Expect(Page.GetKoppelAanZaakHeading()).ToBeVisibleAsync();
        }

        [TestMethod("Connect valid Zaak and verify it is visible")]
        public async Task Connect_Valid_Zaak_And_Verify_Visibility()
        {
            var testOnderwerp = "Test_Contact_Connect_Valid_Zaak";
            await SetupContactverzoek(testOnderwerp, attachZaak: false);

            await NavigateToContactverzoekDetails(testOnderwerp);

            await Step("Click on the Pen-icon next to label 'Gekoppelde zaak'");
            await Page.GetGekoppeldeZaakKoppelenButton().ClickAsync();
            
            await Step("Enter valid Zaaknummer");
            await Page.GetZaaknummerTextbox().FillAsync("ZAAK-2023-009");
            
            await Step("Click on button 'Koppelen'");
            await Page.GetKoppelenButton().ClickAsync();
            
            await Step("Wait for the zaak to be connected");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            
            await VerifyZaakIsVisible("ZAAK-2023-009");
        }

        [TestMethod("Only one Zaak can be connected at once - replacing existing Zaak")]
        public async Task Only_One_Zaak_Can_Be_Connected_Replace_Existing()
        {
            var testOnderwerp = "Test_Contact_Replace_Zaak";
            await SetupContactverzoek(testOnderwerp, attachZaak: false);

            await NavigateToContactverzoekDetails(testOnderwerp);

            await Step("Click on the Pen-icon next to label 'Gekoppelde zaak'");
            await Page.GetGekoppeldeZaakKoppelenButton().ClickAsync();
            
            await Step("Enter first Zaaknummer ZAAK-2023-005");
            await Page.GetZaaknummerTextbox().FillAsync("ZAAK-2023-005");
            await Page.GetZaaknummerTextbox().PressAsync("Enter");
            
            await Step("Verify success message for first zaak");
            await Expect(Page.GetZaakSuccesvolGekoppeldMessage()).ToBeVisibleAsync();
            
            await Step("Wait for the first zaak to be connected");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            
            await Step("Verify first Zaak ZAAK-2023-005 is visible");
            await VerifyZaakIsVisible("ZAAK-2023-005");
            
            await Step("Click on the Pen-icon again to connect a different zaak");
            await Page.GetGekoppeldeZaakWijzigenButton().ClickAsync();
            
            await Step("Enter second Zaaknummer ZAAK-2023-009");
            await Page.GetZaaknummerTextbox().FillAsync("ZAAK-2023-009");
            await Page.GetZaaknummerTextbox().PressAsync("Enter");
            
            await Step("Verify success message for second zaak");
            await Expect(Page.GetZaakSuccesvolGekoppeldMessage()).ToBeVisibleAsync();
            
            await Step("Wait for the second zaak to be connected");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            
            await Step("Verify second Zaak ZAAK-2023-009 is visible");
            await VerifyZaakIsVisible("ZAAK-2023-009");
            
            await Step("Verify first Zaak ZAAK-2023-005 is no longer visible");
            await Expect(Page.Locator("text=ZAAK-2023-005")).Not.ToBeVisibleAsync();
        }

        [TestMethod("Assigning a Contactverzoek to yourself")]
        public async Task Assigning_Contactverzoek_To_Yourself()
        {
            var testOnderwerp = "Test_Contact_from_ITA_E2E_test_without_ZAAK";
            await SetupContactverzoek(testOnderwerp, attachZaak: false);

            await NavigateToContactverzoekDetails(testOnderwerp);
            
            await Step("Click on 'Toewijzen aan mezelf' button");
            await Page.GetToewijzenAanMezelfButton().ClickAsync();
            await Page.GetToewijzenAanMezelfDialogButton().ClickAsync();
            
            await Step("Verify success message is displayed");
            await Expect(Page.GetContactverzoekToegewezenMessage()).ToBeVisibleAsync();
            
            await Step("Wait for the contactverzoek to be assigned");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            
            await Step("Verify the current user is now shown as Behandelaar");
            await Expect(Page.GetBehandelaarValue()).ToHaveTextAsync("E2E test contactverzoek creator");
        }

        [TestMethod("validating Annuleren in confirmation dialog")]
        public async Task Assigning_Contactverzoek_To_Yourself_Annuleren()
        {
            var testOnderwerp = "Test_Contact_from_ITA_E2E_test_without_ZAAK";
            await SetupContactverzoek(testOnderwerp, attachZaak: false);

            await NavigateToContactverzoekDetails(testOnderwerp);
            
            await Step("Capture initial Behandelaar value");
            var initialBehandelaar = await Page.GetBehandelaarValue().InnerTextAsync();

            await Step("Click on 'Toewijzen aan mezelf' button and cancel");
            await Page.GetToewijzenAanMezelfButton().ClickAsync();
            await Page.GetAnnulerenDialogButton().ClickAsync();
            
            await Step("Verify Behandelaar field remains unchanged");
            await Expect(Page.GetBehandelaarValue()).ToHaveTextAsync(initialBehandelaar);
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

        private async Task NavigateToContactverzoekByNummer(string internetaakNummer)
        {
            await Step($"Navigate to contactverzoek by nummer: {internetaakNummer}");
            await Page.GotoAsync($"/contactverzoek/{internetaakNummer}");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            
            await Step($"Verify contactverzoek page loaded with nummer: {internetaakNummer}");
            await Expect(Page.GetByText($"Contactverzoek {internetaakNummer}")).ToBeVisibleAsync();
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
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await Page.GetInformatieVoorBurgerValue().WaitForAsync(new() { State = WaitForSelectorState.Visible });
            await Expect(Page.GetInformatieVoorBurgerValue()).ToContainTextAsync(
                "This is a test contact request created during an end-to-end test run.", new() { Timeout = 10000 });
        }

        private async Task VerifyZaakIsVisible(string zaakIdentificatie)
        {
            await Step($"Verify ZAAK '{zaakIdentificatie}' is visible");
            await Expect(Page.Locator("dd.utrecht-data-list__item-value").Filter(new() { HasText = zaakIdentificatie })).ToBeVisibleAsync();
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

        private async Task VerifyInternetaakStatusInOpenKlant(Guid internetaakUuid, string expectedStatus, bool shouldHaveAfgehandeldOp = false)
        {
            await Step($"Verify status in OpenKlant is '{expectedStatus}'");
            var internetaak = await TestDataHelper.GetInternetaakByIdAsync(internetaakUuid);
            
            Assert.IsNotNull(internetaak, "Internetaak should exist in OpenKlant");
            Assert.AreEqual(expectedStatus, internetaak.Status, $"Status should be '{expectedStatus}'");
            
            if (shouldHaveAfgehandeldOp)
            {
                await Step("Verify afgehandeldOp is set to current date and time");
                Assert.IsNotNull(internetaak.AfgehandeldOp, "AfgehandeldOp should be set");
                
                var timeDifference = Math.Abs((DateTimeOffset.UtcNow - internetaak.AfgehandeldOp.Value).TotalMinutes);
                Assert.IsTrue(timeDifference < 5, 
                    $"AfgehandeldOp should be close to current time. Difference: {timeDifference} minutes");
            }
        }

    }
}