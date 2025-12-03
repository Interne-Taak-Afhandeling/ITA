using System.Text.RegularExpressions;
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


        [TestMethod("Add a note to contactverzoek that has ZAAK connected to it")]
        public async Task User_AddNoteToContactverzoekWithZAAK_FromDashboard()
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

            await Step("User clicks on 'Alleen toelichting' tab");
            var alleenToelichtingTab = Page.GetAlleenToelichtingTab();
            await alleenToelichtingTab.ClickAsync();

            await Step("Add a note to the contactverzoek");
            var toelichtingTextarea = Page.GetToelichtingTextarea();
            var noteText = "This is a test note added during ITA E2E testing.";
            await toelichtingTextarea.FillAsync(noteText);

            await Step("Click on 'Opslaan' button to save the note");
            var opslaanButton = Page.GetOpslaanButton();
            await opslaanButton.ClickAsync();

            await Step("Verify the note has been added successfully in logboek");
            var savedNote = Page.Locator($"text={noteText}");
            await Expect(savedNote).ToBeVisibleAsync();

        }


        [TestMethod("Add a note with the actions contact moment and no answer")]
        public async Task User_AddNoteWithContactMomentNoAnswer_FromDashboard()
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

            await Step("user click on Contact opnemen niet gelukt and selects Nee");
            await Page.GetByRole(AriaRole.Radio, new() { Name = "Contact opnemen niet gelukt" }).CheckAsync();
            await Page.GetByRole(AriaRole.Radio, new() { Name = "Nee" }).CheckAsync();

            await Step("user selects E-mail as kanaal");
            await Page.GetByLabel("Kanaal").SelectOptionAsync(new[] { "E-mail" });

            await Step("user adds internal note and saves contact moment");
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Interne toelichting" }).ClickAsync();
            var noteText = "test note via contact moment no answer";
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Interne toelichting" }).FillAsync(noteText);

            await Step("Click on ' Contactmoment Opslaan' button to save the note");
            var opslaanButton = Page.GetContactmomentOpslaanButton();
            await opslaanButton.ClickAsync();

            await Step("Verify the note has been added successfully in logboek");
            var savedNote = Page.Locator($"text={noteText}");
            await Expect(savedNote).ToBeVisibleAsync();

        }
    }
}
