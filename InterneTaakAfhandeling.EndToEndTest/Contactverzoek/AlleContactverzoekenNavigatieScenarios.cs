using InterneTaakAfhandeling.EndToEndTest.Infrastructure;
using Microsoft.Playwright;
using System.Text.RegularExpressions;

namespace InterneTaakAfhandeling.EndToEndTest.Contactverzoek
{
    [TestClass]
    [DoNotParallelize]
    public class AlleContactverzoekenNavigatieScenarios : ITAPlaywrightTest
    {
        [TestMethod("Beheerder ziet 'Alle contactverzoeken' in de navigatie")]
        public async Task Beheerder_ZietAlleContactverzoeken_InNavigatie()
        {
            await Step("Given een medewerker met de rol 'Beheerder' is ingelogd");
            await Page.GotoAsync("/");

            await Step("When de navigatie wordt weergegeven");
            var navLink = Page.GetByRole(AriaRole.Link, new() { Name = "Alle contactverzoeken" });

            await Step("Then is de optie 'Alle contactverzoeken' zichtbaar");
            await Expect(navLink).ToBeVisibleAsync();
        }

        // TODO: Scenario 2 — "Medewerker zonder beheerderrol ziet 'Alle contactverzoeken' niet"
        // Requires a second test user WITHOUT the "Beheerder" role.
        // Currently only one test account (with Beheerder role) is configured.
        // To implement: add TEST_USERNAME_NON_ADMIN / TEST_PASSWORD_NON_ADMIN to user-secrets,
        // create a login flow for that user, and assert:
        //   await Expect(navLink).Not.ToBeVisibleAsync();

        [TestMethod("Helpteksten zijn zichtbaar op de contactverzoek detailpagina")]
        public async Task Helpteksten_ZijnZichtbaar_OpContactverzoekDetailpagina()
        {
            var onderwerp = $"Test_Helpteksten_{Guid.NewGuid().ToString()[..8]}";
            var (contactmomentUuid, _, internetaakNummer) = await TestDataHelper.CreateTeVerwerkenContactverzoekAsync(onderwerp);
            RegisterCleanup(async () => await TestDataHelper.DeleteContactverzoekAsync(contactmomentUuid.ToString()));

            await Step("Given de gebruiker zich op een contactverzoek detailpagina bevindt");
            await Page.GotoAsync($"/contactverzoek/{internetaakNummer}");
            await Expect(Page.GetContactmomentRegistrerenTab()).ToBeVisibleAsync(new() { Timeout = 10000 });

            await Step("Then wordt de helptekst voor 'Informatie voor inwoner/bedrijf' getoond op het tabblad Contact registreren");
            await Expect(Page.GetByText(
                "Deze tekst kan zichtbaar zijn voor de inwoner of het bedrijf, bijvoorbeeld in een Mijn-omgeving."))
                .ToBeVisibleAsync();

            await Step("And is het veldlabel 'Informatie voor burger/bedrijf' vervangen door 'Informatie voor inwoner/bedrijf'");
            await Expect(Page.Locator("label[for='informatie-burger']")).ToHaveTextAsync("Informatie voor inwoner / bedrijf");
            await Expect(Page.Locator("label[for='informatie-burger']")).Not.ToHaveTextAsync("Informatie voor burger / bedrijf");

            await Step("And wordt de helptekst 'Bedoeld voor collega's.' getoond bij interne toelichting op het tabblad Contact registreren");
            await Expect(Page.GetByText("Bedoeld voor collega's.")).ToBeVisibleAsync();

            await Step("When de gebruiker naar het tabblad Doorsturen navigeert");
            await Page.GetDoorsturenTab().ClickAsync();

            await Step("Then wordt de helptekst 'Bedoeld voor collega's.' getoond bij interne toelichting op het tabblad Doorsturen");
            await Expect(Page.GetByText("Bedoeld voor collega's.")).ToBeVisibleAsync();

            await Step("When de gebruiker naar het tabblad Alleen toelichting navigeert");
            await Page.GetAlleenToelichtingTab().ClickAsync();

            await Step("Then wordt de helptekst 'Bedoeld voor collega's.' getoond bij interne toelichting op het tabblad Alleen toelichting");
            await Expect(Page.GetByText("Bedoeld voor collega's.")).ToBeVisibleAsync();
        }

        [TestMethod("Afgehandeld op datum wordt getoond in Afdelingshistorie en Mijn historie na sluiten")]
        public async Task AfgehandeldOp_IsGevuld_InAfdelingshistorieEnMijnHistorie_NaSluiten()
        {
            const string afdelingNaam = "Burgerzaken_ibz";
            var onderwerp = $"Test_AfgehandeldOp_{Guid.NewGuid().ToString()[..8]}";

            await Step("Given de gebruiker zich op een contactverzoek detailpagina bevindt");
            var contactmomentUuid = await TestDataHelper.CreateContactverzoek(onderwerp, attachZaak: false);
            RegisterCleanup(async () => await TestDataHelper.DeleteContactverzoekAsync(contactmomentUuid.ToString()));

            var internetaakUuid = await TestDataHelper.GetInternetaakUuidFromContactmomentAsync(contactmomentUuid)
                ?? throw new InvalidOperationException("Internetaak UUID should be found");
            var internetaak = await TestDataHelper.GetInternetaakByIdAsync(internetaakUuid);
            var internetaakNummer = internetaak.Nummer ?? throw new InvalidOperationException("Internetaak nummer should be found");

            await Page.GotoAsync($"/contactverzoek/{internetaakNummer}");
            await Expect(Page.GetContactmomentRegistrerenTab()).ToBeVisibleAsync(new() { Timeout = 10000 });

            await Step("And user close the contactverzoek");
            await CloseContactverzoek(onderwerp);

            await Step("When user navigates to Afdelingshistorie");
            await Page.GotoAsync("/");
            var afdelingshistorieLink = Page.GetByRole(AriaRole.Link, new() { Name = "Afdelingshistorie", Exact = true });
            await afdelingshistorieLink.WaitForAsync(new() { State = WaitForSelectorState.Visible });
            await afdelingshistorieLink.ClickAsync();

            await Step("And select the same afdeling from the dropdown");
            var afdelingDropdown = Page.GetByLabel("Selecteer een afdeling of");
            await afdelingDropdown.WaitForAsync(new() { State = WaitForSelectorState.Visible });
            await afdelingDropdown.SelectOptionAsync(new[] { afdelingNaam });

            await Step("Then Afgehandeld op is filled with closing date time");
            await VerifyAfgehandeldOpIsFilled(onderwerp);

            await Step("When user navigates to Mijn historie");
            await Page.GotoAsync("/historie");
            await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Mijn historie", Level = 1 })).ToBeVisibleAsync();

            await Step("Then Afgehandeld op is filled with closing date time");
            await VerifyAfgehandeldOpIsFilled(onderwerp);
        }

        private async Task CloseContactverzoek(string onderwerp)
        {
            var toewijzenButton = Page.GetToewijzenAanMezelfButton();
            if (await toewijzenButton.IsVisibleAsync())
            {
                await toewijzenButton.ClickAsync();
                await Expect(Page.GetByRole(AriaRole.Dialog)).ToBeVisibleAsync();
                await Page.GetToewijzenAanMezelfDialogButton().ClickAsync();
                await Expect(Page.GetContactverzoekToegewezenMessage()).ToBeVisibleAsync();
            }

            await Page.GetContactmomentRegistrerenTab().ClickAsync();
            await Expect(Page.GetContactOpnemenGeluktRadio()).ToBeVisibleAsync();

            var kanaalOption = Page.Locator("#kanalen").Locator("option", new() { HasText = "Telefoon" });
            await kanaalOption.WaitForAsync(new() { State = WaitForSelectorState.Attached });
            await Page.Locator("#kanalen").SelectOptionAsync(new[] { "Telefoon" });

            await Page.Locator("#informatie-burger").FillAsync($"Closing contactverzoek '{onderwerp}' for historie test");
            await Page.GetByLabel("Ja").ClickAsync();
            await Page.GetContactmomentOpslaanButton().ClickAsync();
            await Expect(Page.GetByRole(AriaRole.Dialog)).ToBeVisibleAsync();
            await Page.GetOpslaanEnAfrondenButton().ClickAsync();
        }

        private async Task VerifyAfgehandeldOpIsFilled(string onderwerp)
        {
            var row = Page.GetByRole(AriaRole.Row).Filter(new() { HasText = onderwerp });
            await Expect(row).ToBeVisibleAsync(new() { Timeout = 10000 });

            var rowText = await row.InnerTextAsync();
            var dateTimePattern = @"\d{2}-\d{2}-\d{4} \d{2}:\d{2}";
            Assert.IsTrue(Regex.IsMatch(rowText, dateTimePattern),
                $"Afgehandeld op should contain a datetime in DD-MM-YYYY HH:MM format. Actual row text: '{rowText}'");
        }
    }
}
