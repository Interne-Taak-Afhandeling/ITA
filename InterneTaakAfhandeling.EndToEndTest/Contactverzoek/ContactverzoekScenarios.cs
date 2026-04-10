using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.EndToEndTest.Infrastructure;
using ITA.InterneTaakAfhandeling.EndToEndTest.Helpers;
using Microsoft.Playwright;
using System.Text.RegularExpressions;

namespace InterneTaakAfhandeling.EndToEndTest.Dashboard
{
    [TestClass]
    [DoNotParallelize]
    public partial class ContactverzoekScenarios : ITAPlaywrightTest
    {
        // Helper method for logbook verification
        private async Task VerifyLogbookEntry(string expectedAction, string expectedUserName = "ICATT Integratie Test", bool verifyTimestamp = true)
        {
            await Step($"Verify contact request history/logbook contains action with '{expectedAction}'");
            await Expect(Page.GetByText(expectedAction, new() { Exact = true })).ToBeVisibleAsync();

            await Step($"Verify logbook entry contains '{expectedAction}' description");
            var logbookEntry = Page.GetByText(expectedAction, new() { Exact = true }).Locator("..");
            var logbookText = await logbookEntry.InnerTextAsync();
            Assert.IsTrue(logbookText.Contains(expectedAction), $"Logbook entry should contain description '{expectedAction}'");

            await Step("Verify the user name is visible within the logbook section");
            var logbookSection = Page.Locator("ol");
            var logbookSectionText = await logbookSection.InnerTextAsync();
            var hasUserName = logbookSectionText.Contains(expectedUserName);
            Assert.IsTrue(hasUserName, $"Logbook section should contain user name '{expectedUserName}'. Actual text: '{logbookSectionText}'");

            if (verifyTimestamp)
            {
                await Step("Verify datetime pattern exists in the logbook section");
                var dateTimePattern = @"\d{2}-\d{2}-\d{4} \d{2}:\d{2}";
                var dateTimeMatch = Regex.Match(logbookSectionText, dateTimePattern);
                Assert.IsTrue(dateTimeMatch.Success, $"Logbook section should contain date/time in DD-MM-YYYY HH:MM format. Actual text: '{logbookSectionText}'");

                if (dateTimeMatch.Success)
                {
                    var displayedDateTime = dateTimeMatch.Value;
                    await Step($"Verify datetime '{displayedDateTime}' is visible in logbook section");
                    await Expect(logbookSection.GetByText(displayedDateTime)).ToBeVisibleAsync();
                }
            }
        }

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
            await Page.GetContactOpnemenNietGeluktRadio().ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("Select Kanaal");
            await Page.GetKanalenSelect().SelectOptionAsync(new[] { "Telefoon" });

            await Step("Select 'Ja' for afsluiten (to close the contactverzoek)");
            await Page.GetJaLabel().ClickAsync();

            await Step("Try to save without informatie - should show validation error");
            await Page.GetContactmomentOpslaanButton().ClickAsync();

            await Step("Verify validation error on Informatie field");
            await Expect(Page.GetInformatieBurgerTextbox()).ToHaveJSPropertyAsync("validity.valueMissing", true);

            await Step("Fill in Informatie field");
            await Page.GetInformatieBurgerTextbox().FillAsync("Required information when closing");

            await Step("Submit form - should now trigger confirmation modal");
            await Page.GetContactmomentOpslaanButton().ClickAsync();

            await Step("Verify confirmation dialog is displayed");
            await Expect(Page.GetByRole(AriaRole.Dialog)).ToBeVisibleAsync();

            await Step("Confirm closure");
            await Page.GetOpslaanEnAfrondenButton().ClickAsync();

            await Step("Verify contactverzoek was closed successfully");
            await Expect(Page.GetContactmomentSuccesvolOpgeslagenEnAfgerondMessage()).ToBeVisibleAsync();
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
            await Page.GetKanalenSelect().SelectOptionAsync(new[] { "Telefoon" });
            await Page.GetInformatieBurgerTextbox().FillAsync("Test information for save and close");

            await Step("Select 'Ja' for afsluiten question");
            await Page.GetJaLabel().ClickAsync();

            await Step("Click 'Contactmoment opslaan' button to trigger confirmation");
            await Page.GetContactmomentOpslaanButton().ClickAsync();

            await Step("Verify confirmation dialog is displayed");
            await Expect(Page.GetByRole(AriaRole.Dialog)).ToBeVisibleAsync();

            await Step("Click 'Opslaan & afronden' on confirmation dialog");
            await Page.GetOpslaanEnAfrondenButton().ClickAsync();

            await Step("Verify request is closed - success message");
            await Expect(Page.GetContactmomentSuccesvolOpgeslagenEnAfgerondMessage()).ToBeVisibleAsync();
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
            await Page.GetKanalenSelect().SelectOptionAsync(new[] { "Telefoon" });
            await Page.GetInformatieBurgerTextbox().FillAsync("Test information for cancel scenario");

            await Step("Select 'Ja' for afsluiten question");
            await Page.GetJaLabel().ClickAsync();

            await Step("Click 'Contactmoment opslaan' button to trigger confirmation");
            await Page.GetContactmomentOpslaanButton().ClickAsync();

            await Step("Verify confirmation dialog is displayed");
            await Expect(Page.GetByRole(AriaRole.Dialog)).ToBeVisibleAsync();

            await Step("Click 'No' (Annuleren) on confirmation dialog");
            await Page.GetAnnulerenDialogButton().ClickAsync();

            await Step("Verify dialog is closed");
            await Expect(Page.GetByRole(AriaRole.Dialog)).Not.ToBeVisibleAsync();

            await Step("Verify request is NOT closed");
            await Expect(Page.GetContactmomentSuccesvolOpgeslagenEnAfgerondMessage()).Not.ToBeVisibleAsync();

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
            await Page.GetKanalenSelect().SelectOptionAsync(new[] { "Telefoon" });
            await Page.GetInformatieBurgerTextbox().FillAsync("Initial contactmoment before closing");

            await Step("Select 'Ja' for afsluiten to close the contactverzoek");
            await Page.GetJaLabel().ClickAsync();

            await Step("Click 'Contactmoment opslaan' button to trigger confirmation");
            await Page.GetContactmomentOpslaanButton().ClickAsync();

            await Step("Verify confirmation dialog is displayed");
            await Expect(Page.GetByRole(AriaRole.Dialog)).ToBeVisibleAsync();

            await Step("Click 'Opslaan & afronden' on confirmation dialog");
            await Page.GetOpslaanEnAfrondenButton().ClickAsync();

            await Step("Wait for close confirmation");
            await Expect(Page.GetContactmomentSuccesvolOpgeslagenEnAfgerondMessage()).ToBeVisibleAsync();
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
            await Page.GetKanalenSelect().SelectOptionAsync(new[] { "E-mail" });
            await Page.GetInformatieBurgerTextbox().FillAsync("Follow-up contactmoment on closed request");
            
            await Step("Select 'Nee' for afsluiten (don't close it again)");
            await Page.GetNeeLabel().ClickAsync();
            await Page.GetContactmomentOpslaanButton().ClickAsync();

            await Step("Verify new contactmoment was saved successfully");
            await Expect(Page.GetContactmomentSuccesvolBijgewerktMessage()).ToBeVisibleAsync();
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

        [TestMethod("User clicks on MijnHistorie and views closed assigned contact requests")]
        public async Task User_ClickMijnHistorie_ViewsClosedAssignedContactRequests()
        {
            await Step("Given user is on ITA - Create and assign a contactverzoek to current user");
            var testOnderwerp = $"Test_MijnHistorie_Scenario_{Guid.NewGuid().ToString().Substring(0, 8)}";
            await CreateAssignAndCloseContactverzoek(testOnderwerp, "Test closure for MijnHistorie scenario");

            await Step("When user clicks on MijnHistorie");
            await SafeGotoAsync("/");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await Page.GetByRole(AriaRole.Link, new() { Name = "Mijn historie", Exact = true }).ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("Then closed contact request assigned to the employee is displayed");
            await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Mijn historie", Level = 1 })).ToBeVisibleAsync();
            await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Mijn afgeronde contactverzoeken", Level = 2 })).ToBeVisibleAsync();
            await Expect(Page.Locator($"text={testOnderwerp}")).ToBeVisibleAsync();

            await Step("Verify the contact request shows as assigned to current user");
            var tableRows = Page.Locator("table tbody tr");
            await Expect(tableRows.First).ToBeVisibleAsync();
            var firstRowText = await tableRows.First.InnerTextAsync();
            Assert.IsTrue(firstRowText.Contains(testOnderwerp), $"Expected to find '{testOnderwerp}' in the history table");

            await Step("Given user is on Historie - When user clicks on a contactverzoek from the list");
            var contactverzoekRow = Page.Locator("table tbody tr").Filter(new() { HasText = testOnderwerp });
            var detailsLink = contactverzoekRow.GetByRole(AriaRole.Link).Filter(new() { HasText = "Klik hier" });
            await detailsLink.ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("Then the detail screen of the contact request is the same as the 'Alle contactverzoeken' page");
            await VerifyBasicContactverzoekFields(testOnderwerp);
            await VerifyActionTabsArePresent();
            await VerifyMetadataFields();

            await Step("Verify the contactverzoek detail page elements are present");
            await Expect(Page.Locator($"text={testOnderwerp}")).ToBeVisibleAsync();
            await Expect(Page.GetContactmomentRegistrerenTab()).ToBeVisibleAsync();
            await Expect(Page.GetDoorsturenTab()).ToBeVisibleAsync();
            await Expect(Page.GetAlleenToelichtingTab()).ToBeVisibleAsync();

            await Step("Verify the contactverzoek shows as verwerkt status");
            await Expect(Page.GetStatusValue()).ToHaveTextAsync("verwerkt");
        }

        [TestMethod("User navigates to unassigned contactverzoek, assigns to self, closes it, and views in history")]
        public async Task User_AssignUnassignedContactverzoek_CloseAndViewInHistory()
        {
            var testOnderwerp = $"Test_Unassigned_Reassignment_{Guid.NewGuid().ToString().Substring(0, 8)}";
            
            await Step("Given user is on ITA - Create contactverzoek for another employee (not assigned to current user)");
            var contactmomentUuid = await SetupContactverzoek(testOnderwerp, attachZaak: false, TestDataConstants.ContactverzoekNummers.UnassignedForReassignment);

            await Step("And navigates to a contactverzoek that is created for another employee");
            await SafeGotoAsync($"/contactverzoek/{TestDataConstants.ContactverzoekNummers.UnassignedForReassignment}");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("Verify contactverzoek page is loaded");
            await Expect(Page.GetByText($"Contactverzoek {TestDataConstants.ContactverzoekNummers.UnassignedForReassignment}")).ToBeVisibleAsync();
            await Expect(Page.Locator($"text={testOnderwerp}")).ToBeVisibleAsync();

            await Step("And assign the contactverzoek to self");
            await Page.GetToewijzenAanMezelfButton().ClickAsync();
            await Page.GetToewijzenAanMezelfDialogButton().ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("Verify the current user is now shown as Behandelaar");
            await Expect(Page.GetBehandelaarValue()).ToHaveTextAsync("E2E test contactverzoek creator");

            await Step("When user closes the contact request");
            await NavigateToContactmomentRegistrerenTab();
            await Expect(Page.GetContactOpnemenGeluktRadio()).ToBeCheckedAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Page.GetKanalenSelect().SelectOptionAsync(new[] { "Telefoon" });
            await Page.GetInformatieBurgerTextbox().FillAsync("Contact request closed after reassignment");
            await Page.GetJaLabel().ClickAsync();
            await Page.GetContactmomentOpslaanButton().ClickAsync();
            await Expect(Page.GetByRole(AriaRole.Dialog)).ToBeVisibleAsync();
            await Page.GetOpslaanEnAfrondenButton().ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("And navigates to History tab");
            await SafeGotoAsync("/historie");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("Reload page to ensure latest data is displayed");
            await Page.ReloadAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("Then the closed contact request is displayed in the history tab");
            await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Mijn historie", Level = 1 })).ToBeVisibleAsync();
            await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Mijn afgeronde contactverzoeken", Level = 2 })).ToBeVisibleAsync();
            await Expect(Page.Locator($"text={testOnderwerp}")).ToBeVisibleAsync();

            await Step("Verify the reassigned and closed contact request shows in user's history");
            var tableRows = Page.Locator("table tbody tr");
            await Expect(tableRows.First).ToBeVisibleAsync();
            var firstRowText = await tableRows.First.InnerTextAsync();
            Assert.IsTrue(firstRowText.Contains(testOnderwerp), $"Expected to find reassigned and closed contactverzoek '{testOnderwerp}' in the history table");

            await Step("Verify status in OpenKlant is verwerkt with AfgehandeldOp set");
            var internetaakUuid = await TestDataHelper.GetInternetaakUuidFromContactmomentAsync(contactmomentUuid);
            Assert.IsNotNull(internetaakUuid, "Internetaak UUID should be found");
            await VerifyInternetaakStatusInOpenKlant(internetaakUuid.Value, "verwerkt", shouldHaveAfgehandeldOp: true);
        }

        [TestMethod("Assigning a Contactverzoek to yourself - with logbook verification")]
        public async Task User_AssignContactverzoekToSelf_VerifyLogbookEntry()
        {
            var testOnderwerp = "Test_Contact_Opgepakt_Logbook";
            await SetupContactverzoek(testOnderwerp, attachZaak: false);
            await NavigateToContactverzoekDetails(testOnderwerp);

            await Step("Click on 'Toewijzen aan mezelf' button");
            await Page.GetToewijzenAanMezelfButton().ClickAsync();
            
            await Step("Confirm assignment in dialog");
            await Page.GetToewijzenAanMezelfDialogButton().ClickAsync();

            await Step("Verify success message is displayed");
            await Expect(Page.GetContactverzoekToegewezenMessage()).ToBeVisibleAsync();

            await Step("Wait for the contactverzoek to be assigned");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("Verify the current user is now shown as Behandelaar");
            await Expect(Page.GetBehandelaarValue()).ToHaveTextAsync("E2E test contactverzoek creator");

            await VerifyLogbookEntry("Opgepakt");
        }

        [TestMethod("Assigning a Zaak to contact request - with logbook verification")]
        public async Task User_AssignZaakToContactverzoek_VerifyLogbookEntry()
        {
            var testOnderwerp = "Test_Contact_Zaak_Gekoppeld_Logbook";
            await SetupContactverzoek(testOnderwerp, attachZaak: false);

            await NavigateToContactverzoekDetails(testOnderwerp);

            await Step("Click on the Pen-icon next to label 'Gekoppelde zaak'");
            await Page.GetGekoppeldeZaakKoppelenButton().ClickAsync();

            await Step("Enter valid Zaaknummer");
            await Page.GetZaaknummerTextbox().FillAsync("ZAAK-2023-009");

            await Step("Click on button 'Koppelen'");
            await Page.GetKoppelenButton().ClickAsync();

            await Step("Verify success message for zaak linking");
            await Expect(Page.GetZaakSuccesvolGekoppeldMessage()).ToBeVisibleAsync();

            await Step("Wait for the zaak to be connected");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("Verify the zaak is now visible");
            await VerifyZaakIsVisible("ZAAK-2023-009");

            await VerifyLogbookEntry("Zaak gekoppeld");
        }

        [TestMethod("Replacing existing Zaak on contact request - with logbook verification")]
        public async Task User_ReplaceZaakOnContactverzoek_VerifyLogbookEntry()
        {
            var testOnderwerp = "Test_Contact_Zaak_Gewijzigd_Logbook";
            await SetupContactverzoek(testOnderwerp, attachZaak: true); // Start with a zaak already attached

            await NavigateToContactverzoekDetails(testOnderwerp);

            await Step("Verify initial zaak is visible");
            await VerifyZaakIsVisible(TestDataConstants.Zaken.TestZaakIdentificatie);

            await Step("Click on the Edit zaak to change the existing zaak");
            await Page.GetGekoppeldeZaakWijzigenButton().ClickAsync();

            await Step("Enter different valid Zaaknummer");
            await Page.GetZaaknummerTextbox().FillAsync("ZAAK-2023-009");

            await Step("Click on button 'Koppelen'");
            await Page.GetKoppelenButton().ClickAsync();

            await Step("Verify success message for zaak linking");
            await Expect(Page.GetZaakSuccesvolGekoppeldMessage()).ToBeVisibleAsync();

            await Step("Wait for the new zaak to be connected");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("Verify the new zaak is now visible");
            await VerifyZaakIsVisible("ZAAK-2023-009");

            await Step("Verify the old zaak is no longer visible");
            await Expect(Page.Locator($"text={TestDataConstants.Zaken.TestZaakIdentificatie}")).Not.ToBeVisibleAsync();

            await VerifyLogbookEntry("Zaak gewijzigd");
        }

        [TestMethod("Contact opnemen gelukt - with logbook verification")]
        public async Task User_RegisterContactOpnemenGelukt_VerifyLogbookEntry()
        {
            var testOnderwerp = "Test_Contact_Gelukt_Logbook";
            await SetupContactverzoek(testOnderwerp, attachZaak: false);

            await NavigateToContactverzoekDetails(testOnderwerp);
            await NavigateToContactmomentRegistrerenTab();

            await Step("Verify 'Contact opnemen gelukt' is selected by default");
            await Expect(Page.GetContactOpnemenGeluktRadio()).ToBeCheckedAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("Select 'Nee' for afsluiten question");
            await Page.GetNeeLabel().ClickAsync();

            await Step("Select kanaal as 'Balie'");
            await Page.GetKanalenSelect().SelectOptionAsync(new[] { "Balie" });

            await Step("Enter details in field 'Informatie voor burger / bedrijf' as 'test logboek'");
            await Page.GetInformatieBurgerTextbox().FillAsync("test logboek");

            await Step("Click on contactmoment opslaan");
            await Page.GetContactmomentOpslaanButton().ClickAsync();

            await Step("Verify contactmoment was saved successfully");
            await Expect(Page.GetContactmomentSuccesvolBijgewerktMessage()).ToBeVisibleAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await VerifyLogbookEntry("Contact gelukt");

            await Step("Verify the information 'test logboek' is visible in the logbook section");
            var logbookSection = Page.Locator("ol");
            await Expect(logbookSection.GetByText("test logboek")).ToBeVisibleAsync();
        }

        [TestMethod("Close contactmoment, navigate to Mijn historie and verify logbook entry")]
        public async Task User_CloseContactmoment_NavigateToHistorieAndVerifyLogbookAfgerond()
        {
            var testOnderwerp = $"Test_Contact_Historie_Afgerond_{Guid.NewGuid().ToString().Substring(0, 8)}";
            await SetupContactverzoek(testOnderwerp, attachZaak: false);

            await NavigateToContactverzoekDetails(testOnderwerp);
            await NavigateToContactmomentRegistrerenTab();

            await Step("Verify 'Contact opnemen gelukt' is selected by default");
            await Expect(Page.GetContactOpnemenGeluktRadio()).ToBeCheckedAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("Select 'Ja' for afsluiten question");
            await Page.GetJaLabel().ClickAsync();

            await Step("Select kanaal as 'Balie'");
            await Page.GetKanalenSelect().SelectOptionAsync(new[] { "Balie" });

            await Step("Enter details in field 'Informatie voor burger / bedrijf' as 'test logboek'");
            await Page.GetInformatieBurgerTextbox().FillAsync("test logboek");

            await Step("Click on 'Contactmoment opslaan' button");
            await Page.GetContactmomentOpslaanButton().ClickAsync();

            await Step("Verify confirmation dialog is shown");
            await Expect(Page.GetByRole(AriaRole.Dialog)).ToBeVisibleAsync();

            await Step("Click on 'Opslaan & afronden' in the confirmation dialog");
            await Page.GetOpslaanEnAfrondenButton().ClickAsync();

            await Step("Verify contactverzoek was closed successfully");
            await Expect(Page.GetContactmomentSuccesvolOpgeslagenEnAfgerondMessage()).ToBeVisibleAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
 
            await Step("Navigate to Mijn historie tab");
            await SafeGotoAsync("/historie");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("Verify the closed contactverzoek appears in historie");
            await Expect(Page.Locator($"text={testOnderwerp}")).ToBeVisibleAsync();

            await Step("Click on the contactverzoek from the historie list");
            await Page.GetDetailsLink(testOnderwerp).ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("Verify contactverzoek details page is displayed");
            await Expect(Page.Locator($"text={testOnderwerp}")).ToBeVisibleAsync();

            await VerifyLogbookEntry("Afgerond", verifyTimestamp: false);

            await Step("Verify the information 'test logboek' is visible in the logbook section");
            var logbookSection = Page.Locator("ol");
            await Expect(logbookSection.GetByText("test logboek")).ToBeVisibleAsync();
        }
    }
}