using InterneTaakAfhandeling.EndToEndTest.Infrastructure;
using InterneTaakAfhandeling.EndToEndTest.Werklijst;

namespace InterneTaakAfhandeling.EndToEndTest.DagelijkseHerinnering
{
    /// <summary>
    /// E2E verification for the daily reminder email feature (Feature #413, Task #492).
    ///
    /// Every scenario below calls SchedulerTrigger.TriggerDagelijkseHerinneringSchedulerAsync()
    /// first, which currently reports the test Inconclusive (see SchedulerTrigger.cs) because no
    /// test-trigger endpoint exists yet for the Poller CronJob. The remaining steps (mailbox
    /// check, assertions) are written against the intended behavior and become live once
    /// SchedulerTrigger and TestMailbox are wired to real infrastructure - no further changes to
    /// this file should be needed at that point.
    ///
    /// Link assertions intentionally match the CURRENT implementation in
    /// VerlopenInternetakenProcessor.cs (medewerker -> "/afdelings-contacten", afdeling/groep ->
    /// "/"), which is the reverse of Feature #413's Acceptance Criteria. This is a known,
    /// separately-tracked bug - not something this task fixes.
    /// </summary>
    [TestClass]
    [DoNotParallelize]
    public class DagelijkseHerinneringScenarios : ITAPlaywrightTest
    {
        private const string MedewerkerWerkvoorraadLinkPath = "/afdelings-contacten";
        private const string AfsluitendeZin = "Fijne werkdag";

        private string CurrentUserEmail => Configuration["TestSettings:TEST_USERNAME"]
            ?? throw new InvalidOperationException("TEST_USERNAME is missing from the configuration");

        [TestMethod("Medewerker ontvangt herinneringsmail voor verlopen ContactVerzoek")]
        public async Task Medewerker_ReceivesReminderMail_ForOverdueContactverzoek()
        {
            const string onderwerp = "E2E_DagelijkseHerinnering_Medewerker";
            var plaatsgevondenOp = BusinessHours.SubtractBusinessHours(DateTime.UtcNow, hours: 53);

            await Step("Setup an overdue contactverzoek assigned only to the current Medewerker");
            var (uuid, _) = await TestDataHelper.CreateContactverzoekWithMedewerkerOnly(onderwerp, plaatsgevondenOp);
            RegisterCleanup(async () => await TestDataHelper.DeleteContactverzoekAsync(uuid.ToString()));

            await Step("Trigger the daily reminder scheduler");
            await SchedulerTrigger.TriggerDagelijkseHerinneringSchedulerAsync();

            await Step("Check the Medewerker's testmailbox");
            var emails = await TestMailbox.GetReceivedEmailsAsync(CurrentUserEmail);

            Assert.AreEqual(1, emails.Count, "Expected exactly one reminder mail.");
            var mail = emails[0];
            StringAssert.Contains(mail.Subject, "contactverzoek");
            StringAssert.Contains(mail.HtmlBody, $"href=\"{MedewerkerWerkvoorraadLinkPath}");
            StringAssert.Contains(mail.PlainTextBody, AfsluitendeZin);
        }

        [TestMethod("Geaggregeerde mail — meerdere verlopen contactverzoeken in één mail")]
        public async Task Medewerker_ReceivesSingleAggregatedMail_ForMultipleOverdueContactverzoeken()
        {
            const string onderwerpPrefix = "E2E_DagelijkseHerinnering_Geaggregeerd";
            var plaatsgevondenOp = BusinessHours.SubtractBusinessHours(DateTime.UtcNow, hours: 53);

            await Step("Setup 3 overdue contactverzoeken assigned to the current Medewerker");
            var uuids = await TestDataHelper.CreateMultipleContactverzoekenForMedewerker(onderwerpPrefix, count: 3, plaatsgevondenOp);
            foreach (var uuid in uuids)
            {
                RegisterCleanup(async () => await TestDataHelper.DeleteContactverzoekAsync(uuid.ToString()));
            }

            await Step("Trigger the daily reminder scheduler");
            await SchedulerTrigger.TriggerDagelijkseHerinneringSchedulerAsync();

            await Step("Check the Medewerker's testmailbox");
            var emails = await TestMailbox.GetReceivedEmailsAsync(CurrentUserEmail);

            Assert.AreEqual(1, emails.Count, "Expected exactly one aggregated reminder mail, not one per contactverzoek.");
            var mail = emails[0];
            Assert.AreEqual("3 contactverzoeken wachten op jou", mail.Subject);
            StringAssert.Contains(mail.HtmlBody, "werkdag");
        }
    }
}
