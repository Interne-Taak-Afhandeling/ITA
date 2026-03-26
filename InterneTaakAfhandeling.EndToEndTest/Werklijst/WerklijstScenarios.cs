using InterneTaakAfhandeling.EndToEndTest.Infrastructure;

namespace InterneTaakAfhandeling.EndToEndTest.Werklijst
{
    [TestClass]
    [DoNotParallelize]
    public class WerklijstScenarios : ITAPlaywrightTest
    {
        [TestMethod("Coordinator can navigate to werklijst")]
        public async Task CoordinatorCanNavigateToWerklijst()
        {
            await Step("Navigate to home page");
            await Page.GotoAsync("/");

            await Step("Verify werklijst navigation link is visible");
            var werklijstLink = GetWerklijstNavLink(Page);
            await Expect(werklijstLink).ToBeVisibleAsync();

            await Step("Click werklijst link");
            await werklijstLink.ClickAsync();

            await Step("Verify werklijst page loads");
            var heading = GetWerklijstHeading(Page);
            await Expect(heading).ToBeVisibleAsync();
        }

        [TestMethod("Werklijst displays table with data")]
        public async Task WerklijstDisplaysTableWithData()
        {
            await Step("Navigate to werklijst page");
            await Page.GotoAsync("/werklijst");

            await Step("Verify werklijst heading is present");
            var heading = GetWerklijstHeading(Page);
            await Expect(heading).ToBeVisibleAsync();

            await Step("Verify werklijst table is visible");
            var table = GetWerklijstTable(Page);
            await Expect(table).ToBeVisibleAsync();

            await Step("Verify table has rows");
            var rows = GetWerklijstTableRows(Page);
            var rowCount = await rows.CountAsync();
            Assert.IsTrue(rowCount > 0, "Werklijst table should contain at least one row");
        }

        private static Microsoft.Playwright.ILocator GetWerklijstNavLink(Microsoft.Playwright.IPage page) =>
            WerklijstLocators.GetWerklijstNavLink(page);

        private static Microsoft.Playwright.ILocator GetWerklijstHeading(Microsoft.Playwright.IPage page) =>
            WerklijstLocators.GetWerklijstHeading(page);

        private static Microsoft.Playwright.ILocator GetWerklijstTable(Microsoft.Playwright.IPage page) =>
            WerklijstLocators.GetWerklijstTable(page);

        private static Microsoft.Playwright.ILocator GetWerklijstTableRows(Microsoft.Playwright.IPage page) =>
            WerklijstLocators.GetWerklijstTableRows(page);
    }
}
