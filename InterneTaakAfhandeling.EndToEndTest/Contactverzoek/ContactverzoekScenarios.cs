using System.Text.RegularExpressions;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.EndToEndTest.Infrastructure;
using ITA.InterneTaakAfhandeling.EndToEndTest.Helpers;
using Microsoft.Playwright;

namespace InterneTaakAfhandeling.EndToEndTest.Dashboard
{
    [TestClass]
    [DoNotParallelize]
    public partial class ContactverzoekScenarios : ITAPlaywrightTest
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
            await Expect(Page.GetContactOpnemenGeluktRadio()).ToBeCheckedAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("Fill form fields");
            await Page.Locator("#kanalen").SelectOptionAsync(new[] { "Telefoon" });
            await Page.Locator("#informatie-burger").FillAsync("Test information for cancel scenario");

            await Step("Select 'Ja' for afsluiten question");
            await Page.GetByLabel("Ja").ClickAsync();

            await Step("Click 'Contactmoment opslaan' button to trigger confirmation");
            await Page.GetContactmomentOpslaanButton().ClickAsync();

            await Step("Verify confirmation dialog is displayed");
            await Expect(Page.GetByRole(AriaRole.Dialog)).ToBeVisibleAsync();

            await Step("Click 'No' (Annuleren) on confirmation dialog");
            await Page.GetAnnulerenDialogButton().ClickAsync();

            await Step("Verify dialog is closed");
            await Expect(Page.GetByRole(AriaRole.Dialog)).Not.ToBeVisibleAsync();

            await Step("Verify request is NOT closed");
            await Expect(Page.GetByText("Contactmoment succesvol opgeslagen en afgerond")).Not.ToBeVisibleAsync();

            await Step("Verify user remains on contactverzoek detail page");
            await Expect(Page.GetContactmomentRegistrerenTab()).ToBeVisibleAsync();
            await Expect(Page.Locator($"text={testOnderwerp}")).ToBeVisibleAsync();

            await Step("Verify status in OpenKlant remains 'te_verwerken'");
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
            await CreateAssignAndCloseContactverzoek(testOnderwerp, "Closing contactverzoek for Mijn historie test");

            await Step("Navigate to Mijn historie page");
            await SafeGotoAsync("/historie");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("Verify page title is 'Mijn historie'");
            await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Mijn historie", Level = 1 })).ToBeVisibleAsync();

            await Step("Verify section heading is 'Mijn afgeronde contactverzoeken'");
            await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Mijn afgeronde contactverzoeken", Level = 2 })).ToBeVisibleAsync();

            await Step("Verify closed contactverzoek is displayed in the table");
            await Expect(Page.Locator($"text={testOnderwerp}")).ToBeVisibleAsync();
        }

        [TestMethod("View completed contactverzoeken of afdeling")]
        public async Task User_ViewCompletedContactverzoekenInAfdelingshistorie()
        {
            const string afdelingNaam = "Burgerzaken_ibz";
            var testOnderwerp = $"Test_Afdelingshistorie_{Guid.NewGuid().ToString().Substring(0, 8)}";

            await CreateAssignAndCloseContactverzoek(testOnderwerp, "Afdelingshistorie test close");
            await NavigateToAfdelingshistorieAndSelectOrganisatorischeEenheid(afdelingNaam);

            await Step("Verify the closed contactverzoek appears in afdeling history");
            await Expect(Page.Locator($"text={testOnderwerp}")).ToBeVisibleAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }

        [TestMethod("View completed contactverzoeken of groep")]
        public async Task User_ViewCompletedContactverzoekenInGroepHistorie()
        {
            const string groepNaam = "Communicatieadviseurs";
            await NavigateToAfdelingshistorieAndSelectOrganisatorischeEenheid(groepNaam);

            await Step("Verify closed contactverzoeken for the groep are displayed");
            var tableRows = Page.Locator("table tbody tr");
            await Expect(tableRows.First).ToBeVisibleAsync();
            var rowCount = await tableRows.CountAsync();
            Assert.IsTrue(rowCount > 0, "Expected at least one closed contactverzoek for selected groep");

            await VerifyLatestRecordIsOnTopInHistorieTable(tableRows, rowCount);
        }

        [TestMethod("Validation that reassigned and closed contactverzoek appears in Mijn historie in descending order")]
        public async Task User_ReassignCloseContactverzoek_AppearsInHistorieSortedDescending()
        {
            var firstOnderwerp = $"Test_First_{Guid.NewGuid().ToString().Substring(0, 8)}";
            var secondOnderwerp = $"Test_Second_{Guid.NewGuid().ToString().Substring(0, 8)}";

            await Step("Create first and second contactverzoek one after another");
            var first = await SetupAndResolveContactverzoek(firstOnderwerp, TestDataConstants.ContactverzoekNummers.HistorieFirst);
            var second = await SetupAndResolveContactverzoek(secondOnderwerp, TestDataConstants.ContactverzoekNummers.HistorieSecond);

            await Step("Verify both interne taken are created before closing");
            await VerifyInternetaakStatusInOpenKlant(first.internetaakUuid, "te_verwerken");
            await VerifyInternetaakStatusInOpenKlant(second.internetaakUuid, "te_verwerken");

            await CloseContactverzoekByNummer(first.internetaakNummer, "Telefoon", "First contactverzoek closed");
            await VerifyInternetaakStatusInOpenKlant(first.internetaakUuid, "verwerkt", shouldHaveAfgehandeldOp: true);

            await Task.Delay(2000);

            await CloseContactverzoekByNummer(second.internetaakNummer, "E-mail", "Second contactverzoek closed - most recent");
            await VerifyInternetaakStatusInOpenKlant(second.internetaakUuid, "verwerkt", shouldHaveAfgehandeldOp: true);

            await VerifyMijnHistorieDescendingOrder(secondOnderwerp, firstOnderwerp);
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

        [TestMethod("Check afdelingen en groepen dropdown in Afdelingshistorie")]
        public async Task User_CheckAfdelingenEnGroepenDropdown_InAfdelingshistorie()
        {
            await Step("Navigate to Dashboard");
            await SafeGotoAsync("/");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("Verify menu option 'Afdelingshistorie' is visible");
            await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Afdelingshistorie", Exact = true })).ToBeVisibleAsync();

            await Step("Click menu option 'Afdelingshistorie'");
            await Page.GetByRole(AriaRole.Link, new() { Name = "Afdelingshistorie", Exact = true }).ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("Verify page contains no Contactverzoeken");
            var tableRows = Page.Locator("table tbody tr");
            var rowCount = await tableRows.CountAsync();
            Assert.AreEqual(0, rowCount, "Expected no Contactverzoeken on initial Afdelingshistorie page");

            await Step("Verify dropdown is visible and has selectable options");
            var dropdown = Page.GetByLabel("Selecteer een afdeling of");
            await Expect(dropdown).ToBeVisibleAsync();
            
            await Step("Verify dropdown contains options");
            var options = dropdown.Locator("option");
            var optionCount = await options.CountAsync();
            Assert.IsTrue(optionCount > 1, "Dropdown should contain at least one selectable option besides the default");
        }
    }
}