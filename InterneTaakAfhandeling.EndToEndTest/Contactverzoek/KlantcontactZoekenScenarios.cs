using InterneTaakAfhandeling.EndToEndTest.Infrastructure;
using Microsoft.Playwright;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.RegularExpressions;

namespace InterneTaakAfhandeling.EndToEndTest.Contactverzoek
{
    /// <summary>
    /// E2E-tests voor de klantcontactnummer-zoekpopover in de hoofdnavigatie (Feature #467, Task #556).
    /// Dekt de scenario's uit Task #552 en #553. Twee scenario's zijn niet geautomatiseerd:
    ///   - "buiten bevoegdheid" — de enige geconfigureerde test-identity is Functioneel Beheerder, en
    ///     <c>ContactverzoekAutorisatieGuardService</c> geeft die rol altijd toegang; een non-admin
    ///     test-identity bestaat nog niet (zelfde blocker als de TODO in AlleContactverzoekenAccessScenarios).
    ///   - "overige fout" — de frontend vangt elke fout buiten 403 op in dezelfde generieke melding als
    ///     "niet gevonden"/"meerdere treffers"; er is geen reëel, niet-gemockt backend-scenario dat zich
    ///     hiervan onderscheidt op UI-niveau, en QA.md verbiedt het mocken van API-responses.
    /// </summary>
    [TestClass]
    [DoNotParallelize]
    public class KlantcontactZoekenScenarios : ITAPlaywrightTest
    {
        // Scenario: Een medewerker vindt een Contactverzoek via het klantcontactnummer
        [TestMethod("Een medewerker vindt een Contactverzoek via het klantcontactnummer")]
        public async Task Medewerker_VindtContactverzoek_ViaKlantcontactnummer()
        {
            var onderwerp = $"Test_KlantcontactZoeken_Gevonden_{Guid.NewGuid().ToString()[..8]}";
            var (contactmomentUuid, klantcontactNummer) =
                await TestDataHelper.CreateContactverzoekWithMedewerkerOnly(onderwerp, DateTime.UtcNow);
            RegisterCleanup(async () => await TestDataHelper.DeleteContactverzoekAsync(contactmomentUuid.ToString()));

            await NavigateToAuthenticatedPage();

            await Step($"Open de zoekpopover en zoek op klantcontactnummer {klantcontactNummer}");
            await OpenZoekpopoverAndSearch(klantcontactNummer);

            await Step("Verify navigatie naar de Contactverzoek detailpagina");
            await Expect(Page).ToHaveURLAsync(new Regex($"/contactmoment/{Regex.Escape(klantcontactNummer)}"));
            await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = $"Contactverzoek {klantcontactNummer}" }))
                .ToBeVisibleAsync();
        }

        // Scenario: Een medewerker zoekt op een klantcontactnummer dat niet bestaat
        [TestMethod("Een medewerker zoekt op een klantcontactnummer dat niet bestaat")]
        public async Task Medewerker_ZoektOpNietBestaandKlantcontactnummer()
        {
            await NavigateToAuthenticatedPage();

            var nietBestaandNummer = $"9999{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";

            await Step($"Open de zoekpopover en zoek op niet-bestaand klantcontactnummer {nietBestaandNummer}");
            await OpenZoekpopoverAndSearch(nietBestaandNummer);

            await Step("Verify generieke foutmelding wordt getoond");
            await Expect(Page.GetKlantcontactZoekErrorMessage())
                .ToHaveTextAsync("Geen contactverzoek gevonden voor dit klantcontactnummer.");

            await Step("Verify de popover blijft open (geen navigatie)");
            await Expect(Page.GetKlantcontactZoekInput()).ToBeVisibleAsync();
        }

        // Scenario: Eén klantcontactnummer verwijst naar meerdere Contactverzoeken
        [TestMethod("Eén klantcontactnummer verwijst naar meerdere Contactverzoeken")]
        public async Task Medewerker_ZoektOpDubbelKlantcontactnummer_ToontGenericeFoutmelding()
        {
            var onderwerp = $"Test_KlantcontactZoeken_Ambigu_{Guid.NewGuid().ToString()[..8]}";
            var (contactmomentUuid, klantcontactNummer) =
                await TestDataHelper.CreateDuplicateKlantcontactInternetakenAsync(onderwerp);
            RegisterCleanup(async () => await TestDataHelper.DeleteContactverzoekAsync(contactmomentUuid.ToString()));

            await NavigateToAuthenticatedPage();

            await Step($"Open de zoekpopover en zoek op het dubbele klantcontactnummer {klantcontactNummer}");
            await OpenZoekpopoverAndSearch(klantcontactNummer);

            await Step("Verify dezelfde generieke foutmelding wordt getoond — geen keuzescherm");
            await Expect(Page.GetKlantcontactZoekErrorMessage())
                .ToHaveTextAsync("Geen contactverzoek gevonden voor dit klantcontactnummer.");
            await Expect(Page.GetByRole(AriaRole.Dialog)).Not.ToBeVisibleAsync();
        }

        // Scenario: De zoekpopover-knop toont het definitieve zoek-icoon
        //
        // Task #553's eigen Test Mapping (QA override) erkent al dat het `search-klantcontact`-symbool
        // door commit e036459 (binnen dezelfde merged PR #554) weer uit icon-sprite.svg is verwijderd;
        // de knop toont sindsdien `magnifying-glass`. Deze test asserteert het daadwerkelijk
        // getoonde icoon i.p.v. het inmiddels niet meer bestaande `search-klantcontact`-symbool.
        [TestMethod("De zoekpopover-knop toont het definitieve zoek-icoon")]
        public async Task KlantcontactZoekButton_ToontIcoon()
        {
            await NavigateToAuthenticatedPage();

            await Step("Verify de zoekpopover-knop is aanwezig in de hoofdnavigatie");
            await Expect(Page.GetKlantcontactZoekButton()).ToBeVisibleAsync();

            await Step("Verify het getoonde icoon (huidige sprite-referentie: magnifying-glass)");
            var iconHref = await Page.GetKlantcontactZoekIconUse()
                .EvaluateAsync<string>("el => el.getAttribute('xlink:href') || el.getAttribute('href') || ''");
            Assert.IsTrue(iconHref.EndsWith("#magnifying-glass"),
                $"Expected icon sprite reference to end with '#magnifying-glass'. Actual: '{iconHref}'");
        }

        // Scenario: Een medewerker zoekt op een klantcontactnummer buiten zijn bevoegdheid — NOT AUTOMATED.
        // De enige geconfigureerde test-identity is Functioneel Beheerder; ContactverzoekAutorisatieGuardService
        // geeft die rol altijd toegang (bypass van de afdeling/groep-check), dus een echte 403 is niet
        // reproduceerbaar met de huidige testinfra. Zie ook de uitgecommentarieerde TODO in
        // AlleContactverzoekenAccessScenarios.cs voor dezelfde, al eerder erkende blocker.
        [TestMethod("Een medewerker zoekt op een klantcontactnummer buiten zijn bevoegdheid")]
        [Ignore("Vereist een non-admin test-identity, nog niet beschikbaar in de testinfrastructuur")]
        public async Task Medewerker_ZoektOpKlantcontactnummerBuitenBevoegdheid()
        {
            await Task.CompletedTask;
        }

        // Scenario: Een overige fout tijdens het zoeken — NOT AUTOMATED.
        // De frontend vangt elke fout buiten 403 op in dezelfde generieke melding als "niet gevonden"/
        // "meerdere treffers" (zie KlantcontactZoekPopover.vue's catch-blok). Er is geen reëel,
        // niet-gemockt backend-scenario dat zich hiervan op UI-niveau onderscheidt, en QA.md verbiedt
        // het mocken van API-responses in E2E-tests.
        [TestMethod("Een overige fout tijdens het zoeken")]
        [Ignore("Niet te onderscheiden van 'niet gevonden'/'meerdere treffers' op UI-niveau zonder mocking (verboden per QA.md)")]
        public async Task Medewerker_ZoektMetOverigeFout()
        {
            await Task.CompletedTask;
        }

        // Private helpers

        private async Task NavigateToAuthenticatedPage()
        {
            await Step("Navigate naar de hoofdpagina");
            await SafeGotoAsync("/");
            await Expect(Page.GetKlantcontactZoekButton()).ToBeVisibleAsync(new() { Timeout = 10000 });
        }

        private async Task OpenZoekpopoverAndSearch(string klantcontactNummer)
        {
            await Page.GetKlantcontactZoekButton().ClickAsync();
            await Expect(Page.GetKlantcontactZoekInput()).ToBeVisibleAsync();

            await Page.GetKlantcontactZoekInput().FillAsync(klantcontactNummer);
            await Page.GetKlantcontactZoekZoekenButton().ClickAsync();
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
    }
}
