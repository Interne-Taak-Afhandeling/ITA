using InterneTaakAfhandeling.EndToEndTest.Infrastructure;
using ITA.InterneTaakAfhandeling.EndToEndTest.Helpers;
using Microsoft.Playwright;

namespace InterneTaakAfhandeling.EndToEndTest.Dashboard
{
    [TestClass]
    [DoNotParallelize]
    public class DigitaleAdressenFallbackScenarios : ITAPlaywrightTest
    {
        // Navigation helpers

        private async Task<string> SetupAndNavigateToContactverzoekWithPartij(string onderwerp)
        {
            await Step("Setup contactverzoek with partij (betrokkene has no own adressen)");

            // Ensure the test partij has digitale adressen (test env might not have them)
            var partijUuid = await TestDataHelper.GetPartijUuidByBsnAsync();
            var existingAdressen = await TestDataHelper.GetPartijDigitaleAdressenAsync(partijUuid);

            if (!existingAdressen.Any(a => a.SoortDigitaalAdres == "email"))
            {
                await Step("Creating email on partij (not present in test env)");
                var emailUuid = await TestDataHelper.CreateDigitaalAdresForPartijAsync(
                    partijUuid, "klant-test@e2e-ita.nl", "email", "E-mail test");
                RegisterCleanup(async () => await TestDataHelper.DeleteDigitaalAdresAsync(emailUuid));
            }

            if (!existingAdressen.Any(a => a.SoortDigitaalAdres == "telefoonnummer"))
            {
                await Step("Creating phone on partij (not present in test env)");
                var phoneUuid = await TestDataHelper.CreateDigitaalAdresForPartijAsync(
                    partijUuid, "0612345678", "telefoonnummer", "Mobiel test");
                RegisterCleanup(async () => await TestDataHelper.DeleteDigitaalAdresAsync(phoneUuid));
            }

            var (contactmomentUuid, nummer) = await TestDataHelper.CreateContactverzoekWithAfdelingMedewerkerAndPartij(onderwerp);
            RegisterCleanup(async () => await TestDataHelper.DeleteContactverzoekAsync(contactmomentUuid.ToString()));

            await Step($"Navigate to contactverzoek by nummer: {nummer}");
            await SafeGotoAsync($"/contactverzoek/{nummer}");
            await Expect(Page.GetContactmomentRegistrerenTab()).ToBeVisibleAsync();

            return nummer;
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
                catch (PlaywrightException)
                {
                    if (attempt < 3) await Task.Delay(1500);
                    else throw;
                }
            }
        }

        private async Task AssertContactDetailsWithRetry(Func<Task> assertions)
        {
            try
            {
                await assertions();
            }
            catch (PlaywrightException)
            {
                await Step("Retrying after page reload (external API may be slow)");
                await Page.ReloadAsync(new() { WaitUntil = WaitUntilState.DOMContentLoaded });
                await Expect(Page.GetContactmomentRegistrerenTab()).ToBeVisibleAsync();
                await assertions();
            }
        }

        // Test methods

        [TestMethod("Partij-adressen worden getoond bij afwezigheid eigen adressen betrokkene")]
        public async Task Medewerker_ZietPartijAdressen_WanneerBetrokkeneGeenEigenAdressenHeeft()
        {
            var onderwerp = $"E2E_Fallback_PartijAdressen_{DateTime.UtcNow.Ticks}";
            await SetupAndNavigateToContactverzoekWithPartij(onderwerp);

            await Step("Verify partij e-mailadres is visible (not empty dash)");
            await AssertContactDetailsWithRetry(async () =>
            {
                var emailValue = Page.GetEmailValue();
                await Expect(emailValue).ToBeVisibleAsync();
                var emailText = await emailValue.InnerTextAsync();
                Assert.AreNotEqual("-", emailText.Trim(),
                    "E-mailadres should show the partij's email, not a dash");
                Assert.IsTrue(emailText.Contains("@"),
                    $"E-mailadres should contain an email address, got: '{emailText}'");
            });

            await Step("Verify partij telefoonnummer is visible (not empty dash)");
            var phoneValue = Page.GetTelefoonnummerValue();
            await Expect(phoneValue).ToBeVisibleAsync();
            var phoneText = await phoneValue.InnerTextAsync();
            Assert.AreNotEqual("-", phoneText.Trim(),
                "Telefoonnummer should show the partij's phone number, not a dash");
        }

        [TestMethod("Eigen adressen betrokkene hebben voorrang boven partij-adressen")]
        public async Task Medewerker_ZietEigenAdressen_WanneerBetrokkeneEigenAdressenHeeft()
        {
            var onderwerp = $"E2E_Fallback_EigenAdressen_{DateTime.UtcNow.Ticks}";
            var ownEmail = "eigen-test@e2e-ita.nl";
            var partijEmail = "partij-fallback@e2e-ita.nl";

            // Ensure the partij has an email so the test actually exercises priority logic
            var partijUuid = await TestDataHelper.GetPartijUuidByBsnAsync();
            var existingAdressen = await TestDataHelper.GetPartijDigitaleAdressenAsync(partijUuid);

            if (!existingAdressen.Any(a => a.SoortDigitaalAdres == "email"))
            {
                await Step("Creating email on partij to exercise priority logic");
                var partijEmailUuid = await TestDataHelper.CreateDigitaalAdresForPartijAsync(
                    partijUuid, partijEmail, "email", "Partij e-mail test");
                RegisterCleanup(async () => await TestDataHelper.DeleteDigitaalAdresAsync(partijEmailUuid));
            }

            await Step("Setup contactverzoek with partij AND own digitale adressen on betrokkene");
            var (contactmomentUuid, _, digitaalAdresUuid) = await TestDataHelper.CreateContactverzoekWithPartijAndOwnAdressenAsync(
                onderwerp, ownEmail);
            RegisterCleanup(async () => await TestDataHelper.DeleteDigitaalAdresAsync(digitaalAdresUuid));
            RegisterCleanup(async () => await TestDataHelper.DeleteContactverzoekAsync(contactmomentUuid.ToString()));

            var internetaakUuid = await TestDataHelper.GetInternetaakUuidFromContactmomentAsync(contactmomentUuid);
            Assert.IsNotNull(internetaakUuid, "Internetaak UUID should be found");

            var internetaak = await TestDataHelper.GetInternetaakByIdAsync(internetaakUuid.Value);
            Assert.IsNotNull(internetaak.Nummer, "Internetaak nummer should be found");

            await Step($"Navigate to contactverzoek by nummer: {internetaak.Nummer}");
            await SafeGotoAsync($"/contactverzoek/{internetaak.Nummer}");
            await Expect(Page.GetContactmomentRegistrerenTab()).ToBeVisibleAsync();

            await Step("Verify only own e-mailadres of betrokkene is shown");
            await AssertContactDetailsWithRetry(async () =>
            {
                var emailValue = Page.GetEmailValue();
                await Expect(emailValue).ToBeVisibleAsync();
                await Expect(emailValue).ToHaveTextAsync(ownEmail);
            });
        }

        [TestMethod("Lege contactsectie zonder partij-koppeling")]
        public async Task Medewerker_ZietLegeContactsectie_WanneerGeenPartijEnGeenAdressen()
        {
            var onderwerp = $"E2E_Fallback_GeenPartij_{DateTime.UtcNow.Ticks}";

            await Step("Setup contactverzoek without partij (no digitale adressen)");
            var contactmomentUuid = await TestDataHelper.CreateContactverzoek(onderwerp, attachZaak: false);
            RegisterCleanup(async () => await TestDataHelper.DeleteContactverzoekAsync(contactmomentUuid.ToString()));

            var internetaakUuid = await TestDataHelper.GetInternetaakUuidFromContactmomentAsync(contactmomentUuid);
            Assert.IsNotNull(internetaakUuid, "Internetaak UUID should be found");

            var internetaak = await TestDataHelper.GetInternetaakByIdAsync(internetaakUuid.Value);
            Assert.IsNotNull(internetaak.Nummer, "Internetaak nummer should be found");

            await Step($"Navigate to contactverzoek by nummer: {internetaak.Nummer}");
            await SafeGotoAsync($"/contactverzoek/{internetaak.Nummer}");
            await Expect(Page.GetContactmomentRegistrerenTab()).ToBeVisibleAsync();

            await Step("Verify contactsectie shows dashes (empty state)");
            await Expect(Page.GetEmailLabel()).ToBeVisibleAsync();
            await Expect(Page.GetEmailValue()).ToHaveTextAsync("-");

            await Expect(Page.GetTelefoonnummerLabel()).ToBeVisibleAsync();
            await Expect(Page.GetTelefoonnummerValue()).ToHaveTextAsync("-");
        }

        [TestMethod("Lege contactsectie wanneer partij geen adressen heeft")]
        public async Task Medewerker_ZietLegeContactsectie_WanneerPartijGeenAdressenHeeft()
        {
            // This scenario requires a partij without digitale adressen in the test environment.
            // Setup: creates a partij that exists in OpenKlant but has no digitale adressen.
            // If no such partij can be created, this test is marked Inconclusive.

            var onderwerp = $"E2E_Fallback_PartijZonderAdressen_{DateTime.UtcNow.Ticks}";

            await Step("Create a partij without digitale adressen");
            string? partijUuid;
            try
            {
                partijUuid = await TestDataHelper.CreatePartijWithoutAdressenAsync();
                RegisterCleanup(async () => await TestDataHelper.DeletePartijAsync(partijUuid));
            }
            catch (Exception ex)
            {
                Assert.Inconclusive(
                    $"Could not create partij without adressen in test environment: {ex.Message}.");
                return;
            }

            await Step("Setup contactverzoek with addressless partij");
            var contactmomentUuid = await TestDataHelper.CreateContactverzoekWithSpecificPartijAsync(
                onderwerp, partijUuid);
            RegisterCleanup(async () => await TestDataHelper.DeleteContactverzoekAsync(contactmomentUuid.ToString()));

            var internetaakUuid = await TestDataHelper.GetInternetaakUuidFromContactmomentAsync(contactmomentUuid);
            Assert.IsNotNull(internetaakUuid, "Internetaak UUID should be found");

            var internetaak = await TestDataHelper.GetInternetaakByIdAsync(internetaakUuid.Value);
            Assert.IsNotNull(internetaak.Nummer, "Internetaak nummer should be found");

            await Step($"Navigate to contactverzoek by nummer: {internetaak.Nummer}");
            await SafeGotoAsync($"/contactverzoek/{internetaak.Nummer}");
            await Expect(Page.GetContactmomentRegistrerenTab()).ToBeVisibleAsync();

            await Step("Verify contactsectie shows dashes (partij has no adressen)");
            await AssertContactDetailsWithRetry(async () =>
            {
                await Expect(Page.GetEmailValue()).ToHaveTextAsync("-");
                await Expect(Page.GetTelefoonnummerValue()).ToHaveTextAsync("-");
            });
        }

        [TestMethod("Geen herkomst-indicatie zichtbaar bij partij-adressen")]
        public async Task Medewerker_ZietGeenHerkomstIndicatie_WanneerPartijAdressenGetoond()
        {
            var onderwerp = $"E2E_Fallback_GeenHerkomst_{DateTime.UtcNow.Ticks}";
            await SetupAndNavigateToContactverzoekWithPartij(onderwerp);

            await Step("Verify no source indication is visible (no 'partij' or 'bron' label)");
            await AssertContactDetailsWithRetry(async () =>
            {
                // The contact section should show email and phone without any indication
                // that the data comes from the partij rather than the betrokkene.
                var contactSection = Page.Locator("div.ita-data-list__group").First;
                var sectionText = await contactSection.InnerTextAsync();

                Assert.IsFalse(sectionText.Contains("partij", StringComparison.OrdinalIgnoreCase),
                    $"Contact section should not contain 'partij' source indication. Text: '{sectionText}'");
                Assert.IsFalse(sectionText.Contains("bron", StringComparison.OrdinalIgnoreCase),
                    $"Contact section should not contain 'bron' source indication. Text: '{sectionText}'");
                Assert.IsFalse(sectionText.Contains("herkomst", StringComparison.OrdinalIgnoreCase),
                    $"Contact section should not contain 'herkomst' source indication. Text: '{sectionText}'");

                // Verify the standard field labels are present (Klantnaam, E-mailadres, Telefoonnummer)
                await Expect(Page.GetKlantnaamLabel()).ToBeVisibleAsync();
                await Expect(Page.GetEmailLabel()).ToBeVisibleAsync();
                await Expect(Page.GetTelefoonnummerLabel()).ToBeVisibleAsync();
            });
        }
    }
}
