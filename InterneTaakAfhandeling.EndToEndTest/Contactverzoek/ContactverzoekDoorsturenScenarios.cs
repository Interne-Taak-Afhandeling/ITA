using InterneTaakAfhandeling.EndToEndTest.Infrastructure;
using Microsoft.Playwright;

namespace InterneTaakAfhandeling.EndToEndTest.Contactverzoek
{
    /// <summary>
    /// E2E tests for doorsturen via medewerker/afdeling/groep (Feature #349, Task #438).
    /// Phase 2 verification: covers all Gherkin scenarios from Tasks #404 and #405
    /// against the deployed test environment.
    /// </summary>
    [TestClass]
    [DoNotParallelize]
    public class ContactverzoekDoorsturenScenarios : ITAPlaywrightTest
    {
        // === Task #405 Frontend Scenarios ===

        [TestMethod("Standaard is de modus Afdeling geselecteerd")]
        public async Task User_ForwardForm_DefaultsToAfdelingMode()
        {
            var onderwerp = $"Test_Forward_Default_{Guid.NewGuid().ToString()[..8]}";
            var contactmomentUuid = await SetupContactverzoek(onderwerp);

            await NavigateToContactverzoekAndOpenDoorsturenTab(onderwerp);

            await Step("Verify 'Afdeling' radio is checked by default");
            await Expect(Page.GetDoorsturenAfdelingRadio()).ToBeCheckedAsync();

            await Step("Verify afdeling select is visible");
            await Expect(Page.GetAfdelingSelect()).ToBeVisibleAsync();
        }

        [TestMethod("Medewerker selecteren en filteren op naam")]
        public async Task User_CanSearchMedewerker_ByName()
        {
            var onderwerp = $"Test_Forward_Search_{Guid.NewGuid().ToString()[..8]}";
            var contactmomentUuid = await SetupContactverzoek(onderwerp);

            await NavigateToContactverzoekAndOpenDoorsturenTab(onderwerp);

            await Step("Select 'Medewerker' mode");
            await Page.GetDoorsturenMedewerkerRadio().ClickAsync();

            await Step("Type search query in medewerker combobox");
            var combobox = Page.GetMedewerkerCombobox();
            await combobox.FillAsync(TestDataConstants.Doorsturen.TestMedewerkerSearchQuery);

            await Step("Verify search results appear in listbox");
            var listbox = Page.Locator("#medewerker-combobox-listbox");
            await Expect(listbox).ToBeVisibleAsync();
            var options = listbox.GetByRole(AriaRole.Option);
            var count = await options.CountAsync();
            Assert.IsTrue(count > 0, "Expected at least one medewerker search result");
        }

        [TestMethod("Na medewerker-selectie verschijnt de secundaire afdeling/groep-picker")]
        public async Task User_CanSeeSecondaryPicker_AfterMedewerkerSelection()
        {
            var onderwerp = $"Test_Forward_Secondary_{Guid.NewGuid().ToString()[..8]}";
            var contactmomentUuid = await SetupContactverzoek(onderwerp);

            await NavigateToContactverzoekAndOpenDoorsturenTab(onderwerp);

            await Step("Select 'Medewerker' mode and search");
            await Page.GetDoorsturenMedewerkerRadio().ClickAsync();
            await SearchAndSelectFirstMedewerker();

            await Step("Verify secondary picker (afdeling/groep) is visible");
            await Expect(Page.GetSecondaryPicker()).ToBeVisibleAsync();
        }

        [TestMethod("Modus Afdeling met optionele medewerker-picker")]
        public async Task User_CanSeeOptionalMedewerkerPicker_InAfdelingMode()
        {
            var onderwerp = $"Test_Forward_AfdelingMdw_{Guid.NewGuid().ToString()[..8]}";
            var contactmomentUuid = await SetupContactverzoek(onderwerp);

            await NavigateToContactverzoekAndOpenDoorsturenTab(onderwerp);

            await Step("Verify Afdeling mode is default");
            await Expect(Page.GetDoorsturenAfdelingRadio()).ToBeCheckedAsync();

            await Step("Try afdeling options until one shows the optional medewerker picker");
            var afdelingSelect = Page.GetAfdelingSelect();
            var medewerkerCombobox = Page.GetAfdelingGroepMedewerkerCombobox();
            var found = await SelectOptionUntilElementVisible(afdelingSelect, medewerkerCombobox);

            Assert.IsTrue(found, "No afdeling with attached medewerkers found in test environment");
        }

        [TestMethod("Modus Groep met optionele medewerker-picker")]
        public async Task User_CanSeeOptionalMedewerkerPicker_InGroepMode()
        {
            var onderwerp = $"Test_Forward_GroepMdw_{Guid.NewGuid().ToString()[..8]}";
            var contactmomentUuid = await SetupContactverzoek(onderwerp);

            await NavigateToContactverzoekAndOpenDoorsturenTab(onderwerp);

            await Step("Select 'Groep' mode");
            await Page.GetDoorsturenGroepRadio().ClickAsync();

            await Step("Try groep options until one shows the optional medewerker picker");
            var groepSelect = Page.GetGroepSelect();
            await Expect(groepSelect).ToBeVisibleAsync();
            var medewerkerCombobox = Page.GetGroepMedewerkerCombobox();
            var found = await SelectOptionUntilElementVisible(groepSelect, medewerkerCombobox);

            Assert.IsTrue(found, "No groep with attached medewerkers found in test environment");
        }

        [TestMethod("Volgorde selectiemodi: Afdeling, Groep, Medewerker")]
        public async Task User_SeesRadioOptionsInCorrectOrder()
        {
            var onderwerp = $"Test_Forward_Order_{Guid.NewGuid().ToString()[..8]}";
            var contactmomentUuid = await SetupContactverzoek(onderwerp);

            await NavigateToContactverzoekAndOpenDoorsturenTab(onderwerp);

            await Step("Verify all three radio options are visible");
            await Expect(Page.GetDoorsturenAfdelingRadio()).ToBeVisibleAsync();
            await Expect(Page.GetDoorsturenGroepRadio()).ToBeVisibleAsync();
            await Expect(Page.GetDoorsturenMedewerkerRadio()).ToBeVisibleAsync();

            await Step("Verify order: Afdeling → Groep → Medewerker");
            var radios = Page.GetByRole(AriaRole.Radio);
            var labels = new List<string>();
            var count = await radios.CountAsync();
            for (var i = 0; i < count; i++)
            {
                var label = await radios.Nth(i).GetAttributeAsync("value");
                if (label != null) labels.Add(label);
            }

            var afdelingIdx = labels.IndexOf("Afdeling");
            var groepIdx = labels.IndexOf("Groep");
            var medewerkerIdx = labels.IndexOf("Medewerker");

            Assert.IsTrue(afdelingIdx >= 0, "Afdeling radio should be present");
            Assert.IsTrue(groepIdx >= 0, "Groep radio should be present");
            Assert.IsTrue(medewerkerIdx >= 0, "Medewerker radio should be present");
            Assert.IsTrue(afdelingIdx < groepIdx, "Afdeling should come before Groep");
            Assert.IsTrue(groepIdx < medewerkerIdx, "Groep should come before Medewerker");
        }

        [TestMethod("Contactverzoek doorsturen via modus Medewerker")]
        public async Task User_CanForwardContactverzoek_ViaMedewerkerMode()
        {
            var onderwerp = $"Test_Forward_Mdw_{Guid.NewGuid().ToString()[..8]}";
            var contactmomentUuid = await SetupContactverzoek(onderwerp);

            await NavigateToContactverzoekAndOpenDoorsturenTab(onderwerp);

            await Step("Select 'Medewerker' mode");
            await Page.GetDoorsturenMedewerkerRadio().ClickAsync();

            await Step("Search and select a medewerker");
            await SearchAndSelectFirstMedewerker();

            await Step("Select secondary afdeling/groep");
            await SelectFirstSecondaryOption();

            await Step("Submit the forward form");
            await Page.GetContactverzoekDoorsturenButton().ClickAsync();

            await Step("Verify success toast");
            await Expect(Page.GetByRole(AriaRole.Status).Filter(new() { HasText = "doorgestuurd" })).ToBeVisibleAsync();
        }

        [TestMethod("Contactverzoek doorsturen via modus Groep")]
        public async Task User_CanForwardContactverzoek_ViaGroepMode()
        {
            var onderwerp = $"Test_Forward_Groep_{Guid.NewGuid().ToString()[..8]}";
            var contactmomentUuid = await SetupContactverzoek(onderwerp);

            await NavigateToContactverzoekAndOpenDoorsturenTab(onderwerp);

            await Step("Select 'Groep' mode");
            await Page.GetDoorsturenGroepRadio().ClickAsync();

            await Step("Select a groep from dropdown (first non-placeholder)");
            await Page.GetGroepSelect().SelectOptionAsync(new SelectOptionValue { Index = 1 });

            await Step("Submit the forward form");
            await Page.GetContactverzoekDoorsturenButton().ClickAsync();

            await Step("Verify success toast");
            await Expect(Page.GetByRole(AriaRole.Status).Filter(new() { HasText = "doorgestuurd" })).ToBeVisibleAsync();
        }

        [TestMethod("Handmatige e-mailinvoer is niet meer mogelijk")]
        public async Task User_CannotSeeEmailInputField_InForwardForm()
        {
            var onderwerp = $"Test_Forward_NoEmail_{Guid.NewGuid().ToString()[..8]}";
            var contactmomentUuid = await SetupContactverzoek(onderwerp);

            await NavigateToContactverzoekAndOpenDoorsturenTab(onderwerp);

            await Step("Verify no free-text email input field exists in the forward form");
            var emailInput = Page.Locator("input[type='email']");
            await Expect(emailInput).Not.ToBeVisibleAsync();

            await Step("Verify no field labeled 'E-mailadres' exists in the form");
            var emailLabel = Page.GetByLabel("E-mailadres");
            await Expect(emailLabel).Not.ToBeVisibleAsync();
        }

        [TestMethod("Zoeken zonder resultaten toont geen opties")]
        public async Task User_SeesNoOptions_WhenMedewerkerSearchHasNoResults()
        {
            var onderwerp = $"Test_Forward_NoResults_{Guid.NewGuid().ToString()[..8]}";
            var contactmomentUuid = await SetupContactverzoek(onderwerp);

            await NavigateToContactverzoekAndOpenDoorsturenTab(onderwerp);

            await Step("Select 'Medewerker' mode");
            await Page.GetDoorsturenMedewerkerRadio().ClickAsync();

            await Step("Type a query that returns no results");
            var combobox = Page.GetMedewerkerCombobox();
            await combobox.FillAsync(TestDataConstants.Doorsturen.TestMedewerkerSearchQueryNoResults);

            await Step("Verify no listbox/options are shown");
            var listbox = Page.Locator("#medewerker-combobox-listbox");
            await Expect(listbox).Not.ToBeVisibleAsync();
        }

        // === Task #404 Backend Scenarios (verified via UI E2E) ===

        [TestMethod("Contactverzoek doorsturen met medewerker en gekoppelde afdeling")]
        public async Task User_CanForwardContactverzoek_ViaMedewerkerWithAfdeling()
        {
            var onderwerp = $"Test_Forward_MdwAfd_{Guid.NewGuid().ToString()[..8]}";
            var contactmomentUuid = await SetupContactverzoek(onderwerp);

            await NavigateToContactverzoekAndOpenDoorsturenTab(onderwerp);

            await Step("Select 'Medewerker' mode");
            await Page.GetDoorsturenMedewerkerRadio().ClickAsync();

            await Step("Search and select a medewerker");
            await SearchAndSelectFirstMedewerker();

            await Step("Select an afdeling from secondary picker");
            var secondaryPicker = Page.GetSecondaryPicker();
            await Expect(secondaryPicker).ToBeVisibleAsync();
            await secondaryPicker.SelectOptionAsync(new SelectOptionValue { Index = 1 });

            await Step("Submit the forward form");
            await Page.GetContactverzoekDoorsturenButton().ClickAsync();

            await Step("Verify success toast (confirms email notification sent or forwarded)");
            await Expect(Page.GetByRole(AriaRole.Status).Filter(new() { HasText = "doorgestuurd" })).ToBeVisibleAsync();
        }

        [TestMethod("Validatie faalt zonder secundaire afdeling of groep bij modus medewerker")]
        public async Task User_CannotSubmitForwardForm_InMedewerkerMode_WithoutAnySelection()
        {
            var onderwerp = $"Test_Forward_NoSec_{Guid.NewGuid().ToString()[..8]}";
            var contactmomentUuid = await SetupContactverzoek(onderwerp);

            await NavigateToContactverzoekAndOpenDoorsturenTab(onderwerp);

            await Step("Select 'Medewerker' mode");
            await Page.GetDoorsturenMedewerkerRadio().ClickAsync();

            await Step("Search and select a medewerker (but do NOT select secondary)");
            await SearchAndSelectFirstMedewerker();

            await Step("Verify secondary picker is visible with default placeholder");
            var secondaryPicker = Page.GetSecondaryPicker();
            await Expect(secondaryPicker).ToBeVisibleAsync();

            await Step("Attempt to submit without selecting secondary option");
            await Page.GetContactverzoekDoorsturenButton().ClickAsync();

            await Step("Verify form validation blocks submission — secondary picker reports validity error");
            await Expect(secondaryPicker).ToHaveJSPropertyAsync("validity.valueMissing", true);

            await Step("Verify no success toast was shown");
            await Expect(Page.GetByRole(AriaRole.Status).Filter(new() { HasText = "doorgestuurd" })).Not.ToBeVisibleAsync();
        }

        [TestMethod("Doorsturen wanneer medewerker geen e-mailadres heeft")]
        public async Task User_CanForwardContactverzoek_WhenMedewerkerHasNoEmail()
        {
            if (string.IsNullOrEmpty(TestDataConstants.Doorsturen.TestMedewerkerNoEmailSearchQuery))
            {
                Assert.Inconclusive(
                    "TestMedewerkerNoEmailSearchQuery is not configured. " +
                    "Set this constant to a medewerker without an email address in the test objectenregister.");
            }

            var onderwerp = $"Test_Forward_NoEmail_Mdw_{Guid.NewGuid().ToString()[..8]}";
            var contactmomentUuid = await SetupContactverzoek(onderwerp);

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

            await Step("Select secondary option");
            await SelectFirstSecondaryOption();

            await Step("Submit the forward form");
            await Page.GetContactverzoekDoorsturenButton().ClickAsync();

            await Step("Verify toast indicates forwarding succeeded (with notification warning)");
            await Expect(Page.GetByRole(AriaRole.Status).Filter(new() { HasText = "doorgestuurd" })).ToBeVisibleAsync();
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

        private async Task SearchAndSelectFirstMedewerker()
        {
            var combobox = Page.GetMedewerkerCombobox();
            await combobox.FillAsync(TestDataConstants.Doorsturen.TestMedewerkerSearchQuery);

            await Step("Wait for search results to appear");
            var listbox = Page.Locator("#medewerker-combobox-listbox");
            await Expect(listbox).ToBeVisibleAsync();

            await Step("Select the first medewerker option");
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
        /// Iterates through select options (skipping the placeholder at index 0) until
        /// the target element becomes visible. Returns true if found, false if no option triggers it.
        /// </summary>
        private async Task<bool> SelectOptionUntilElementVisible(ILocator selectLocator, ILocator targetElement, int maxAttempts = 10)
        {
            for (var i = 1; i <= maxAttempts; i++)
            {
                try
                {
                    await selectLocator.SelectOptionAsync(new SelectOptionValue { Index = i });
                }
                catch (PlaywrightException)
                {
                    // No more options available
                    break;
                }

                try
                {
                    await Expect(targetElement).ToBeVisibleAsync(new() { Timeout = 5000 });
                    await Step($"Found option at index {i} with medewerkers attached");
                    return true;
                }
                catch (PlaywrightException)
                {
                    // This option didn't trigger the element, try next
                }
            }
            return false;
        }
    }
}
