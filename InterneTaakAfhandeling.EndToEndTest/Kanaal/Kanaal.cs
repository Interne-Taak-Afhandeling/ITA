using System.Text.RegularExpressions;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.EndToEndTest.Infrastructure;
using ITA.InterneTaakAfhandeling.EndToEndTest.Helpers;
using InterneTaakAfhandeling.EndToEndTest.Dashboard;
using Microsoft.Playwright;

namespace InterneTaakAfhandeling.EndToEndTest.Kanaal
{
    [TestClass]
    [DoNotParallelize]
    public class KanalenScenarios : ITAPlaywrightTest
    {
        private const string TestKanaalName = "test_delete_123";
        private const string EditedKanaalName = "test_automation_456";

        private BeheerLocators locators = null!;
        private readonly List<string> kanalenToCleanup = new();

        [TestInitialize]
        public void TestInitialize()
        {
            locators = new BeheerLocators(Page);
        }

        [TestCleanup]
        public async Task TestCleanup()
        {
            foreach (var kanaalName in kanalenToCleanup)
            {
                try
                {
                    await Page.GotoAsync("/");
                    await locators.BeheerLink.ClickAsync();
                    await locators.KanalenLink.ClickAsync();
                    
                    var kanaalExists = await locators.GetKanaalListItem(kanaalName).CountAsync() > 0;
                    if (kanaalExists)
                    {
                        Page.Dialog += (_, dialog) => dialog.AcceptAsync();
                        await locators.GetDeleteButton(kanaalName).ClickAsync();
                        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
                    }
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
            kanalenToCleanup.Clear();
        }

        private async Task NavigateToKanalenPage()
        {
            await Step("Navigate to home page");
            await Page.GotoAsync("/");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("Navigate to Kanalen via Beheer menu");
            await locators.BeheerLink.ClickAsync();
            await locators.KanalenLink.ClickAsync();
        }

        private async Task CreateKanaal(string kanaalName)
        {
            await Step($"Create new kanaal: {kanaalName}");
            await locators.KanaalToevoegenLink.ClickAsync();
            await locators.NaamTextbox.FillAsync(kanaalName);
            await locators.OpslaanButton.ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }

        private async Task DeleteKanaal(string kanaalName)
        {
            await Step($"Delete kanaal: {kanaalName}");
            Page.Dialog += (_, dialog) => dialog.AcceptAsync();
            await locators.GetDeleteButton(kanaalName).ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }

        private async Task VerifyKanaalExists(string kanaalName)
        {
            await Step($"Verify kanaal '{kanaalName}' exists");
            await NavigateToKanalenPage();
            await Expect(locators.GetKanaalLink(kanaalName)).ToBeVisibleAsync();
        }

        private async Task EditKanaalName(string currentName, string newName)
        {
            await Step($"Edit kanaal from '{currentName}' to '{newName}'");
            await locators.GetKanaalLink(currentName).ClickAsync();
            await locators.NaamTextbox.FillAsync(newName);
            await locators.OpslaanButton.ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }

        // Test Methods

        [TestMethod("Navigate to Beheer of Kanalen")]
        public async Task User_NavigateToKanalen_FromDashboard()
        {
            await NavigateToKanalenPage();

            await Expect(locators.KanalenHeading).ToBeVisibleAsync();
            await Expect(locators.BeheerSubmenu).ToBeVisibleAsync();
        }

        [TestMethod("Add Kanalen")]
        public async Task User_AddKanalen_FromDashboard()
        {
            await NavigateToKanalenPage();
            
            await CreateKanaal(TestKanaalName);
            kanalenToCleanup.Add(TestKanaalName);
            
            await VerifyKanaalExists(TestKanaalName);

            await DeleteKanaal(TestKanaalName);
            kanalenToCleanup.Remove(TestKanaalName);
        }

        [TestMethod("Edit Kanalen")]
        public async Task User_EditKanalen_FromDashboard()
        {
            await NavigateToKanalenPage();

            await CreateKanaal(TestKanaalName);
            kanalenToCleanup.Add(EditedKanaalName);

            await NavigateToKanalenPage();
            await EditKanaalName(TestKanaalName, EditedKanaalName);

            await VerifyKanaalExists(EditedKanaalName);

            await DeleteKanaal(EditedKanaalName);
            kanalenToCleanup.Remove(EditedKanaalName);
        }

        [TestMethod("Delete Kanalen")]
        public async Task User_DeleteKanalen_FromDashboard()
        {
            await NavigateToKanalenPage();

            await CreateKanaal(TestKanaalName);

            await NavigateToKanalenPage();

            await DeleteKanaal(TestKanaalName);

            await Step("Verify kanaal was deleted");
            await Expect(locators.GetKanaalListItem(TestKanaalName)).ToHaveCountAsync(0);
        }

        [TestMethod("Error message shown when Kanaal name already exists")]
        public async Task User_SeesError_WhenCreatingDuplicateKanaal()
        {
            await NavigateToKanalenPage();

            await CreateKanaal(TestKanaalName);
            kanalenToCleanup.Add(TestKanaalName);
            
            await VerifyKanaalExists(TestKanaalName);
            
            await Step($"Attempt to create duplicate kanaal: {TestKanaalName}");
            await locators.KanaalToevoegenLink.ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await locators.NaamTextbox.FillAsync(TestKanaalName);
            await locators.OpslaanButton.ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("Verify error message is displayed");
            var errorMessage = Page.Locator(".preserve-newline").Filter(new() { HasText = $"Er bestaat al een kanaal met de naam {TestKanaalName}" });
            await Expect(errorMessage).ToBeVisibleAsync(new() { Timeout = 10000 });

            await Step("Cancel and delete the kanaal for cleanup");
            await locators.AnnulerenButton.ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await DeleteKanaal(TestKanaalName);
            kanalenToCleanup.Remove(TestKanaalName);
        }
    }
}