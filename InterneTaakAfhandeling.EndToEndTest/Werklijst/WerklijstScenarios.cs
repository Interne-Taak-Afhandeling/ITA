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
            var werklijstLink = Page.GetWerklijstNavLink();
            await Expect(werklijstLink).ToBeVisibleAsync();

            await Step("Click werklijst link");
            await werklijstLink.ClickAsync();

            await Step("Verify werklijst page loads");
            var heading = Page.GetWerklijstHeading();
            await Expect(heading).ToBeVisibleAsync();
        }

        [TestMethod("Werklijst displays table with data")]
        public async Task WerklijstDisplaysTableWithData()
        {
            await Step("Navigate to werklijst page");
            await Page.GotoAsync("/werklijst");

            await Step("Verify werklijst heading is present");
            var heading = Page.GetWerklijstHeading();
            await Expect(heading).ToBeVisibleAsync();

            await Step("Verify werklijst table is visible");
            var table = Page.GetWerklijstTable();
            await Expect(table).ToBeVisibleAsync();

            await Step("Verify table has rows");
            var rows = Page.GetWerklijstTableRows();
            var rowCount = await rows.CountAsync();
            Assert.IsTrue(rowCount > 0, "Werklijst table should contain at least one row");
        }
    }
}
