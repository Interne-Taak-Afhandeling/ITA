using InterneTaakAfhandeling.EndToEndTest.Infrastructure;
using Microsoft.Playwright;

namespace InterneTaakAfhandeling.EndToEndTest.Contactverzoek
{
    /// <summary>
    /// E2E tests for groepsmailbox-aware forwarding (Feature #512, Task #550).
    /// Phase 2 verification: covers all UI-observable Gherkin scenarios from Tasks
    /// #538, #539, #541, #542 and #543 against the deployed test environment.
    /// </summary>
    [TestClass]
    [DoNotParallelize]
    public class ContactverzoekDoorsturenGroepsmailboxScenarios : ITAPlaywrightTest
    {
        private const string MedewerkerOptioneelLabel = "Medewerker (optioneel)";
        private const string MedewerkerVerplichtLabel = "Medewerker";

        // === Task #538: Afdelingen- en Groepenoverzicht endpoints ===

        [TestMethod("Afdeling met groepsmailbox")]
        public async Task Afdeling_MetGroepsmailbox_GeeftAanGroepsmailboxTeHebben()
        {
            var onderwerp = $"Test_Groepsmailbox_AfdMet_{Guid.NewGuid().ToString()[..8]}";
            await SetupContactverzoek(onderwerp);
            await NavigateToContactverzoekAndOpenDoorsturenTab(onderwerp);

            await Step("Verify 'Afdeling' mode is default");
            await Expect(Page.GetDoorsturenAfdelingRadio()).ToBeCheckedAsync();

            await Step("Find an afdeling with a groepsmailbox and medewerkers attached");
            var found = await SelectOptionUntilMedewerkerLabelIsAsync(
                Page.GetAfdelingSelect(), Page.GetAfdelingMedewerkerLabel(), MedewerkerOptioneelLabel);
            Assert.IsTrue(found, "No afdeling with a groepsmailbox (and attached medewerkers) found in test environment");

            await Step("Verify the medewerkerveld reflects the groepsmailbox (optional, not required)");
            await Expect(Page.GetAfdelingGroepMedewerkerCombobox()).ToHaveJSPropertyAsync("required", false);
        }

        [TestMethod("Afdeling zonder groepsmailbox")]
        public async Task Afdeling_ZonderGroepsmailbox_GeeftAanGeenGroepsmailboxTeHebben()
        {
            var onderwerp = $"Test_Groepsmailbox_AfdZonder_{Guid.NewGuid().ToString()[..8]}";
            await SetupContactverzoek(onderwerp);
            await NavigateToContactverzoekAndOpenDoorsturenTab(onderwerp);

            await Step("Find an afdeling without a groepsmailbox (with medewerkers attached)");
            var found = await SelectOptionUntilMedewerkerLabelIsAsync(
                Page.GetAfdelingSelect(), Page.GetAfdelingMedewerkerLabel(), MedewerkerVerplichtLabel);
            Assert.IsTrue(found, "No afdeling without a groepsmailbox (with attached medewerkers) found in test environment");

            await Step("Verify the medewerkerveld reflects the missing groepsmailbox (required)");
            await Expect(Page.GetAfdelingGroepMedewerkerCombobox()).ToHaveJSPropertyAsync("required", true);
        }

        [TestMethod("Groep met groepsmailbox")]
        public async Task Groep_MetGroepsmailbox_GeeftAanGroepsmailboxTeHebben()
        {
            var onderwerp = $"Test_Groepsmailbox_GrpMet_{Guid.NewGuid().ToString()[..8]}";
            await SetupContactverzoek(onderwerp);
            await NavigateToContactverzoekAndOpenDoorsturenTab(onderwerp);

            await Step("Select 'Groep' mode");
            await Page.GetDoorsturenGroepRadio().ClickAsync();
            await Expect(Page.GetGroepSelect()).ToBeVisibleAsync();

            await Step("Find a groep with a groepsmailbox and medewerkers attached");
            var found = await SelectOptionUntilMedewerkerLabelIsAsync(
                Page.GetGroepSelect(), Page.GetGroepMedewerkerLabel(), MedewerkerOptioneelLabel);
            Assert.IsTrue(found, "No groep with a groepsmailbox (and attached medewerkers) found in test environment");

            await Step("Verify the medewerkerveld reflects the groepsmailbox (optional, not required)");
            await Expect(Page.GetGroepMedewerkerCombobox()).ToHaveJSPropertyAsync("required", false);
        }

        [TestMethod("Groep zonder groepsmailbox")]
        public async Task Groep_ZonderGroepsmailbox_GeeftAanGeenGroepsmailboxTeHebben()
        {
            var onderwerp = $"Test_Groepsmailbox_GrpZonder_{Guid.NewGuid().ToString()[..8]}";
            await SetupContactverzoek(onderwerp);
            await NavigateToContactverzoekAndOpenDoorsturenTab(onderwerp);

            await Step("Select 'Groep' mode");
            await Page.GetDoorsturenGroepRadio().ClickAsync();
            await Expect(Page.GetGroepSelect()).ToBeVisibleAsync();

            await Step("Find a groep without a groepsmailbox (with medewerkers attached)");
            var found = await SelectOptionUntilMedewerkerLabelIsAsync(
                Page.GetGroepSelect(), Page.GetGroepMedewerkerLabel(), MedewerkerVerplichtLabel);
            Assert.IsTrue(found, "No groep without a groepsmailbox (with attached medewerkers) found in test environment");

            await Step("Verify the medewerkerveld reflects the missing groepsmailbox (required)");
            await Expect(Page.GetGroepMedewerkerCombobox()).ToHaveJSPropertyAsync("required", true);
        }

        // === Task #539: Eén eenduidige notificatie-ontvanger ===

        [TestMethod("Toegewezen aan zowel medewerker als afdeling/groep")]
        public async Task User_ForwardWithMedewerkerAndAfdelingMetMailbox_ForwardsSuccessfully()
        {
            var onderwerp = $"Test_EenOntvanger_Beide_{Guid.NewGuid().ToString()[..8]}";
            await SetupContactverzoek(onderwerp);
            await NavigateToContactverzoekAndOpenDoorsturenTab(onderwerp);

            await Step("Find an afdeling with a groepsmailbox and medewerkers attached");
            var found = await SelectOptionUntilMedewerkerLabelIsAsync(
                Page.GetAfdelingSelect(), Page.GetAfdelingMedewerkerLabel(), MedewerkerOptioneelLabel);
            Assert.IsTrue(found, "No afdeling with a groepsmailbox (and attached medewerkers) found in test environment");

            await Step("Select a medewerker on the (optional) nested combobox");
            await SelectFirstNestedMedewerker(Page.GetAfdelingGroepMedewerkerCombobox());

            await Step("Submit the forward form");
            await Page.GetContactverzoekDoorsturenButton().ClickAsync();

            await Step("Verify success toast");
            await Expect(Page.GetSuccessToast("succesvol doorgestuurd")).ToBeVisibleAsync();
        }

        [TestMethod("Toegewezen aan uitsluitend een medewerker")]
        public async Task User_ForwardWithMedewerkerAndAfdelingZonderMailbox_MedewerkerIsSoleRecipient()
        {
            var onderwerp = $"Test_EenOntvanger_Medewerker_{Guid.NewGuid().ToString()[..8]}";
            await SetupContactverzoek(onderwerp);
            await NavigateToContactverzoekAndOpenDoorsturenTab(onderwerp);

            await Step("Find an afdeling without a groepsmailbox (with medewerkers attached)");
            var found = await SelectOptionUntilMedewerkerLabelIsAsync(
                Page.GetAfdelingSelect(), Page.GetAfdelingMedewerkerLabel(), MedewerkerVerplichtLabel);
            Assert.IsTrue(found, "No afdeling without a groepsmailbox (with attached medewerkers) found in test environment");

            await Step("Select the required medewerker (afdeling itself has no mailbox to fall back on)");
            await SelectFirstNestedMedewerker(Page.GetAfdelingGroepMedewerkerCombobox());

            await Step("Submit the forward form");
            await Page.GetContactverzoekDoorsturenButton().ClickAsync();

            await Step("Verify success toast — since the afdeling has no mailbox, this can only succeed via the medewerker's own email");
            await Expect(Page.GetSuccessToast("Contactverzoek succesvol doorgestuurd")).ToBeVisibleAsync();
        }

        [TestMethod("Toegewezen aan uitsluitend een afdeling/groep")]
        public async Task User_ForwardWithAfdelingMetMailboxOnly_ForwardsSuccessfully()
        {
            var onderwerp = $"Test_EenOntvanger_Afdeling_{Guid.NewGuid().ToString()[..8]}";
            await SetupContactverzoek(onderwerp);
            await NavigateToContactverzoekAndOpenDoorsturenTab(onderwerp);

            await Step("Find an afdeling with a groepsmailbox and medewerkers attached");
            var found = await SelectOptionUntilMedewerkerLabelIsAsync(
                Page.GetAfdelingSelect(), Page.GetAfdelingMedewerkerLabel(), MedewerkerOptioneelLabel);
            Assert.IsTrue(found, "No afdeling with a groepsmailbox (and attached medewerkers) found in test environment");

            await Step("Submit the forward form, leaving the optional medewerkerveld empty");
            await Page.GetContactverzoekDoorsturenButton().ClickAsync();

            await Step("Verify success toast — the afdeling's groepsmailbox is the sole recipient");
            await Expect(Page.GetSuccessToast("Contactverzoek succesvol doorgestuurd")).ToBeVisibleAsync();
        }

        [TestMethod("Medewerker zonder geldig e-mailadres, afdeling met groepsmailbox")]
        public async Task User_ForwardWithMedewerkerZonderEmail_FallsBackToAfdelingGroepsmailbox()
        {
            if (string.IsNullOrEmpty(TestDataConstants.Doorsturen.TestMedewerkerNoEmailSearchQuery))
            {
                Assert.Inconclusive(
                    "TestMedewerkerNoEmailSearchQuery is not configured. " +
                    "Set this constant to a medewerker without an email address in the test objectenregister.");
            }

            var onderwerp = $"Test_EenOntvanger_Terugval_{Guid.NewGuid().ToString()[..8]}";
            await SetupContactverzoek(onderwerp);
            await NavigateToContactverzoekAndOpenDoorsturenTab(onderwerp);

            await Step("Select 'Medewerker' mode");
            await Page.GetDoorsturenMedewerkerRadio().ClickAsync();

            await Step("Search for medewerker without email");
            var combobox = Page.GetMedewerkerCombobox();
            await combobox.FillAsync(TestDataConstants.Doorsturen.TestMedewerkerNoEmailSearchQuery);

            await Step("Wait for search results and select first option");
            var listbox = Page.Locator("#medewerker-combobox-listbox");
            await Expect(listbox).ToBeVisibleAsync();
            await listbox.GetByRole(AriaRole.Option).First.ClickAsync();

            await Step("Select secondary afdeling/groep");
            await SelectFirstSecondaryOption();

            await Step("Submit the forward form");
            await Page.GetContactverzoekDoorsturenButton().ClickAsync();

            await Step("Verify success toast — the afdeling/groep mailbox receives the notification as fallback");
            await Expect(Page.GetSuccessToast("Contactverzoek succesvol doorgestuurd")).ToBeVisibleAsync();
        }

        // === Task #541: Medewerker verplicht bij ontbrekende groepsmailbox ===

        [TestMethod("Doorsturen naar afdeling zonder groepsmailbox en zonder medewerker wordt geweigerd")]
        public async Task User_CannotForward_WhenAfdelingZonderMailboxAndNoMedewerkerSelected()
        {
            var onderwerp = $"Test_Verplicht_AfdGeweigerd_{Guid.NewGuid().ToString()[..8]}";
            await SetupContactverzoek(onderwerp);
            await NavigateToContactverzoekAndOpenDoorsturenTab(onderwerp);

            await Step("Find an afdeling without a groepsmailbox (with medewerkers attached)");
            var found = await SelectOptionUntilMedewerkerLabelIsAsync(
                Page.GetAfdelingSelect(), Page.GetAfdelingMedewerkerLabel(), MedewerkerVerplichtLabel);
            Assert.IsTrue(found, "No afdeling without a groepsmailbox (with attached medewerkers) found in test environment");

            await Step("Attempt to submit without selecting a medewerker");
            await Page.GetContactverzoekDoorsturenButton().ClickAsync();

            await Step("Verify the medewerker combobox reports a required-field validation error");
            await Expect(Page.GetAfdelingGroepMedewerkerCombobox()).ToHaveJSPropertyAsync("validity.valueMissing", true);

            await Step("Verify no success toast was shown");
            await Expect(Page.GetSuccessToast("succesvol doorgestuurd")).Not.ToBeVisibleAsync();
        }

        [TestMethod("Doorsturen naar afdeling zonder groepsmailbox mét medewerker wordt toegestaan")]
        public async Task User_CanForward_WhenAfdelingZonderMailboxWithMedewerkerSelected()
        {
            var onderwerp = $"Test_Verplicht_AfdToegestaan_{Guid.NewGuid().ToString()[..8]}";
            await SetupContactverzoek(onderwerp);
            await NavigateToContactverzoekAndOpenDoorsturenTab(onderwerp);

            await Step("Find an afdeling without a groepsmailbox (with medewerkers attached)");
            var found = await SelectOptionUntilMedewerkerLabelIsAsync(
                Page.GetAfdelingSelect(), Page.GetAfdelingMedewerkerLabel(), MedewerkerVerplichtLabel);
            Assert.IsTrue(found, "No afdeling without a groepsmailbox (with attached medewerkers) found in test environment");

            await Step("Select the required medewerker");
            await SelectFirstNestedMedewerker(Page.GetAfdelingGroepMedewerkerCombobox());

            await Step("Submit the forward form");
            await Page.GetContactverzoekDoorsturenButton().ClickAsync();

            await Step("Verify success toast");
            await Expect(Page.GetSuccessToast("succesvol doorgestuurd")).ToBeVisibleAsync();
        }

        [TestMethod("Doorsturen naar afdeling mét groepsmailbox zonder medewerker blijft toegestaan")]
        public async Task User_CanForward_WhenAfdelingMetMailboxAndNoMedewerkerSelected()
        {
            var onderwerp = $"Test_Verplicht_AfdMetMailbox_{Guid.NewGuid().ToString()[..8]}";
            await SetupContactverzoek(onderwerp);
            await NavigateToContactverzoekAndOpenDoorsturenTab(onderwerp);

            await Step("Find an afdeling with a groepsmailbox and medewerkers attached");
            var found = await SelectOptionUntilMedewerkerLabelIsAsync(
                Page.GetAfdelingSelect(), Page.GetAfdelingMedewerkerLabel(), MedewerkerOptioneelLabel);
            Assert.IsTrue(found, "No afdeling with a groepsmailbox (and attached medewerkers) found in test environment");

            await Step("Submit the forward form without selecting a medewerker");
            await Page.GetContactverzoekDoorsturenButton().ClickAsync();

            await Step("Verify success toast — unchanged existing behavior");
            await Expect(Page.GetSuccessToast("succesvol doorgestuurd")).ToBeVisibleAsync();
        }

        [TestMethod("Doorsturen naar groep zonder groepsmailbox en zonder medewerker wordt geweigerd")]
        public async Task User_CannotForward_WhenGroepZonderMailboxAndNoMedewerkerSelected()
        {
            var onderwerp = $"Test_Verplicht_GrpGeweigerd_{Guid.NewGuid().ToString()[..8]}";
            await SetupContactverzoek(onderwerp);
            await NavigateToContactverzoekAndOpenDoorsturenTab(onderwerp);

            await Step("Select 'Groep' mode");
            await Page.GetDoorsturenGroepRadio().ClickAsync();
            await Expect(Page.GetGroepSelect()).ToBeVisibleAsync();

            await Step("Find a groep without a groepsmailbox (with medewerkers attached)");
            var found = await SelectOptionUntilMedewerkerLabelIsAsync(
                Page.GetGroepSelect(), Page.GetGroepMedewerkerLabel(), MedewerkerVerplichtLabel);
            Assert.IsTrue(found, "No groep without a groepsmailbox (with attached medewerkers) found in test environment");

            await Step("Attempt to submit without selecting a medewerker");
            await Page.GetContactverzoekDoorsturenButton().ClickAsync();

            await Step("Verify the medewerker combobox reports a required-field validation error");
            await Expect(Page.GetGroepMedewerkerCombobox()).ToHaveJSPropertyAsync("validity.valueMissing", true);

            await Step("Verify no success toast was shown");
            await Expect(Page.GetSuccessToast("succesvol doorgestuurd")).Not.ToBeVisibleAsync();
        }

        // === Task #542: Doorstuurscherm toont groepsmailbox-status ===

        [TestMethod("Afdeling zonder groepsmailbox maakt medewerkerveld verplicht")]
        public async Task Doorstuurscherm_AfdelingZonderGroepsmailbox_MaaktMedewerkerveldVerplicht()
        {
            var onderwerp = $"Test_Scherm_AfdZonder_{Guid.NewGuid().ToString()[..8]}";
            await SetupContactverzoek(onderwerp);
            await NavigateToContactverzoekAndOpenDoorsturenTab(onderwerp);

            await Step("Find an afdeling without a groepsmailbox (with medewerkers attached)");
            var found = await SelectOptionUntilMedewerkerLabelIsAsync(
                Page.GetAfdelingSelect(), Page.GetAfdelingMedewerkerLabel(), MedewerkerVerplichtLabel);
            Assert.IsTrue(found, "No afdeling without a groepsmailbox (with attached medewerkers) found in test environment");

            await Step("Verify the medewerkerveld label reads 'Medewerker' (required)");
            await Expect(Page.GetAfdelingMedewerkerLabel()).ToHaveTextAsync(MedewerkerVerplichtLabel);
            await Expect(Page.GetAfdelingGroepMedewerkerCombobox()).ToHaveJSPropertyAsync("required", true);

            await Step("Verify the toelichting explains why the field is required");
            await Expect(Page.GetGroepsmailboxToelichtingAlert()).ToContainTextAsync(
                "heeft geen eigen e-mailadres, daarom is het verplicht om een medewerker te selecteren");
        }

        [TestMethod("Afdeling met groepsmailbox laat medewerkerveld optioneel")]
        public async Task Doorstuurscherm_AfdelingMetGroepsmailbox_LaatMedewerkerveldOptioneel()
        {
            var onderwerp = $"Test_Scherm_AfdMet_{Guid.NewGuid().ToString()[..8]}";
            await SetupContactverzoek(onderwerp);
            await NavigateToContactverzoekAndOpenDoorsturenTab(onderwerp);

            await Step("Find an afdeling with a groepsmailbox and medewerkers attached");
            var found = await SelectOptionUntilMedewerkerLabelIsAsync(
                Page.GetAfdelingSelect(), Page.GetAfdelingMedewerkerLabel(), MedewerkerOptioneelLabel);
            Assert.IsTrue(found, "No afdeling with a groepsmailbox (and attached medewerkers) found in test environment");

            await Step("Verify the medewerkerveld label reads 'Medewerker (optioneel)'");
            await Expect(Page.GetAfdelingMedewerkerLabel()).ToHaveTextAsync(MedewerkerOptioneelLabel);
            await Expect(Page.GetAfdelingGroepMedewerkerCombobox()).ToHaveJSPropertyAsync("required", false);

            await Step("Verify no toelichting alert is shown");
            await Expect(Page.GetGroepsmailboxToelichtingAlert()).Not.ToBeVisibleAsync();
        }

        [TestMethod("Groep zonder groepsmailbox maakt medewerkerveld verplicht")]
        public async Task Doorstuurscherm_GroepZonderGroepsmailbox_MaaktMedewerkerveldVerplicht()
        {
            var onderwerp = $"Test_Scherm_GrpZonder_{Guid.NewGuid().ToString()[..8]}";
            await SetupContactverzoek(onderwerp);
            await NavigateToContactverzoekAndOpenDoorsturenTab(onderwerp);

            await Step("Select 'Groep' mode");
            await Page.GetDoorsturenGroepRadio().ClickAsync();
            await Expect(Page.GetGroepSelect()).ToBeVisibleAsync();

            await Step("Find a groep without a groepsmailbox (with medewerkers attached)");
            var found = await SelectOptionUntilMedewerkerLabelIsAsync(
                Page.GetGroepSelect(), Page.GetGroepMedewerkerLabel(), MedewerkerVerplichtLabel);
            Assert.IsTrue(found, "No groep without a groepsmailbox (with attached medewerkers) found in test environment");

            await Step("Verify the medewerkerveld label reads 'Medewerker' (required)");
            await Expect(Page.GetGroepMedewerkerLabel()).ToHaveTextAsync(MedewerkerVerplichtLabel);
            await Expect(Page.GetGroepMedewerkerCombobox()).ToHaveJSPropertyAsync("required", true);

            await Step("Verify the toelichting explains why the field is required");
            await Expect(Page.GetGroepsmailboxToelichtingAlert()).ToContainTextAsync(
                "heeft geen eigen e-mailadres, daarom is het verplicht om een medewerker te selecteren");
        }

        [TestMethod("Afdeling zonder groepsmailbox en zonder beschikbare medewerkers toont blokkerende melding")]
        public async Task Doorstuurscherm_AfdelingZonderGroepsmailboxEnZonderMedewerkers_ToontBlokkerendeMelding()
        {
            if (string.IsNullOrEmpty(TestDataConstants.Groepsmailbox.AfdelingZonderMailboxEnZonderMedewerkersKey))
            {
                Assert.Inconclusive(
                    "AfdelingZonderMailboxEnZonderMedewerkersKey is not configured. " +
                    "Set this constant to an afdeling without a groepsmailbox and without any linked medewerkers.");
            }

            var onderwerp = $"Test_Scherm_Blokkerend_{Guid.NewGuid().ToString()[..8]}";
            await SetupContactverzoek(onderwerp);
            await NavigateToContactverzoekAndOpenDoorsturenTab(onderwerp);

            await Step("Select the afdeling without a groepsmailbox and without available medewerkers");
            await Page.GetAfdelingSelect().SelectOptionAsync(new SelectOptionValue
            {
                Value = TestDataConstants.Groepsmailbox.AfdelingZonderMailboxEnZonderMedewerkersKey
            });

            await Step("Verify a blocking warning message is shown instead of an unreachable required field");
            await Expect(Page.GetGroepsmailboxBlokkerendeMelding()).ToContainTextAsync(
                "geen medewerkers beschikbaar binnen deze afdeling");

            await Step("Verify no medewerkerveld is rendered");
            await Expect(Page.GetAfdelingGroepMedewerkerCombobox()).Not.ToBeVisibleAsync();
        }

        // === Task #543: Duidelijke foutafhandeling (UI-observeerbaar deel) ===

        [TestMethod("Doorsturen zonder enige resolveerbare e-mail toont geen vals-positief succesbericht")]
        public async Task User_ForwardWithNoResolvableEmail_ShowsExplicitFailureNotFalseSuccess()
        {
            if (string.IsNullOrEmpty(TestDataConstants.Groepsmailbox.AfdelingZonderMailboxMetMedewerkerZonderEmailSearchQuery))
            {
                Assert.Inconclusive(
                    "AfdelingZonderMailboxMetMedewerkerZonderEmailSearchQuery is not configured. " +
                    "Set this constant to a search query matching a medewerker without a valid email address, " +
                    "linked to an afdeling without a groepsmailbox.");
            }

            var onderwerp = $"Test_GeenNotificatie_{Guid.NewGuid().ToString()[..8]}";
            await SetupContactverzoek(onderwerp);
            await NavigateToContactverzoekAndOpenDoorsturenTab(onderwerp);

            await Step("Find an afdeling without a groepsmailbox (with medewerkers attached)");
            var found = await SelectOptionUntilMedewerkerLabelIsAsync(
                Page.GetAfdelingSelect(), Page.GetAfdelingMedewerkerLabel(), MedewerkerVerplichtLabel);
            Assert.IsTrue(found, "No afdeling without a groepsmailbox (with attached medewerkers) found in test environment");

            await Step("Search for and select the medewerker without a valid email address");
            var combobox = Page.GetAfdelingGroepMedewerkerCombobox();
            await combobox.FillAsync(TestDataConstants.Groepsmailbox.AfdelingZonderMailboxMetMedewerkerZonderEmailSearchQuery);
            var listbox = Page.Locator("#afdeling-groep-medewerker-combobox-listbox");
            await Expect(listbox).ToBeVisibleAsync();
            await listbox.GetByRole(AriaRole.Option).First.ClickAsync();

            await Step("Submit the forward form");
            await Page.GetContactverzoekDoorsturenButton().ClickAsync();

            await Step("Verify no false-positive success toast is shown");
            await Expect(Page.GetSuccessToast("Contactverzoek succesvol doorgestuurd")).Not.ToBeVisibleAsync();

            await Step("Verify an explicit failure is communicated instead");
            await Expect(Page.GetErrorToast()).ToBeVisibleAsync();
        }

        [TestMethod("Succesvolle resolutie blijft ongewijzigd gemeld")]
        public async Task User_ForwardWithResolvableEmail_ShowsUnchangedSuccessMessage()
        {
            var onderwerp = $"Test_OngewijzigdSucces_{Guid.NewGuid().ToString()[..8]}";
            await SetupContactverzoek(onderwerp);
            await NavigateToContactverzoekAndOpenDoorsturenTab(onderwerp);

            await Step("Select 'Groep' mode and pick the first real groep");
            await Page.GetDoorsturenGroepRadio().ClickAsync();
            await Page.GetGroepSelect().SelectOptionAsync(new SelectOptionValue { Index = 1 });

            await Step("Submit the forward form");
            await Page.GetContactverzoekDoorsturenButton().ClickAsync();

            await Step("Verify the exact, unchanged success message is shown");
            await Expect(Page.GetByRole(AriaRole.Status).Filter(new() { HasText = "Contactverzoek succesvol doorgestuurd" }))
                .ToBeVisibleAsync();
        }

        // === Edge cases (Task #550) ===

        [TestMethod("Wisselen tussen afdelingen werkt verplicht/optioneel-status live bij")]
        public async Task Doorstuurscherm_WisselenTussenAfdelingen_UpdatesMedewerkerveldStatusLive()
        {
            var onderwerp = $"Test_Reactief_{Guid.NewGuid().ToString()[..8]}";
            await SetupContactverzoek(onderwerp);
            await NavigateToContactverzoekAndOpenDoorsturenTab(onderwerp);

            await Step("Find an afdeling with a groepsmailbox and medewerkers attached");
            var foundMet = await SelectOptionUntilMedewerkerLabelIsAsync(
                Page.GetAfdelingSelect(), Page.GetAfdelingMedewerkerLabel(), MedewerkerOptioneelLabel);
            Assert.IsTrue(foundMet, "No afdeling with a groepsmailbox (and attached medewerkers) found in test environment");
            await Expect(Page.GetAfdelingMedewerkerLabel()).ToHaveTextAsync(MedewerkerOptioneelLabel);

            await Step("Switch to an afdeling without a groepsmailbox (with medewerkers attached)");
            var foundZonder = await SelectOptionUntilMedewerkerLabelIsAsync(
                Page.GetAfdelingSelect(), Page.GetAfdelingMedewerkerLabel(), MedewerkerVerplichtLabel);
            Assert.IsTrue(foundZonder, "No afdeling without a groepsmailbox (with attached medewerkers) found in test environment");

            await Step("Verify the medewerkerveld status updated live to required, not just on first selection");
            await Expect(Page.GetAfdelingMedewerkerLabel()).ToHaveTextAsync(MedewerkerVerplichtLabel);
            await Expect(Page.GetAfdelingGroepMedewerkerCombobox()).ToHaveJSPropertyAsync("required", true);
        }

        [TestMethod("Afdeling met alleen-whitespace e-mailadres wordt behandeld als geen groepsmailbox")]
        public async Task Afdeling_MetWhitespaceEmail_WordtBehandeldAlsGeenGroepsmailbox()
        {
            if (string.IsNullOrEmpty(TestDataConstants.Groepsmailbox.AfdelingMetWhitespaceEmailKey))
            {
                Assert.Inconclusive(
                    "AfdelingMetWhitespaceEmailKey is not configured. " +
                    "Set this constant to an afdeling whose Email field contains only whitespace.");
            }

            var onderwerp = $"Test_Whitespace_{Guid.NewGuid().ToString()[..8]}";
            await SetupContactverzoek(onderwerp);
            await NavigateToContactverzoekAndOpenDoorsturenTab(onderwerp);

            await Step("Select the afdeling with a whitespace-only Email");
            await Page.GetAfdelingSelect().SelectOptionAsync(new SelectOptionValue
            {
                Value = TestDataConstants.Groepsmailbox.AfdelingMetWhitespaceEmailKey
            });

            await Step("Verify it is treated identically to a fully absent groepsmailbox");
            await Expect(Page.GetAfdelingMedewerkerLabel()).ToHaveTextAsync(MedewerkerVerplichtLabel);
            await Expect(Page.GetAfdelingGroepMedewerkerCombobox()).ToHaveJSPropertyAsync("required", true);
        }

        // === Private helpers ===

        private async Task<Guid> SetupContactverzoek(string onderwerp)
        {
            await Step("Setup test data via API");
            var uuid = await TestDataHelper.CreateContactverzoek(onderwerp, attachZaak: false);
            RegisterCleanup(async () => await TestDataHelper.DeleteContactverzoekAsync(uuid.ToString()));
            return uuid;
        }

        private async Task NavigateToContactverzoekAndOpenDoorsturenTab(string onderwerp)
        {
            await Step("Navigate to home page");
            await SafeGotoAsync("/");

            await Step($"Click on contactverzoek '{onderwerp}'");
            var detailsLink = Page.GetDetailsLink(onderwerp);
            await detailsLink.WaitForAsync(new() { State = WaitForSelectorState.Visible });
            await detailsLink.ClickAsync();

            await Step("Wait for detail page to load");
            var doorsturenTab = Page.Locator("#label-contactmomentDoorsturen");
            await Expect(doorsturenTab).ToBeVisibleAsync();

            await Step("Click 'Doorsturen' tab");
            await doorsturenTab.ClickAsync();

            await Step("Wait for doorsturen form to load (radio buttons visible)");
            await Expect(Page.GetDoorsturenAfdelingRadio()).ToBeVisibleAsync();
        }

        /// <summary>
        /// Selects the first available option from an already-loaded, client-side-filtered
        /// medewerker combobox (the afdeling/groep-scoped combobox, not the global search one) by
        /// focusing it — which reveals the full option list without needing a matching search query.
        /// </summary>
        private async Task SelectFirstNestedMedewerker(ILocator combobox)
        {
            await combobox.ClickAsync();

            var listboxId = $"{await combobox.GetAttributeAsync("id")}-listbox";
            var listbox = Page.Locator($"#{listboxId}");
            await Expect(listbox).ToBeVisibleAsync();
            await listbox.GetByRole(AriaRole.Option).First.ClickAsync();
        }

        private async Task SelectFirstSecondaryOption()
        {
            var secondaryPicker = Page.GetSecondaryPicker();
            var isVisible = await secondaryPicker.IsVisibleAsync();
            if (isVisible)
            {
                await secondaryPicker.SelectOptionAsync(new SelectOptionValue { Index = 1 });
            }
            // If secondaryPicker is not visible, there's only one option and it's auto-selected via hidden input
        }

        private async Task SafeGotoAsync(string url)
        {
            PlaywrightException? lastException = null;
            for (var attempt = 1; attempt <= 3; attempt++)
            {
                try
                {
                    await Page.GotoAsync(url);
                    return;
                }
                catch (PlaywrightException ex)
                {
                    lastException = ex;
                    if (attempt < 3) await Task.Delay(1500);
                }
            }
            throw new InvalidOperationException(
                $"Failed to navigate to '{url}' after 3 attempts.",
                lastException);
        }

        /// <summary>
        /// Iterates through select options (skipping the placeholder at index 0) until the
        /// medewerkerveld label matches the expected text ("Medewerker" or "Medewerker (optioneel)"),
        /// which reflects the selected afdeling/groep's groepsmailbox status. Returns true if found.
        /// </summary>
        private async Task<bool> SelectOptionUntilMedewerkerLabelIsAsync(
            ILocator selectLocator, ILocator medewerkerLabel, string expectedLabelText, int maxAttempts = 10)
        {
            for (var i = 1; i <= maxAttempts; i++)
            {
                try
                {
                    await selectLocator.SelectOptionAsync(new SelectOptionValue { Index = i });
                }
                catch (PlaywrightException)
                {
                    break;
                }

                try
                {
                    await Expect(medewerkerLabel).ToHaveTextAsync(expectedLabelText, new() { Timeout = 3000 });
                    await Step($"Found option at index {i} with medewerkerveld label '{expectedLabelText}'");
                    return true;
                }
                catch (PlaywrightException)
                {
                    // This option didn't match, try the next one
                }
            }
            return false;
        }
    }
}
