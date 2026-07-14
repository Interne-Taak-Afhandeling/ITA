using System.Text.RegularExpressions;
using InterneTaakAfhandeling.EndToEndTest.Infrastructure;
using Microsoft.Playwright;

namespace InterneTaakAfhandeling.EndToEndTest.Werklijst
{
    [TestClass]
    [DoNotParallelize]
    public class WerklijstUrgentieScenarios : ITAPlaywrightTest
    {
        private const string MijnWerkvoorraadPath = "/";
        private const string AlleContactverzoekenPath = "/alle-contactverzoeken";

        private async Task GotoAsync(string path)
        {
            PlaywrightException? lastException = null;
            for (var attempt = 1; attempt <= 3; attempt++)
            {
                try
                {
                    await Page.GotoAsync(path);
                    return;
                }
                catch (PlaywrightException ex)
                {
                    lastException = ex;
                    if (attempt < 3)
                    {
                        await Task.Delay(1500);
                    }
                }
            }
            throw new InvalidOperationException($"Failed to navigate to '{path}' after 3 attempts.", lastException);
        }

        // --- Urgentie badge scenarios (uit Task #504) ---

        [TestMethod("Contactverzoek binnen afhandeltermijn toont groen label")]
        public async Task User_SeesGroenLabel_WhenBinnenAfhandeltermijn()
        {
            const string onderwerp = "E2E_Werklijst_Urgentie_Groen";
            var now = DateTime.UtcNow;
            var contactDatum = BusinessHours.SubtractBusinessHours(now, hours: 10);

            await Step("Setup contactverzoek with ContactDatum ~10 business hours ago");
            var (uuid, _, _) = await TestDataHelper.CreateContactverzoekWithContactDatum(onderwerp, contactDatum);
            RegisterCleanup(async () => await TestDataHelper.DeleteContactverzoekAsync(uuid.ToString()));

            await Step("Navigate to Mijn werkvoorraad");
            await GotoAsync(MijnWerkvoorraadPath);

            await AssertUrgentieBadgeMatchesOracle(onderwerp, contactDatum);
        }

        [TestMethod("Contactverzoek bijna verlopen toont oranje label")]
        public async Task User_SeesOranjeLabel_WhenBijnaVerlopen()
        {
            const string onderwerp = "E2E_Werklijst_Urgentie_Oranje";
            var now = DateTime.UtcNow;
            var contactDatum = BusinessHours.SubtractBusinessHours(now, hours: 44);

            await Step("Setup contactverzoek with ContactDatum ~44 business hours ago (4h remaining)");
            var (uuid, _, _) = await TestDataHelper.CreateContactverzoekWithContactDatum(onderwerp, contactDatum);
            RegisterCleanup(async () => await TestDataHelper.DeleteContactverzoekAsync(uuid.ToString()));

            await Step("Navigate to Mijn werkvoorraad");
            await GotoAsync(MijnWerkvoorraadPath);

            await AssertUrgentieBadgeMatchesOracle(onderwerp, contactDatum);
        }

        [TestMethod("Contactverzoek verlopen toont rood label")]
        public async Task User_SeesRoodLabel_WhenVerlopen()
        {
            const string onderwerp = "E2E_Werklijst_Urgentie_Rood";
            var now = DateTime.UtcNow;
            var contactDatum = BusinessHours.SubtractBusinessHours(now, hours: 53);

            await Step("Setup contactverzoek with ContactDatum ~53 business hours ago (5h verlopen)");
            var (uuid, _, _) = await TestDataHelper.CreateContactverzoekWithContactDatum(onderwerp, contactDatum);
            RegisterCleanup(async () => await TestDataHelper.DeleteContactverzoekAsync(uuid.ToString()));

            await Step("Navigate to Mijn werkvoorraad");
            await GotoAsync(MijnWerkvoorraadPath);

            await AssertUrgentieBadgeMatchesOracle(onderwerp, contactDatum);
        }

        [TestMethod("Weekenden uitgesloten van berekening")]
        public async Task User_SeesWeekendExcluded_InUrgentieBerekening()
        {
            // 130 business hours is more than 5 full weekdays (max 120h), so subtracting it from
            // `now` is guaranteed to cross at least one full weekend regardless of which weekday
            // the suite runs on. AssertUrgentieBadgeMatchesOracle's expected value is computed
            // with the same weekend-skipping arithmetic as the app - if the app instead used
            // naive calendar-hour math, the actual label would diverge from this expectation by
            // roughly 48h (2d) per weekend crossed, making this a real regression check rather
            // than a tautology.
            const string onderwerp = "E2E_Werklijst_Urgentie_WeekendUitgesloten";
            var now = DateTime.UtcNow;
            var contactDatum = BusinessHours.SubtractBusinessHours(now, hours: 130);

            await Step("Setup contactverzoek with ContactDatum spanning at least one full weekend");
            var (uuid, _, _) = await TestDataHelper.CreateContactverzoekWithContactDatum(onderwerp, contactDatum);
            RegisterCleanup(async () => await TestDataHelper.DeleteContactverzoekAsync(uuid.ToString()));

            await Step("Navigate to Mijn werkvoorraad");
            await GotoAsync(MijnWerkvoorraadPath);

            await AssertUrgentieBadgeMatchesOracle(onderwerp, contactDatum);
        }

        [TestMethod("Ceiling-afronding voorkomt \"0u\" label")]
        public async Task User_SeesCeilingRounding_PreventsZeroUurLabel()
        {
            const string onderwerp = "E2E_Werklijst_Urgentie_CeilingAfronding";
            var now = DateTime.UtcNow;
            // 47.5 business hours elapsed leaves 0.5h remaining - comfortably inside the (0h, 1h]
            // bucket that must ceiling to "nog 1u", with a 30-minute safety margin against
            // execution drift between fixture creation and assertion.
            var contactDatum = BusinessHours.SubtractBusinessHours(now, hours: 47.5);

            await Step("Setup contactverzoek with ContactDatum leaving ~0.5 business hours remaining");
            var (uuid, _, _) = await TestDataHelper.CreateContactverzoekWithContactDatum(onderwerp, contactDatum);
            RegisterCleanup(async () => await TestDataHelper.DeleteContactverzoekAsync(uuid.ToString()));

            await Step("Navigate to Mijn werkvoorraad");
            await GotoAsync(MijnWerkvoorraadPath);

            await Step("Verify label reads 'nog 1u', not 'nog 0u'");
            await Expect(Page.GetUrgentieBadge(onderwerp)).ToHaveTextAsync("nog 1u");
        }

        [TestMethod("Urgentielabel zichtbaar op beide werklijstweergaven")]
        public async Task User_SeesUrgentieBadge_OnBothWerklijstViews()
        {
            const string onderwerp = "E2E_Werklijst_Urgentie_BeideWeergaven";
            var contactDatum = BusinessHours.SubtractBusinessHours(DateTime.UtcNow, hours: 10);

            await Step("Setup contactverzoek assigned to current user");
            var (uuid, _, _) = await TestDataHelper.CreateContactverzoekWithContactDatum(onderwerp, contactDatum);
            RegisterCleanup(async () => await TestDataHelper.DeleteContactverzoekAsync(uuid.ToString()));

            await Step("Verify urgentielabel is visible on Mijn werkvoorraad");
            await GotoAsync(MijnWerkvoorraadPath);
            await Expect(Page.GetUrgentieBadge(onderwerp)).ToBeVisibleAsync();

            await Step("Verify urgentielabel is visible on Alle contactverzoeken");
            await GotoAsync(AlleContactverzoekenPath);
            await Expect(Page.GetUrgentieBadge(onderwerp)).ToBeVisibleAsync();
        }

        // --- Kolommen scenarios (uit Task #505) ---

        [TestMethod("Kolom Afdeling zichtbaar op Mijn werkvoorraad")]
        public async Task User_SeesAfdelingColumn_OnMijnWerkvoorraad()
        {
            const string onderwerp = "E2E_Werklijst_Kolom_Afdeling";

            await Step("Setup contactverzoek with a known afdeling");
            var (uuid, _, afdelingNaam) = await TestDataHelper.CreateContactverzoekWithContactDatum(onderwerp, DateTime.UtcNow);
            RegisterCleanup(async () => await TestDataHelper.DeleteContactverzoekAsync(uuid.ToString()));

            await Step("Navigate to Mijn werkvoorraad");
            await GotoAsync(MijnWerkvoorraadPath);

            await Step("Verify Afdeling column header is visible");
            await Expect(Page.GetColumnHeader("Afdeling")).ToBeVisibleAsync();

            await Step($"Verify Afdeling cell shows '{afdelingNaam}'");
            var afdelingCell = await Page.GetColumnCellAsync(onderwerp, "Afdeling");
            await Expect(afdelingCell).ToHaveTextAsync(afdelingNaam);
        }

        [TestMethod("Kolom Klantcontactnummer zichtbaar op alle werklijsten")]
        public async Task User_SeesKlantcontactnummerColumn_OnBothWerklijsten()
        {
            const string onderwerp = "E2E_Werklijst_Kolom_Klantcontactnummer";

            await Step("Setup contactverzoek");
            var (uuid, klantcontactNummer, _) = await TestDataHelper.CreateContactverzoekWithContactDatum(onderwerp, DateTime.UtcNow);
            RegisterCleanup(async () => await TestDataHelper.DeleteContactverzoekAsync(uuid.ToString()));

            await Step("Verify Klantcontactnummer column on Mijn werkvoorraad");
            await GotoAsync(MijnWerkvoorraadPath);
            await Expect(Page.GetColumnHeader("Klantcontactnummer")).ToBeVisibleAsync();
            var myCell = await Page.GetColumnCellAsync(onderwerp, "Klantcontactnummer");
            await Expect(myCell).ToHaveTextAsync(klantcontactNummer);

            await Step("Verify Klantcontactnummer column on Alle contactverzoeken");
            await GotoAsync(AlleContactverzoekenPath);
            await Expect(Page.GetColumnHeader("Klantcontactnummer")).ToBeVisibleAsync();
            var allCell = await Page.GetColumnCellAsync(onderwerp, "Klantcontactnummer");
            await Expect(allCell).ToHaveTextAsync(klantcontactNummer);
        }

        [TestMethod("Contactverzoek zonder afdeling toont lege cel")]
        public async Task User_SeesEmptyCell_WhenAfdelingMissing()
        {
            const string onderwerp = "E2E_Werklijst_Kolom_LegeAfdeling";

            await Step("Setup contactverzoek with no afdeling actor assigned");
            var (uuid, _) = await TestDataHelper.CreateContactverzoekWithCurrentUserAssignedNoAfdeling(onderwerp);
            RegisterCleanup(async () => await TestDataHelper.DeleteContactverzoekAsync(uuid.ToString()));

            await Step("Navigate to Mijn werkvoorraad");
            await GotoAsync(MijnWerkvoorraadPath);

            await Step("Verify Afdeling cell shows '-' instead of erroring");
            var afdelingCell = await Page.GetColumnCellAsync(onderwerp, "Afdeling");
            await Expect(afdelingCell).ToHaveTextAsync("-");
        }

        // --- Klikbare rijen scenarios (uit Task #506) ---

        [TestMethod("Klikbare rij opent contactverzoek")]
        public async Task User_ClickingRow_OpensContactverzoek()
        {
            const string onderwerp = "E2E_Werklijst_KlikbareRij_Navigatie";

            await Step("Setup contactverzoek");
            var (uuid, _, _) = await TestDataHelper.CreateContactverzoekWithContactDatum(onderwerp, DateTime.UtcNow);
            RegisterCleanup(async () => await TestDataHelper.DeleteContactverzoekAsync(uuid.ToString()));

            await Step("Navigate to Mijn werkvoorraad");
            await GotoAsync(MijnWerkvoorraadPath);

            await Step("Click the row away from the arrow link");
            await Page.GetWerklijstRow(onderwerp).GetByText(onderwerp).ClickAsync();

            await Step("Verify the contactverzoek detail screen opened");
            await Expect(Page.GetContactmomentRegistrerenTab()).ToBeVisibleAsync();
        }

        [TestMethod("Rij toont hover-effect en pointer")]
        public async Task User_HoversRow_SeesPointerCursorAndBackgroundChange()
        {
            const string onderwerp = "E2E_Werklijst_KlikbareRij_Hover";

            await Step("Setup contactverzoek");
            var (uuid, _, _) = await TestDataHelper.CreateContactverzoekWithContactDatum(onderwerp, DateTime.UtcNow);
            RegisterCleanup(async () => await TestDataHelper.DeleteContactverzoekAsync(uuid.ToString()));

            await Step("Navigate to Mijn werkvoorraad");
            await GotoAsync(MijnWerkvoorraadPath);

            var row = Page.GetWerklijstRow(onderwerp);

            await Step("Verify cursor is pointer on the row");
            await Expect(row).ToHaveCSSAsync("cursor", "pointer");

            // Move the mouse away first - the virtual cursor position persists across
            // navigations within the same page, so a previous test's click/hover could leave it
            // sitting over this row and make "before" already look hovered.
            await Step("Move mouse away from the row before capturing baseline");
            await Page.Mouse.MoveAsync(0, 0);

            await Step("Capture background color before hover");
            var backgroundBefore = await row.EvaluateAsync<string>("el => getComputedStyle(el).backgroundColor");

            await Step("Hover over the row");
            await row.HoverAsync();
            var backgroundAfter = await row.EvaluateAsync<string>("el => getComputedStyle(el).backgroundColor");

            Assert.AreNotEqual(backgroundBefore, backgroundAfter, "Row background color should change on hover");
        }

        [TestMethod("Link in rij toont pijl in plaats van \"Klik hier\"")]
        public async Task User_SeesArrowLink_WithDescriptiveAriaLabel()
        {
            const string onderwerp = "E2E_Werklijst_KlikbareRij_PijlLink";

            await Step("Setup contactverzoek");
            var (uuid, _, _) = await TestDataHelper.CreateContactverzoekWithContactDatum(onderwerp, DateTime.UtcNow);
            RegisterCleanup(async () => await TestDataHelper.DeleteContactverzoekAsync(uuid.ToString()));

            await Step("Navigate to Mijn werkvoorraad");
            await GotoAsync(MijnWerkvoorraadPath);

            var detailsLink = Page.GetDetailsLink(onderwerp);

            await Step("Verify the link shows '→'");
            await Expect(detailsLink).ToHaveTextAsync("→");

            await Step("Verify the link has a descriptive aria-label");
            var ariaLabel = await detailsLink.GetAttributeAsync("aria-label");
            Assert.IsNotNull(ariaLabel);
            Assert.IsTrue(ariaLabel.StartsWith("Open contactverzoek", StringComparison.Ordinal), $"aria-label should describe the contactverzoek, was '{ariaLabel}'");
        }

        // --- Shared assertion helper ---

        private async Task AssertUrgentieBadgeMatchesOracle(string onderwerp, DateTime contactDatum)
        {
            var badge = Page.GetUrgentieBadge(onderwerp);
            await Expect(badge).ToBeVisibleAsync();

            var assertionTime = DateTime.UtcNow;
            var elapsed = BusinessHours.ElapsedBusinessHours(contactDatum, assertionTime);
            var resterend = BusinessHours.AfhandeltermijnUren - elapsed;
            var expectedStatusClass = BusinessHours.ExpectedStatusClass(resterend);
            var expectedLabel = BusinessHours.ExpectedLabel(resterend);

            await Step($"Verify badge has status class '{expectedStatusClass}' and label '{expectedLabel}'");
            await Expect(badge).ToHaveClassAsync(new Regex($@"\b{Regex.Escape(expectedStatusClass)}\b"));
            await Expect(badge).ToHaveTextAsync(expectedLabel);
        }
    }
}
