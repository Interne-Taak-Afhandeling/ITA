using InterneTaakAfhandeling.EndToEndTest.Infrastructure;
using Microsoft.Playwright;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.RegularExpressions;

namespace InterneTaakAfhandeling.EndToEndTest.Contactverzoek
{
    /// <summary>
    /// E2E tests for reopening a closed (verwerkt) contactverzoek (Feature #391, Task #466).
    /// These tests cover all automated Gherkin scenarios. Two scenarios are excluded from automation:
    ///   - "API-aanroep zonder beheerderssessie wordt geweigerd" — covered by existing guard tests in ContactverzoekScenarios
    ///   - "Contactverzoek kan meerdere malen worden heropend" — excluded per task specification
    /// </summary>
    [TestClass]
    [DoNotParallelize]
    public class HeroepenContactverzoekScenarios : ITAPlaywrightTest
    {
        // Scenario: Beheerder ziet de heropenen-knop op een gesloten contactverzoek
        [TestMethod("Beheerder ziet de heropenen-knop op een gesloten contactverzoek")]
        public async Task HeroepenKnop_IsVisible_ForVerwerktContactverzoek()
        {
            var onderwerp = $"Test_HeroepenKnop_Visible_{Guid.NewGuid().ToString()[..8]}";
            var (contactmomentUuid, _, internetaakNummer) = await TestDataHelper.CreateVerwerktContactverzoekAsync(onderwerp);
            RegisterCleanup(async () => await TestDataHelper.DeleteContactverzoekAsync(contactmomentUuid.ToString()));

            await NavigateToVerwerktContactverzoekByNummer(internetaakNummer);

            await Step("Verify 'Heropenen' button is visible for verwerkt contactverzoek");
            await Expect(Page.GetHeroepenButton()).ToBeVisibleAsync();
        }

        // Scenario: Heropenen-knop is niet zichtbaar bij een open contactverzoek
        [TestMethod("Heropenen-knop is niet zichtbaar bij een open contactverzoek")]
        public async Task HeroepenKnop_IsNotVisible_ForTeVerwerkenContactverzoek()
        {
            var onderwerp = $"Test_HeroepenKnop_NotVisible_{Guid.NewGuid().ToString()[..8]}";
            var (contactmomentUuid, _, internetaakNummer) = await TestDataHelper.CreateTeVerwerkenContactverzoekAsync(onderwerp);
            RegisterCleanup(async () => await TestDataHelper.DeleteContactverzoekAsync(contactmomentUuid.ToString()));

            await NavigateToContactverzoekByNummer(internetaakNummer);

            await Step("Verify 'Heropenen' button is NOT visible for te_verwerken contactverzoek");
            await Expect(Page.GetHeroepenButton()).Not.ToBeVisibleAsync();
        }

        // Scenario: Beheerder heropent een gesloten contactverzoek via de dialoog
        [TestMethod("Beheerder heropent een gesloten contactverzoek via de dialoog")]
        public async Task Beheerder_CanReopenVerwerktContactverzoek_ViaDialog()
        {
            var onderwerp = $"Test_Heropen_ViaDialog_{Guid.NewGuid().ToString()[..8]}";
            var (contactmomentUuid, internetaakUuid, internetaakNummer) = await TestDataHelper.CreateVerwerktContactverzoekAsync(onderwerp);
            RegisterCleanup(async () => await TestDataHelper.DeleteContactverzoekAsync(contactmomentUuid.ToString()));

            await NavigateToVerwerktContactverzoekByNummer(internetaakNummer);

            await Step("Click 'Heropenen' button to open dialog");
            await Page.GetHeroepenButton().ClickAsync();

            await Step("Verify dialog is open with heading 'Contactverzoek heropenen'");
            await Expect(Page.GetByRole(AriaRole.Dialog)).ToBeVisibleAsync();
            await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Contactverzoek heropenen" })).ToBeVisibleAsync();

            await Step("Fill in reden 'Klant heeft aanvullende vraag'");
            await Page.GetHeroepenRedenTextbox().FillAsync("Klant heeft aanvullende vraag");

            await Step("Click 'Heropenen' confirm button in dialog");
            await Page.GetHeroepenDialogBevestigenButton().ClickAsync();

            await Step("Verify success toast is shown");
            await Expect(Page.GetContactverzoekHeropendMessage()).ToBeVisibleAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("Verify action controls are now visible (contactverzoek is open again)");
            await Expect(Page.GetContactmomentRegistrerenTab()).ToBeVisibleAsync();
            await Expect(Page.GetDoorsturenTab()).ToBeVisibleAsync();

            await Step("Verify afgehandeld message is no longer shown");
            await Expect(Page.GetAfgehandeldMessage()).Not.ToBeVisibleAsync();

            await Step("Verify behandelaar assignment is still intact (same behandelaar)");
            await Expect(Page.GetToewijzenAanMezelfButton()).ToBeVisibleAsync();

            await Step("Verify status in OpenKlant is 'te_verwerken'");
            await VerifyInternetaakStatusInOpenKlant(internetaakUuid, "te_verwerken");
        }

        // Scenario: Logboek toont heropening-regel na heropening
        [TestMethod("Logboek toont heropening-regel na heropening")]
        public async Task Logboek_ToontHeropeningRegel_NaHeropening()
        {
            var onderwerp = $"Test_Logboek_Heropening_{Guid.NewGuid().ToString()[..8]}";
            var (contactmomentUuid, _, internetaakNummer) = await TestDataHelper.CreateVerwerktContactverzoekAsync(onderwerp);
            RegisterCleanup(async () => await TestDataHelper.DeleteContactverzoekAsync(contactmomentUuid.ToString()));

            await NavigateToVerwerktContactverzoekByNummer(internetaakNummer);

            await Step("Heropen contactverzoek with reden 'Herbeoordeling'");
            await Page.GetHeroepenButton().ClickAsync();
            await Page.GetHeroepenRedenTextbox().FillAsync("Herbeoordeling");
            await Page.GetHeroepenDialogBevestigenButton().ClickAsync();
            await Expect(Page.GetContactverzoekHeropendMessage()).ToBeVisibleAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("Reload page to ensure logboek is refreshed");
            await Page.ReloadAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("Verify logboek contains 'Heropend' step heading");
            await Expect(Page.GetByText("Heropend", new() { Exact = true })).ToBeVisibleAsync();

            await Step("Verify logboek contains actor name and timestamp");
            var logbookSection = Page.Locator("ol");
            var logbookText = await logbookSection.InnerTextAsync();

            Assert.IsTrue(logbookText.Contains("E2E test contactverzoek creator"),
                $"Logboek should contain actor name 'E2E test contactverzoek creator'. Actual: '{logbookText}'");

            var dateTimePattern = @"\d{2}-\d{2}-\d{4} \d{2}:\d{2}";
            Assert.IsTrue(Regex.IsMatch(logbookText, dateTimePattern),
                $"Logboek should contain datetime in DD-MM-YYYY HH:MM format. Actual: '{logbookText}'");

            await Step("Verify reden 'Herbeoordeling' is visible in logboek");
            await Expect(logbookSection.GetByText("Herbeoordeling")).ToBeVisibleAsync();
        }

        // Scenario: Bevestigen zonder reden is niet mogelijk
        [TestMethod("Bevestigen zonder reden is niet mogelijk")]
        public async Task HeroepenDialog_BlocksSubmit_WhenRedenIsEmpty()
        {
            var onderwerp = $"Test_Heropen_ZonderReden_{Guid.NewGuid().ToString()[..8]}";
            var (contactmomentUuid, _, internetaakNummer) = await TestDataHelper.CreateVerwerktContactverzoekAsync(onderwerp);
            RegisterCleanup(async () => await TestDataHelper.DeleteContactverzoekAsync(contactmomentUuid.ToString()));

            await NavigateToVerwerktContactverzoekByNummer(internetaakNummer);

            await Step("Open heropenen dialog");
            await Page.GetHeroepenButton().ClickAsync();
            await Expect(Page.GetByRole(AriaRole.Dialog)).ToBeVisibleAsync();

            await Step("Try to submit without filling in reden");
            await Page.GetHeroepenDialogBevestigenButton().ClickAsync();

            await Step("Verify form validation blocks submission — reden field reports valueMissing");
            var redenTextbox = Page.GetHeroepenRedenTextbox();
            await Expect(redenTextbox).ToHaveJSPropertyAsync("validity.valueMissing", true);

            await Step("Verify dialog is still open (submission was blocked)");
            await Expect(Page.GetByRole(AriaRole.Dialog)).ToBeVisibleAsync();

            await Step("Verify no success toast was shown");
            await Expect(Page.GetContactverzoekHeropendMessage()).Not.ToBeVisibleAsync();
        }

        // Scenario: Annuleren van de dialoog wijzigt de status niet
        [TestMethod("Annuleren van de dialoog wijzigt de status niet")]
        public async Task HeroepenDialog_Annuleren_LeavesStatusVerwerkt()
        {
            var onderwerp = $"Test_Heropen_Annuleren_{Guid.NewGuid().ToString()[..8]}";
            var (contactmomentUuid, internetaakUuid, internetaakNummer) = await TestDataHelper.CreateVerwerktContactverzoekAsync(onderwerp);
            RegisterCleanup(async () => await TestDataHelper.DeleteContactverzoekAsync(contactmomentUuid.ToString()));

            await NavigateToVerwerktContactverzoekByNummer(internetaakNummer);

            await Step("Open heropenen dialog");
            await Page.GetHeroepenButton().ClickAsync();
            await Expect(Page.GetByRole(AriaRole.Dialog)).ToBeVisibleAsync();

            await Step("Fill in reden (dialog should be openable and fillable)");
            await Page.GetHeroepenRedenTextbox().FillAsync("Testannulering");

            await Step("Click 'Annuleren' to close dialog without confirming");
            await Page.GetAnnulerenDialogButton().ClickAsync();

            await Step("Verify dialog is closed");
            await Expect(Page.GetByRole(AriaRole.Dialog)).Not.ToBeVisibleAsync();

            await Step("Verify read-only mode is still active — afgehandeld message visible, action tabs hidden");
            await Expect(Page.GetAfgehandeldMessage()).ToBeVisibleAsync();
            await Expect(Page.GetContactmomentRegistrerenTab()).Not.ToBeVisibleAsync();
            await Expect(Page.GetDoorsturenTab()).Not.ToBeVisibleAsync();

            await Step("Verify no success toast was shown");
            await Expect(Page.GetContactverzoekHeropendMessage()).Not.ToBeVisibleAsync();

            await Step("Verify status in OpenKlant is still 'verwerkt'");
            await VerifyInternetaakStatusInOpenKlant(internetaakUuid, "verwerkt");
        }

        // Scenario: Toast bij heropening met inactieve behandelaar
        // Excluded: requires inactive medewerker with organisatorische eenheid in test environment.
        // Creating this test data reliably is not possible with current TestDataHelper.
        // See task #466: "Inactieve-behandelaar scenario: Verifieer haalbaarheid voordat je de helper implementeert"
        [TestMethod("Toast bij heropening met inactieve behandelaar")]
        [Ignore("Testdata niet beschikbaar in testomgeving: inactieve behandelaar met organisatorische eenheid vereist")]
        public async Task Heropen_ShowsWarningToast_WhenBehandelaarIsInactief()
        {
            await Task.CompletedTask;
        }

        // Scenario: API-aanroep zonder beheerderssessie wordt geweigerd — NOT AUTOMATED per task specification.
        // Covered by existing 403-guard pattern in ContactverzoekScenarios (Feature #344 guard tests).

        // Scenario: Contactverzoek kan meerdere malen worden heropend — NOT AUTOMATED per task specification.

        // Private navigation helpers

        private async Task NavigateToVerwerktContactverzoekByNummer(string internetaakNummer)
        {
            await Step($"Navigate to verwerkt contactverzoek: {internetaakNummer}");
            await SafeGotoAsync($"/contactverzoek/{internetaakNummer}");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await Expect(Page.GetAfgehandeldMessage()).ToBeVisibleAsync(new() { Timeout = 10000 });
        }

        private async Task NavigateToContactverzoekByNummer(string internetaakNummer)
        {
            await Step($"Navigate to contactverzoek: {internetaakNummer}");
            await SafeGotoAsync($"/contactverzoek/{internetaakNummer}");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await Expect(Page.GetContactmomentRegistrerenTab()).ToBeVisibleAsync(new() { Timeout = 10000 });
        }

        private async Task SafeGotoAsync(string url)
        {
            Microsoft.Playwright.PlaywrightException? lastException = null;
            for (var attempt = 1; attempt <= 3; attempt++)
            {
                try
                {
                    await Page.GotoAsync(url);
                    return;
                }
                catch (Microsoft.Playwright.PlaywrightException ex)
                {
                    lastException = ex;
                    if (attempt < 3) await Task.Delay(1500);
                }
            }
            throw new InvalidOperationException(
                $"Failed to navigate to '{url}' after 3 attempts.",
                lastException);
        }

        private async Task VerifyInternetaakStatusInOpenKlant(Guid internetaakUuid, string expectedStatus)
        {
            await Step($"Verify status in OpenKlant is '{expectedStatus}'");
            var internetaak = await TestDataHelper.GetInternetaakByIdAsync(internetaakUuid);
            var deadline = DateTimeOffset.UtcNow.AddSeconds(30);

            while (internetaak.Status != expectedStatus && DateTimeOffset.UtcNow < deadline)
            {
                await Task.Delay(1000);
                internetaak = await TestDataHelper.GetInternetaakByIdAsync(internetaakUuid);
            }

            Assert.AreEqual(expectedStatus, internetaak.Status, $"Status should be '{expectedStatus}'");
        }
    }
}
