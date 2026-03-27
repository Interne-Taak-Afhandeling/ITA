using System.Text.RegularExpressions;
using InterneTaakAfhandeling.EndToEndTest.Infrastructure;
using ITA.InterneTaakAfhandeling.EndToEndTest.Helpers;
using Microsoft.Playwright;

namespace InterneTaakAfhandeling.EndToEndTest.Dashboard
{
    public partial class ContactverzoekScenarios
    {
        //  Setup & Navigation Helpers

        private async Task<Guid> SetupContactverzoek(string onderwerp, bool attachZaak)
        {
            await Step("Setup test data via API");
            var uuid = await TestDataHelper.CreateContactverzoek(onderwerp, attachZaak);
            RegisterCleanup(async () => await TestDataHelper.DeleteContactverzoekAsync(uuid.ToString()));
            return uuid;
        }

        private async Task<Guid> SetupContactverzoek(string onderwerp, bool attachZaak, string internetaakNummer)
        {
            await Step("Setup test data via API");
            var uuid = await TestDataHelper.CreateContactverzoek(onderwerp, attachZaak, internetaakNummer);
            RegisterCleanup(async () => await TestDataHelper.DeleteContactverzoekAsync(uuid.ToString()));
            return uuid;
        }

        private async Task CreateAssignAndCloseContactverzoek(string onderwerp, string informatieText)
        {
            await Step("Create and close contactverzoek");
            var contactmomentUuid = await SetupContactverzoek(onderwerp, attachZaak: false);
            var internetaakUuid = await TestDataHelper.GetInternetaakUuidFromContactmomentAsync(contactmomentUuid);
            Assert.IsNotNull(internetaakUuid, "Internetaak UUID should be found");

            var internetaak = await TestDataHelper.GetInternetaakByIdAsync(internetaakUuid.Value);
            Assert.IsNotNull(internetaak.Nummer, "Internetaak nummer should be found");

            await NavigateToContactverzoekByNummer(internetaak.Nummer!);
            await Page.GetToewijzenAanMezelfButton().ClickAsync();
            await Page.GetToewijzenAanMezelfDialogButton().ClickAsync();
            await Expect(Page.GetContactverzoekToegewezenMessage()).ToBeVisibleAsync();

            await NavigateToContactmomentRegistrerenTab();
            await Page.Locator("#kanalen").SelectOptionAsync(new[] { "Telefoon" });
            await Page.Locator("#informatie-burger").FillAsync(informatieText);
            await Page.GetByLabel("Ja").ClickAsync();
            await Page.GetContactmomentOpslaanButton().ClickAsync();
            await Expect(Page.GetByRole(AriaRole.Dialog)).ToBeVisibleAsync();
            await Page.GetOpslaanEnAfrondenButton().ClickAsync();

            await VerifyInternetaakStatusInOpenKlant(internetaakUuid.Value, "verwerkt", shouldHaveAfgehandeldOp: true);
        }

        private async Task<(Guid internetaakUuid, string internetaakNummer)> SetupAndResolveContactverzoek(string onderwerp, string internetaakNummer)
        {
            var contactmomentUuid = await SetupContactverzoek(onderwerp, attachZaak: false, internetaakNummer: internetaakNummer);
            var internetaakUuid = await TestDataHelper.GetInternetaakUuidFromContactmomentAsync(contactmomentUuid);
            Assert.IsNotNull(internetaakUuid, "Internetaak UUID should be found");

            var internetaak = await TestDataHelper.GetInternetaakByIdAsync(internetaakUuid.Value);
            Assert.IsNotNull(internetaak.Nummer, "Internetaak nummer should be found");

            return (internetaakUuid.Value, internetaak.Nummer!);
        }

        private async Task CloseContactverzoekByNummer(string internetaakNummer, string kanaal, string informatie)
        {
            await NavigateToContactverzoekByNummer(internetaakNummer);
            await Page.GetToewijzenAanMezelfButton().ClickAsync();
            await Page.GetToewijzenAanMezelfDialogButton().ClickAsync();
            await Expect(Page.GetContactverzoekToegewezenMessage()).ToBeVisibleAsync();

            await NavigateToContactmomentRegistrerenTab();
            await Expect(Page.GetContactOpnemenGeluktRadio()).ToBeCheckedAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Page.Locator("#kanalen").SelectOptionAsync(new[] { kanaal });
            await Page.Locator("#informatie-burger").FillAsync(informatie);
            await Page.GetByLabel("Ja").ClickAsync();
            await Page.GetContactmomentOpslaanButton().ClickAsync();
            await Expect(Page.GetByRole(AriaRole.Dialog)).ToBeVisibleAsync();
            await Page.GetOpslaanEnAfrondenButton().ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }

        private async Task SafeGotoAsync(string url)
        {
            for (var attempt = 1; attempt <= 3; attempt++)
            {
                try
                {
                    await Page.GotoAsync(url);
                    return;
                }
                catch (Microsoft.Playwright.PlaywrightException) when (attempt < 3)
                {
                    await Task.Delay(1500);
                }
            }
        }

        private async Task NavigateToContactverzoekDetails(string onderwerp)
        {
            await Step("Navigate to home page");
            await SafeGotoAsync("/");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step($"Click on contactverzoek '{onderwerp}'");
            var detailsLink = Page.GetDetailsLink(onderwerp);
            await detailsLink.WaitForAsync(new() { State = WaitForSelectorState.Visible });
            await detailsLink.ClickAsync();
        }

        private async Task NavigateToContactverzoekByNummer(string internetaakNummer)
        {
            await Step($"Navigate to contactverzoek by nummer: {internetaakNummer}");
            await SafeGotoAsync($"/contactverzoek/{internetaakNummer}");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step($"Verify contactverzoek page loaded with nummer: {internetaakNummer}");
            await Expect(Page.GetByText($"Contactverzoek {internetaakNummer}")).ToBeVisibleAsync();
        }

        private async Task NavigateToContactmomentRegistrerenTab()
        {
            await Step("Navigate to Contactmoment Registreren tab");
            await Page.GetContactmomentRegistrerenTab().ClickAsync();
        }

        private async Task NavigateToAfdelingshistorieAndSelectOrganisatorischeEenheid(string organisatorischeEenheidNaam)
        {
            await Step("Navigate to Afdelingshistorie");
            await SafeGotoAsync("/");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await Page.GetByRole(AriaRole.Link, new() { Name = "Afdelingshistorie", Exact = true }).ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step($"Select department '{organisatorischeEenheidNaam}' from dropdown");
            await Page.GetByLabel("Selecteer een afdeling of").SelectOptionAsync(new[] { organisatorischeEenheidNaam });
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }

        private async Task<List<string>> GetAfdelingenEnGroepenDropdownEntriesAsync()
        {
            var optionTexts = await Page
                .GetByLabel("Selecteer een afdeling of")
                .Locator("option")
                .AllInnerTextsAsync();

            return optionTexts
                .Select(text => text.Trim())
                .Where(text => text.StartsWith("Afdeling:") || text.StartsWith("Groep:"))
                .ToList();
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

        private async Task VerifyLatestRecordIsOnTopInHistorieTable(ILocator tableRows, int rowCount)
        {
            await Step("Verify latest record is on top");

            if (rowCount < 2)
            {
                return;
            }

            var firstRowText = await tableRows.First.InnerTextAsync();
            var secondRowText = await tableRows.Nth(1).InnerTextAsync();

            var datePattern = @"\b(\d{1,2})-(\d{1,2})-(\d{4})\b";
            var firstDateMatch = Regex.Match(firstRowText, datePattern);
            var secondDateMatch = Regex.Match(secondRowText, datePattern);

            if (firstDateMatch.Success && secondDateMatch.Success)
            {
                var firstDate = DateTime.ParseExact(firstDateMatch.Value, "d-M-yyyy", null);
                var secondDate = DateTime.ParseExact(secondDateMatch.Value, "d-M-yyyy", null);
                Assert.IsTrue(firstDate >= secondDate, "Expected latest closed contactverzoek to be listed first");
                return;
            }

            Assert.IsTrue(firstRowText.Length > 0, "Expected first row to contain data");
        }

        private async Task VerifyMijnHistorieDescendingOrder(string newestOnderwerp, string olderOnderwerp)
        {
            await Step("Navigate to History tab (Mijn historie)");
            await SafeGotoAsync("/historie");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("Verify both closed contact requests are displayed in the history tab");
            await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Mijn historie", Level = 1 })).ToBeVisibleAsync();
            await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Mijn afgeronde contactverzoeken", Level = 2 })).ToBeVisibleAsync();
            await Expect(Page.Locator($"text={newestOnderwerp}")).ToBeVisibleAsync();
            await Expect(Page.Locator($"text={olderOnderwerp}")).ToBeVisibleAsync();

            await Step("Verify list is sorted in descending order (most recent first)");
            var tableRows = Page.Locator("table tbody tr");
            var rowCount = await tableRows.CountAsync();

            Assert.IsTrue(rowCount >= 2, "Should have at least 2 rows in history");

            var firstRowText = await tableRows.First.InnerTextAsync();
            var secondRowText = await tableRows.Nth(1).InnerTextAsync();

            Assert.IsTrue(firstRowText.Contains(newestOnderwerp),
                $"Most recently closed contactverzoek '{newestOnderwerp}' should appear first in descending order");
            Assert.IsTrue(secondRowText.Contains(olderOnderwerp),
                $"Earlier closed contactverzoek '{olderOnderwerp}' should appear second in descending order");
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
            var deadline = DateTimeOffset.UtcNow.AddSeconds(30);

            while ((internetaak.Status != expectedStatus || (shouldHaveAfgehandeldOp && internetaak.AfgehandeldOp == null))
                   && DateTimeOffset.UtcNow < deadline)
            {
                await Task.Delay(1000);
                internetaak = await TestDataHelper.GetInternetaakByIdAsync(internetaakUuid);
            }

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
