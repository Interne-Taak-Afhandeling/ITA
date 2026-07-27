using System;
using System.Text.RegularExpressions;
using InterneTaakAfhandeling.EndToEndTest.Infrastructure;
using Microsoft.Playwright;

namespace InterneTaakAfhandeling.EndToEndTest.Contactverzoek
{
    // Feature #514 — Toegang tot Contactverzoeken beperkt tot eigen afdeling/groep
    // Task #544 — Phase 2 E2E-verificatie (implementatie is al gemerged, zie ITA-514)
    [TestClass]
    [DoNotParallelize]
    public class ContactverzoekToegangsbeperkingenScenarios : ITAPlaywrightTest
    {
        // The only afdeling confirmed present in the test objectenregister today (used
        // throughout TestDataHelper). "Financien_ibz" below is a placeholder for the second,
        // distinct afdeling the Gherkin scenarios need — not yet confirmed to exist.
        private const string BestaandeAfdeling = "Burgerzaken_ibz";
        private const string AndereAfdelingPlaceholder = "Financien_ibz";
        private const string GeenToegangMelding = "Je hebt geen toegang tot dit contactverzoek";
        private const string NietGevondenMelding = "Dit contactverzoek bestaat niet of is niet meer beschikbaar";

        [TestMethod("Beheerder kan Contactverzoek van elke afdeling inzien")]
        public async Task Beheerder_CanAccessContactverzoek_VanAndereAfdeling()
        {
            // Any afdeling proves the point here — the beheerder-exception in
            // ContactverzoekAutorisatieGuardService bypasses the afdeling/groep check entirely,
            // so we use the one afdeling confirmed to exist in the test objectenregister.
            await Step("Given een Contactverzoek toegewezen aan afdeling 'Burgerzaken_ibz'");
            var onderwerp = $"Test_Toegang_Beheerder_{Guid.NewGuid().ToString()[..8]}";
            var (contactmomentUuid, klantcontactNummer, _) = await TestDataHelper.CreateContactverzoekWithAfdelingOnlyAndContactDatum(
                onderwerp, DateTime.UtcNow, BestaandeAfdeling);
            RegisterCleanup(async () => await TestDataHelper.DeleteContactverzoekAsync(contactmomentUuid.ToString()));

            await Step("When de beheerder het Contactverzoek opent via de detail-URL");
            await Page.GotoAsync($"/contactmoment/{klantcontactNummer}");

            await Step("Then wordt de Contactverzoek-detailpagina getoond met inhoudelijke gegevens");
            await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = $"Contactverzoek {klantcontactNummer}" })).ToBeVisibleAsync();
            await Expect(Page.Locator($"text={onderwerp}")).ToBeVisibleAsync();
        }

        // Onderstaande 5 scenario's vereisen een non-admin behandelaar-testaccount dat nog niet
        // bestaat in de test-infrastructuur (zelfde blokkade als de bestaande TODO's in
        // AlleContactverzoekenAccessScenarios.cs / AlleContactverzoekenNavigatieScenarios.cs).
        // Ze zijn volledig geïmplementeerd en compileren, maar staan op [Ignore] tot:
        //   1. TestSettings:TEST_USERNAME_NON_ADMIN / TEST_PASSWORD_NON_ADMIN / TEST_TOTP_SECRET_NON_ADMIN
        //      zijn geconfigureerd via dotnet user-secrets voor een Azure AD-account zonder de
        //      FunctioneelBeheerder-rol;
        //   2. dat account een medewerker-actor heeft gekoppeld aan afdeling "Burgerzaken_ibz";
        //   3. een tweede, andere afdelingsnaam bevestigd bestaat in het testregister (hier
        //      gebruikt als placeholder "Financien_ibz").
        // Zie Task #544 (Technical Approach) voor details. Verwijder [Ignore] zodra opgelost.

        [Ignore("Vereist non-admin behandelaar-testaccount — zie class-level comment (Task #544).")]
        [TestMethod("Behandelaar kan Contactverzoek van eigen afdeling inzien")]
        public async Task Behandelaar_CanAccessContactverzoek_VanEigenAfdeling()
        {
            await HandleAuthenticationAsync(
                Configuration["TestSettings:TEST_USERNAME_NON_ADMIN"],
                Configuration["TestSettings:TEST_PASSWORD_NON_ADMIN"]);

            await Step("Given een Contactverzoek toegewezen aan de eigen afdeling van de behandelaar");
            var onderwerp = $"Test_Toegang_EigenAfdeling_{Guid.NewGuid().ToString()[..8]}";
            var (contactmomentUuid, klantcontactNummer, _) = await TestDataHelper.CreateContactverzoekWithAfdelingOnlyAndContactDatum(
                onderwerp, DateTime.UtcNow, BestaandeAfdeling);
            RegisterCleanup(async () => await TestDataHelper.DeleteContactverzoekAsync(contactmomentUuid.ToString()));

            await Step("When de behandelaar het Contactverzoek opent via de detail-URL");
            await Page.GotoAsync($"/contactmoment/{klantcontactNummer}");

            await Step("Then wordt de Contactverzoek-detailpagina getoond met inhoudelijke gegevens");
            await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = $"Contactverzoek {klantcontactNummer}" })).ToBeVisibleAsync();
            await Expect(Page.Locator($"text={onderwerp}")).ToBeVisibleAsync();
        }

        [Ignore("Vereist non-admin behandelaar-testaccount — zie class-level comment (Task #544).")]
        [TestMethod("Behandelaar kan Contactverzoek van andere afdeling niet inzien")]
        public async Task Behandelaar_CanNotAccessContactverzoek_VanAndereAfdeling()
        {
            await HandleAuthenticationAsync(
                Configuration["TestSettings:TEST_USERNAME_NON_ADMIN"],
                Configuration["TestSettings:TEST_PASSWORD_NON_ADMIN"]);

            await Step("Given een Contactverzoek toegewezen aan een andere afdeling dan de behandelaar");
            var onderwerp = $"Test_GeenToegang_AndereAfdeling_{Guid.NewGuid().ToString()[..8]}";
            var (contactmomentUuid, klantcontactNummer, _) = await TestDataHelper.CreateContactverzoekWithAfdelingOnlyAndContactDatum(
                onderwerp, DateTime.UtcNow, AndereAfdelingPlaceholder);
            RegisterCleanup(async () => await TestDataHelper.DeleteContactverzoekAsync(contactmomentUuid.ToString()));

            await Step("When de behandelaar het Contactverzoek opent via de detail-URL");
            await Page.GotoAsync($"/contactmoment/{klantcontactNummer}");

            await Step("Then wordt de melding 'Je hebt geen toegang tot dit contactverzoek' getoond");
            await Expect(Page.GetGeenToegangAlert()).ToBeVisibleAsync();

            await Step("And zijn er geen inhoudelijke gegevens zichtbaar");
            await Expect(Page.Locator($"text={onderwerp}")).Not.ToBeVisibleAsync();
        }

        [Ignore("Vereist non-admin behandelaar-testaccount — zie class-level comment (Task #544).")]
        [TestMethod("Geen-toegang-melding is onderscheiden van niet-gevonden")]
        public async Task GeenToegangMelding_IsOnderscheidenVanNietGevonden()
        {
            await HandleAuthenticationAsync(
                Configuration["TestSettings:TEST_USERNAME_NON_ADMIN"],
                Configuration["TestSettings:TEST_PASSWORD_NON_ADMIN"]);

            await Step("Given een Contactverzoek toegewezen aan een andere afdeling dan de behandelaar");
            var onderwerp = $"Test_GeenToegang_VsNietGevonden_{Guid.NewGuid().ToString()[..8]}";
            var (contactmomentUuid, klantcontactNummer, _) = await TestDataHelper.CreateContactverzoekWithAfdelingOnlyAndContactDatum(
                onderwerp, DateTime.UtcNow, AndereAfdelingPlaceholder);
            RegisterCleanup(async () => await TestDataHelper.DeleteContactverzoekAsync(contactmomentUuid.ToString()));

            await Step("When de behandelaar het Contactverzoek opent via de detail-URL");
            await Page.GotoAsync($"/contactmoment/{klantcontactNummer}");

            await Step("Then wordt de geen-toegang-melding getoond, niet de niet-gevonden-melding");
            await Expect(Page.GetGeenToegangAlert()).ToBeVisibleAsync();
            await Expect(Page.GetByText(NietGevondenMelding)).Not.ToBeVisibleAsync();
        }

        [Ignore("Vereist non-admin behandelaar-testaccount — zie class-level comment (Task #544).")]
        [TestMethod("Geen inhoudelijke gegevens in API-response bij geen toegang")]
        public async Task ApiResponse_BevatGeenInhoudelijkeGegevens_BijGeenToegang()
        {
            await HandleAuthenticationAsync(
                Configuration["TestSettings:TEST_USERNAME_NON_ADMIN"],
                Configuration["TestSettings:TEST_PASSWORD_NON_ADMIN"]);

            await Step("Given een Contactverzoek toegewezen aan een andere afdeling dan de behandelaar");
            var onderwerp = $"Test_ApiGeenToegang_{Guid.NewGuid().ToString()[..8]}";
            var (contactmomentUuid, klantcontactNummer, afdelingNaam) = await TestDataHelper.CreateContactverzoekWithAfdelingOnlyAndContactDatum(
                onderwerp, DateTime.UtcNow, AndereAfdelingPlaceholder);
            RegisterCleanup(async () => await TestDataHelper.DeleteContactverzoekAsync(contactmomentUuid.ToString()));

            await Step("When de API-call naar het by-klantcontact-endpoint wordt gedaan");
            var response = await Page.Context.APIRequest.GetAsync(
                $"/api/internetaken/by-klantcontact/{Uri.EscapeDataString(klantcontactNummer)}");

            await Step("Then is de HTTP-statuscode 403");
            Assert.AreEqual(403, response.Status, "Expected HTTP 403 when behandelaar has no access to the contactverzoek");

            await Step("And bevat de response body geen inhoudelijke gegevens");
            var body = await response.TextAsync();
            Assert.IsTrue(body.Contains(GeenToegangMelding), "Response body should contain the geen-toegang message");
            Assert.IsFalse(body.Contains(onderwerp), "Response body should not leak the onderwerp");
            Assert.IsFalse(body.Contains(afdelingNaam), "Response body should not leak the afdelingnaam");
            Assert.IsFalse(body.Contains(klantcontactNummer), "Response body should not leak the klantcontactnummer");
            Assert.IsFalse(body.Contains(contactmomentUuid.ToString()), "Response body should not leak the contactmoment UUID");
        }

        [Ignore("Vereist non-admin behandelaar-testaccount — zie class-level comment (Task #544).")]
        [TestMethod("Directe URL-navigatie respecteert autorisatie")]
        public async Task DirecteUrlNavigatie_RespecteertAutorisatie()
        {
            await HandleAuthenticationAsync(
                Configuration["TestSettings:TEST_USERNAME_NON_ADMIN"],
                Configuration["TestSettings:TEST_PASSWORD_NON_ADMIN"]);

            await Step("Given een Contactverzoek toegewezen aan een andere afdeling dan de behandelaar");
            var onderwerp = $"Test_DirecteUrl_{Guid.NewGuid().ToString()[..8]}";
            var (contactmomentUuid, klantcontactNummer, _) = await TestDataHelper.CreateContactverzoekWithAfdelingOnlyAndContactDatum(
                onderwerp, DateTime.UtcNow, AndereAfdelingPlaceholder);
            RegisterCleanup(async () => await TestDataHelper.DeleteContactverzoekAsync(contactmomentUuid.ToString()));

            await Step("When de gebruiker handmatig navigeert naar de directe URL");
            var directUrl = $"/contactmoment/{klantcontactNummer}";
            await Page.GotoAsync(directUrl);

            await Step("Then wordt de geen-toegang-melding getoond en wordt niet doorgestuurd");
            await Expect(Page.GetGeenToegangAlert()).ToBeVisibleAsync();
            await Expect(Page).ToHaveURLAsync(new Regex(Regex.Escape(directUrl)));
        }
    }
}
