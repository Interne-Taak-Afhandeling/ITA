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
        public override async Task TestInitialize()
        {
            await base.TestInitialize();

            locators = new BeheerLocators(Page);
            
            // Pre-cleanup: Remove test data that may exist from previous failed test runs
            await TryDeleteKanaal(TestKanaalName);
            await TryDeleteKanaal(EditedKanaalName);

            // Register post-test cleanup via the base class pattern
            RegisterCleanup(async () =>
            {
                foreach (var kanaalName in kanalenToCleanup)
                {
                    await TryDeleteKanaal(kanaalName);
                }
                kanalenToCleanup.Clear();
            });
        }


        private async Task<bool> TryDeleteKanaal(string kanaalName)
        {
            try
            {
                await Page.GotoAsync("/");
                await locators.BeheerLink.ClickAsync();
                await locators.KanalenLink.ClickAsync();
                
                var count = await locators.GetKanaalListItem(kanaalName).CountAsync();
                if (count == 0)
                {
                    return true; 
                }

                void AcceptDialog(object? sender, IDialog dialog) => dialog.AcceptAsync();
                Page.Dialog += AcceptDialog;
                try
                {
                    await locators.GetDeleteButton(kanaalName).ClickAsync();
                }
                finally
                {
                    Page.Dialog -= AcceptDialog;
                }
                
                await Expect(locators.GetKanaalListItem(kanaalName)).ToHaveCountAsync(0);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cleanup failed for '{kanaalName}'");
                return false;
            }
        }

        private async Task NavigateToKanalenPage()
        {
            await Step("Navigate to home page");
            await Page.GotoAsync("/");

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
        }

        private async Task DeleteKanaal(string kanaalName)
        {
            await Step($"Delete kanaal: {kanaalName}");
            void AcceptDialog(object? sender, IDialog dialog) => dialog.AcceptAsync();
            Page.Dialog += AcceptDialog;
            try
            {
                await locators.GetDeleteButton(kanaalName).ClickAsync();
            }
            finally
            {
                Page.Dialog -= AcceptDialog;
            }
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
        }

        [TestMethod("Delete Kanalen")]
        public async Task User_DeleteKanalen_FromDashboard()
        {
            await NavigateToKanalenPage();

            await CreateKanaal(TestKanaalName);
            kanalenToCleanup.Add(TestKanaalName);

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

            // After a successful save the app redirects to the kanalen list — verify there
            // without navigating away first, which would race against the in-progress redirect
            await Expect(locators.GetKanaalLink(TestKanaalName)).ToBeVisibleAsync();

            await Step($"Attempt to create duplicate kanaal: {TestKanaalName}");
            await CreateKanaal(TestKanaalName);

            await Step("Verify error message is displayed");
            var errorMessage = Page.Locator(".preserve-newline").Filter(new() { HasText = $"Er bestaat al een kanaal met de naam {TestKanaalName}" });
            await Expect(errorMessage).ToBeVisibleAsync();
        }
    }
}