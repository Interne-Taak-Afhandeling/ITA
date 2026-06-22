using InterneTaakAfhandeling.EndToEndTest.Infrastructure;
using Microsoft.Playwright;

namespace InterneTaakAfhandeling.EndToEndTest.Dashboard
{
    [TestClass]
    [DoNotParallelize]
    public class ContactverzoekDoorsturenScenarios : ITAPlaywrightTest
    {
        // ── helpers ──────────────────────────────────────────────────────────

        private async Task<Guid> SetupAndNavigateToDoorsturenTab(string onderwerp)
        {
            var uuid = await TestDataHelper.CreateContactverzoek(onderwerp, attachZaak: false);
            RegisterCleanup(async () => await TestDataHelper.DeleteContactverzoekAsync(uuid.ToString()));

            await Step("Navigate to home and open the contactverzoek detail page");
            await SafeGotoAsync("/");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            var link = Page.GetDetailsLink(onderwerp);
            await link.WaitForAsync(new() { State = WaitForSelectorState.Visible });
            await link.ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("Click the 'Doorsturen' tab");
            await Page.GetDoorsturenTab().ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            return uuid;
        }

        private async Task SafeGotoAsync(string url)
        {
            Microsoft.Playwright.PlaywrightException? lastException = null;
            for (var attempt = 1; attempt <= 3; attempt++)
            {
                try { await Page.GotoAsync(url); return; }
                catch (Microsoft.Playwright.PlaywrightException ex)
                {
                    lastException = ex;
                    if (attempt < 3) await Task.Delay(1500);
                }
            }
            throw new InvalidOperationException(
                $"Failed to navigate to '{url}' after 3 attempts.", lastException);
        }

        private async Task<string> SelectFirstOption(ILocator select)
        {
            var firstOption = select.Locator("option:not([value=''])").First;
            await Expect(firstOption).ToBeAttachedAsync();
            var label = await firstOption.InnerTextAsync();
            var value = await firstOption.GetAttributeAsync("value") ?? label.Trim();
            await select.SelectOptionAsync(new[] { value });
            return label.Trim();
        }

        /// <summary>
        /// Iterates through the select options and picks the first one that causes the
        /// "Medewerker (optioneel)" picker to appear. Not all afdelingen/groepen have
        /// medewerkers in the test objectenregister, so a blind first-pick may not suffice.
        /// </summary>
        private async Task<string> SelectFirstOptionWithMedewerkerPicker(ILocator select)
        {
            var options = select.Locator("option:not([value=''])");
            await Expect(options.First).ToBeAttachedAsync();
            var count = await options.CountAsync();
            for (var i = 0; i < count; i++)
            {
                var option = options.Nth(i);
                var label = await option.InnerTextAsync();
                var value = await option.GetAttributeAsync("value") ?? label.Trim();
                await select.SelectOptionAsync(new[] { value });
                await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
                if (await Page.GetByText("Medewerker (optioneel)").IsVisibleAsync())
                    return label.Trim();
            }
            // Fall back to first option so the subsequent Expect() gives a clear failure message
            var fallback = options.First;
            var fallbackLabel = await fallback.InnerTextAsync();
            var fallbackValue = await fallback.GetAttributeAsync("value") ?? fallbackLabel.Trim();
            await select.SelectOptionAsync(new[] { fallbackValue });
            return fallbackLabel.Trim();
        }

        // ── tests ─────────────────────────────────────────────────────────────

        [TestMethod("Handmatige e-mailinvoer is niet meer mogelijk — vrij e-mailadresveld afwezig")]
        public async Task User_CannotSeeEmailInputField_InForwardForm()
        {
            var onderwerp = $"Test_Doorsturen_NoEmail_{Guid.NewGuid().ToString()[..8]}";
            await SetupAndNavigateToDoorsturenTab(onderwerp);

            await Step("Verify the free email input field is NOT present");
            await Expect(Page.GetDoorsturenEmailInput()).ToBeHiddenAsync();
        }

        [TestMethod("Standaard is de modus Afdeling geselecteerd")]
        public async Task User_ForwardForm_DefaultsToAfdelingMode()
        {
            var onderwerp = $"Test_Doorsturen_Default_{Guid.NewGuid().ToString()[..8]}";
            await SetupAndNavigateToDoorsturenTab(onderwerp);

            await Step("Verify 'Afdeling' radio is checked by default");
            await Expect(Page.GetDoorsturenAfdelingRadio()).ToBeCheckedAsync();

            await Step("Verify 'Groep' and 'Medewerker' radios are not checked");
            await Expect(Page.GetDoorsturenGroepRadio()).Not.ToBeCheckedAsync();
            await Expect(Page.GetDoorsturenMedewerkerRadio()).Not.ToBeCheckedAsync();

            await Step("Verify the afdeling select is visible (afdeling mode active)");
            await Expect(Page.GetDoorsturenAfdelingSelect()).ToBeVisibleAsync();
        }

        [TestMethod("Contactverzoek doorsturen via modus Afdeling")]
        public async Task User_CanForwardContactverzoek_ViaAfdelingMode()
        {
            var onderwerp = $"Test_Doorsturen_Afdeling_{Guid.NewGuid().ToString()[..8]}";
            await SetupAndNavigateToDoorsturenTab(onderwerp);

            await Step("Verify 'Afdeling' mode is active by default");
            await Expect(Page.GetDoorsturenAfdelingSelect()).ToBeVisibleAsync();

            await Step("Select the first available afdeling");
            await SelectFirstOption(Page.GetDoorsturenAfdelingSelect());

            await Step("Submit the forwarding form");
            await Page.GetDoorsturenSubmitButton().ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("Verify success notification is shown");
            await Expect(Page.GetDoorsturenToast("doorgestuurd")).ToBeVisibleAsync(new() { Timeout = 10000 });
        }

        [TestMethod("Contactverzoek doorsturen via modus Groep")]
        public async Task User_CanForwardContactverzoek_ViaGroepMode()
        {
            var onderwerp = $"Test_Doorsturen_Groep_{Guid.NewGuid().ToString()[..8]}";
            await SetupAndNavigateToDoorsturenTab(onderwerp);

            await Step("Select 'Groep' mode");
            await Page.GetDoorsturenGroepRadio().ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("Select the first available groep");
            await SelectFirstOption(Page.GetDoorsturenGroepSelect());

            await Step("Submit the forwarding form");
            await Page.GetDoorsturenSubmitButton().ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("Verify success notification is shown");
            await Expect(Page.GetDoorsturenToast("doorgestuurd")).ToBeVisibleAsync(new() { Timeout = 10000 });
        }

        [TestMethod("Modus Afdeling — optionele medewerker-picker verschijnt na afdeling selectie")]
        public async Task User_CanSeeOptionalMedewerkerPicker_InAfdelingMode()
        {
            var onderwerp = $"Test_Doorsturen_AfdelingMdw_{Guid.NewGuid().ToString()[..8]}";
            await SetupAndNavigateToDoorsturenTab(onderwerp);

            await Step("Select the first afdeling that has an optional medewerker picker");
            await SelectFirstOptionWithMedewerkerPicker(Page.GetDoorsturenAfdelingSelect());

            await Step("Verify optional medewerker combobox is visible and labeled 'Medewerker (optioneel)'");
            await Expect(Page.GetByText("Medewerker (optioneel)")).ToBeVisibleAsync(new() { Timeout = 10000 });
            await Expect(Page.GetDoorsturenAfdelingMedewerkerCombobox()).ToBeVisibleAsync();
        }

        [TestMethod("Modus Groep — optionele medewerker-picker verschijnt na groep selectie")]
        public async Task User_CanSeeOptionalMedewerkerPicker_InGroepMode()
        {
            var onderwerp = $"Test_Doorsturen_GroepMdw_{Guid.NewGuid().ToString()[..8]}";
            await SetupAndNavigateToDoorsturenTab(onderwerp);

            await Step("Select 'Groep' mode");
            await Page.GetDoorsturenGroepRadio().ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("Select the first groep that has an optional medewerker picker");
            await SelectFirstOptionWithMedewerkerPicker(Page.GetDoorsturenGroepSelect());

            await Step("Verify optional medewerker combobox is visible");
            await Expect(Page.GetByText("Medewerker (optioneel)")).ToBeVisibleAsync(new() { Timeout = 10000 });
            await Expect(Page.GetDoorsturenGroepMedewerkerCombobox()).ToBeVisibleAsync();
        }

        [TestMethod("Medewerker selecteren en filteren op naam — zoekresultaten worden getoond")]
        public async Task User_CanSearchMedewerker_ByName()
        {
            var onderwerp = $"Test_Doorsturen_MdwSearch_{Guid.NewGuid().ToString()[..8]}";
            await SetupAndNavigateToDoorsturenTab(onderwerp);

            await Step("Switch to 'Medewerker' mode");
            await Page.GetDoorsturenMedewerkerRadio().ClickAsync();

            await Step($"Type '{TestDataConstants.Doorsturen.TestMedewerkerSearchQuery}' in the medewerker combobox");
            await Page.GetDoorsturenMedewerkerCombobox()
                .FillAsync(TestDataConstants.Doorsturen.TestMedewerkerSearchQuery);

            await Step("Wait for server-side search results (debounce + API call)");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("Verify the listbox with search results is visible");
            var listbox = Page.GetByRole(AriaRole.Listbox);
            await Expect(listbox).ToBeVisibleAsync(new() { Timeout = 10000 });

            await Step("Verify at least one option is shown in the listbox");
            var firstOption = listbox.GetByRole(AriaRole.Option).First;
            await Expect(firstOption).ToBeVisibleAsync();
        }

        [TestMethod("Na medewerker-selectie verschijnt de secundaire afdeling/groep-picker")]
        public async Task User_CanSeeSecondaryPicker_AfterMedewerkerSelection()
        {
            var onderwerp = $"Test_Doorsturen_SecondaryPicker_{Guid.NewGuid().ToString()[..8]}";
            await SetupAndNavigateToDoorsturenTab(onderwerp);

            await Step("Switch to 'Medewerker' mode");
            await Page.GetDoorsturenMedewerkerRadio().ClickAsync();

            await Step($"Search for '{TestDataConstants.Doorsturen.TestMedewerkerSearchQuery}'");
            await Page.GetDoorsturenMedewerkerCombobox()
                .FillAsync(TestDataConstants.Doorsturen.TestMedewerkerSearchQuery);
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("Select the first result from the listbox");
            var firstOption = Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option).First;
            await Expect(firstOption).ToBeVisibleAsync(new() { Timeout = 10000 });
            await firstOption.ClickAsync();

            await Step("Verify the secondary afdeling/groep picker appears");
            var secondaryPickerOrAutoSelect = Page.Locator("#secondaryPicker, input[name='afdeling'], input[name='groep']");
            await Expect(secondaryPickerOrAutoSelect.First).ToBeAttachedAsync(new() { Timeout = 5000 });
        }

        [TestMethod("Contactverzoek doorsturen via modus Medewerker")]
        public async Task User_CanForwardContactverzoek_ViaMedewerkerMode()
        {
            var onderwerp = $"Test_Doorsturen_Medewerker_{Guid.NewGuid().ToString()[..8]}";
            await SetupAndNavigateToDoorsturenTab(onderwerp);

            await Step("Switch to 'Medewerker' mode");
            await Page.GetDoorsturenMedewerkerRadio().ClickAsync();

            await Step($"Search for '{TestDataConstants.Doorsturen.TestMedewerkerSearchQuery}'");
            await Page.GetDoorsturenMedewerkerCombobox()
                .FillAsync(TestDataConstants.Doorsturen.TestMedewerkerSearchQuery);
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("Select the first medewerker from the listbox");
            var listbox = Page.GetByRole(AriaRole.Listbox);
            await Expect(listbox).ToBeVisibleAsync(new() { Timeout = 10000 });
            var firstOption = listbox.GetByRole(AriaRole.Option).First;
            await firstOption.ClickAsync();

            await Step("Select secondary afdeling or groep if picker is shown (auto-selected when only one option)");
            var secondaryPicker = Page.GetDoorsturenSecondaryPicker();
            if (await secondaryPicker.IsVisibleAsync())
            {
                await secondaryPicker.SelectOptionAsync(new SelectOptionValue[] { new() { Index = 1 } });
            }

            await Step("Submit the forwarding form");
            await Page.GetDoorsturenSubmitButton().ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("Verify success notification is shown");
            await Expect(Page.GetDoorsturenToast("doorgestuurd")).ToBeVisibleAsync(new() { Timeout = 10000 });
        }

        [TestMethod("Zoeken zonder resultaten toont geen opties in de combobox")]
        public async Task User_SeesNoOptions_WhenMedewerkerSearchHasNoResults()
        {
            var onderwerp = $"Test_Doorsturen_NoResults_{Guid.NewGuid().ToString()[..8]}";
            await SetupAndNavigateToDoorsturenTab(onderwerp);

            await Step("Switch to 'Medewerker' mode");
            await Page.GetDoorsturenMedewerkerRadio().ClickAsync();

            await Step("Type a search query that matches no medewerker");
            await Page.GetDoorsturenMedewerkerCombobox().FillAsync("xyzxyzxyz999notexistent");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("Verify no listbox / options are rendered");
            await Expect(Page.GetByRole(AriaRole.Listbox)).ToBeHiddenAsync(new() { Timeout = 5000 });
        }

        [TestMethod("Doorsturen in medewerker-modus zonder e-mailadres medewerker — contactverzoek doorgestuurd")]
        [TestCategory("RequiresSpecificTestData")]
        public async Task User_CanForwardContactverzoek_WhenMedewerkerHasNoEmail()
        {
            if (string.IsNullOrWhiteSpace(TestDataConstants.Doorsturen.TestMedewerkerNoEmailSearchQuery))
                Assert.Inconclusive("TestMedewerkerNoEmailSearchQuery is not set — requires a medewerker without email in the test objectenregister.");

            var onderwerp = $"Test_Doorsturen_NoEmailMdw_{Guid.NewGuid().ToString()[..8]}";
            await SetupAndNavigateToDoorsturenTab(onderwerp);

            await Step("Switch to 'Medewerker' mode");
            await Page.GetDoorsturenMedewerkerRadio().ClickAsync();

            await Step($"Search for '{TestDataConstants.Doorsturen.TestMedewerkerNoEmailSearchQuery}'");
            await Page.GetDoorsturenMedewerkerCombobox()
                .FillAsync(TestDataConstants.Doorsturen.TestMedewerkerNoEmailSearchQuery);
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("Select the first medewerker from the listbox");
            var listbox = Page.GetByRole(AriaRole.Listbox);
            await Expect(listbox).ToBeVisibleAsync(new() { Timeout = 10000 });
            var firstOption = listbox.GetByRole(AriaRole.Option).First;
            await firstOption.ClickAsync();

            await Step("Select secondary afdeling or groep if picker is shown");
            var secondaryPicker = Page.GetDoorsturenSecondaryPicker();
            if (await secondaryPicker.IsVisibleAsync())
            {
                await secondaryPicker.SelectOptionAsync(new SelectOptionValue[] { new() { Index = 1 } });
            }

            await Step("Submit the forwarding form");
            await Page.GetDoorsturenSubmitButton().ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("Verify contactverzoek was forwarded (toast contains 'doorgestuurd')");
            await Expect(Page.GetDoorsturenToast("doorgestuurd")).ToBeVisibleAsync(new() { Timeout = 10000 });
        }

        [TestMethod("Validatie: medewerker-modus vereist secundaire afdeling of groep — submit geblokkeerd")]
        public async Task User_CannotSubmitForwardForm_InMedewerkerMode_WithoutAnySelection()
        {
            var onderwerp = $"Test_Doorsturen_NoSelection_{Guid.NewGuid().ToString()[..8]}";
            await SetupAndNavigateToDoorsturenTab(onderwerp);

            await Step("Switch to 'Medewerker' mode without selecting a medewerker or secondary");
            await Page.GetDoorsturenMedewerkerRadio().ClickAsync();

            await Step("Attempt to submit the form without making any selection");
            await Page.GetDoorsturenSubmitButton().ClickAsync();

            await Step("Verify no success toast appeared — form submission was blocked");
            await Expect(Page.GetDoorsturenToast("doorgestuurd")).ToBeHiddenAsync(new() { Timeout = 3000 });
        }

        [TestMethod("Modus selectie volgorde: Afdeling eerst, dan Groep, dan Medewerker")]
        public async Task User_SeesRadioOptionsInCorrectOrder()
        {
            var onderwerp = $"Test_Doorsturen_Order_{Guid.NewGuid().ToString()[..8]}";
            await SetupAndNavigateToDoorsturenTab(onderwerp);

            await Step("Verify all three radio buttons are present");
            await Expect(Page.GetDoorsturenAfdelingRadio()).ToBeVisibleAsync();
            await Expect(Page.GetDoorsturenGroepRadio()).ToBeVisibleAsync();
            await Expect(Page.GetDoorsturenMedewerkerRadio()).ToBeVisibleAsync();

            await Step("Verify Afdeling radio appears before Groep in the DOM");
            var radioButtons = Page.GetByRole(AriaRole.Radio);
            var labels = await radioButtons.AllInnerTextsAsync();
            // The radios themselves don't have inner text, check via ARIA names via positions
            var afdelingBox = await Page.GetDoorsturenAfdelingRadio().BoundingBoxAsync();
            var groepBox = await Page.GetDoorsturenGroepRadio().BoundingBoxAsync();
            var medewerkerBox = await Page.GetDoorsturenMedewerkerRadio().BoundingBoxAsync();
            Assert.IsNotNull(afdelingBox);
            Assert.IsNotNull(groepBox);
            Assert.IsNotNull(medewerkerBox);
            Assert.IsTrue(afdelingBox.Y < groepBox.Y, "Afdeling radio should appear above Groep");
            Assert.IsTrue(groepBox.Y < medewerkerBox.Y, "Groep radio should appear above Medewerker");
        }
    }
}
