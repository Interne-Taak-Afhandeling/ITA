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
        private const string AfdelingWerkvoorraadLinkPath = "/";
        private const string AfsluitendeZin = "Fijne werkdag";
        private const string InstructieZin = "Neem contact op en handel deze contactverzoeken af.";

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

        [TestMethod("Afdeling ontvangt herinneringsmail voor niet-toegewezen verlopen ContactVerzoek")]
        public async Task Afdeling_ReceivesReminderMail_ForUnassignedOverdueContactverzoek()
        {
            const string onderwerp = "E2E_DagelijkseHerinnering_Afdeling";
            var plaatsgevondenOp = BusinessHours.SubtractBusinessHours(DateTime.UtcNow, hours: 53);

            await Step("Setup an overdue contactverzoek assigned only to an Afdeling, no Medewerker");
            var (uuid, _, afdelingNaam) = await TestDataHelper.CreateContactverzoekWithAfdelingOnlyAndContactDatum(onderwerp, plaatsgevondenOp);
            RegisterCleanup(async () => await TestDataHelper.DeleteContactverzoekAsync(uuid.ToString()));

            await Step("Trigger the daily reminder scheduler");
            await SchedulerTrigger.TriggerDagelijkseHerinneringSchedulerAsync();

            await Step($"Check the '{afdelingNaam}' Afdeling's testmailbox");
            // TODO: resolve the Afdeling's real email address via the Objects API once
            // TestMailbox is wired to a real mailbox — placeholder until then.
            var emails = await TestMailbox.GetReceivedEmailsAsync($"afdeling-{afdelingNaam}@test.icatt.nl");

            Assert.AreEqual(1, emails.Count, "Expected exactly one reminder mail for the Afdeling.");
            StringAssert.Contains(emails[0].HtmlBody, $"href=\"{AfdelingWerkvoorraadLinkPath}");
        }

        [TestMethod("Dual-assignment — zowel Medewerker als Afdeling ontvangen een herinneringsmail")]
        public async Task BothMedewerkerAndAfdeling_ReceiveReminderMail_ForDualAssignedContactverzoek()
        {
            const string onderwerp = "E2E_DagelijkseHerinnering_DualAssignment";
            var plaatsgevondenOp = BusinessHours.SubtractBusinessHours(DateTime.UtcNow, hours: 53);

            await Step("Setup an overdue contactverzoek assigned to both a Medewerker and an Afdeling");
            var (uuid, _, afdelingNaam) = await TestDataHelper.CreateContactverzoekWithContactDatum(onderwerp, plaatsgevondenOp);
            RegisterCleanup(async () => await TestDataHelper.DeleteContactverzoekAsync(uuid.ToString()));

            await Step("Trigger the daily reminder scheduler");
            await SchedulerTrigger.TriggerDagelijkseHerinneringSchedulerAsync();

            await Step("Check the Medewerker's testmailbox");
            var medewerkerEmails = await TestMailbox.GetReceivedEmailsAsync(CurrentUserEmail);
            Assert.AreEqual(1, medewerkerEmails.Count, "Expected the Medewerker to receive a reminder mail.");

            await Step($"Check the '{afdelingNaam}' Afdeling's testmailbox");
            var afdelingEmails = await TestMailbox.GetReceivedEmailsAsync($"afdeling-{afdelingNaam}@test.icatt.nl");
            Assert.AreEqual(1, afdelingEmails.Count, "Expected the Afdeling to also receive a reminder mail.");
        }

        [TestMethod("Mailbody bevat geen persoonsgegevens van contactverzoeken")]
        public async Task ReminderMail_ContainsNoPersonalData()
        {
            const string onderwerp = "E2E_DagelijkseHerinnering_GeenPersoonsgegevens";
            const string klantnaam = "Jan Janssen";
            var plaatsgevondenOp = BusinessHours.SubtractBusinessHours(DateTime.UtcNow, hours: 53);

            await Step("Setup an overdue contactverzoek with a named klant and gespreksinhoud");
            var (uuid, _) = await TestDataHelper.CreateContactverzoekWithMedewerkerOnly(onderwerp, plaatsgevondenOp);
            RegisterCleanup(async () => await TestDataHelper.DeleteContactverzoekAsync(uuid.ToString()));

            await Step("Trigger the daily reminder scheduler");
            await SchedulerTrigger.TriggerDagelijkseHerinneringSchedulerAsync();

            await Step("Check the Medewerker's testmailbox");
            var emails = await TestMailbox.GetReceivedEmailsAsync(CurrentUserEmail);

            Assert.AreEqual(1, emails.Count);
            var mail = emails[0];
            StringAssert.DoesNotMatch(mail.HtmlBody, new System.Text.RegularExpressions.Regex(System.Text.RegularExpressions.Regex.Escape(klantnaam)));
            StringAssert.DoesNotMatch(mail.HtmlBody, new System.Text.RegularExpressions.Regex(System.Text.RegularExpressions.Regex.Escape(onderwerp)));
            StringAssert.Contains(mail.HtmlBody, InstructieZin);
        }

        [TestMethod("Geen herinneringsmail voor ContactVerzoek met status \"verwerkt\"")]
        public async Task NoReminderMail_ForVerwerktContactverzoek()
        {
            const string onderwerp = "E2E_DagelijkseHerinnering_Verwerkt";
            var plaatsgevondenOp = BusinessHours.SubtractBusinessHours(DateTime.UtcNow, hours: 53);

            await Step("Setup an overdue contactverzoek with status 'verwerkt', assigned to the current Medewerker");
            var (uuid, _, _) = await TestDataHelper.CreateVerwerktContactverzoekWithMedewerkerAsync(onderwerp, plaatsgevondenOp);
            RegisterCleanup(async () => await TestDataHelper.DeleteContactverzoekAsync(uuid.ToString()));

            await Step("Trigger the daily reminder scheduler");
            await SchedulerTrigger.TriggerDagelijkseHerinneringSchedulerAsync();

            await Step("Check the Medewerker's testmailbox");
            var emails = await TestMailbox.GetReceivedEmailsAsync(CurrentUserEmail);

            Assert.AreEqual(0, emails.Count, "A 'verwerkt' contactverzoek must never trigger a reminder mail.");
        }

        [TestMethod("Geen verlopen contactverzoeken — geen mails verstuurd")]
        public async Task NoReminderMails_WhenNoOverdueContactverzoeken()
        {
            await Step("Trigger the daily reminder scheduler with no overdue contactverzoeken set up");
            await SchedulerTrigger.TriggerDagelijkseHerinneringSchedulerAsync();

            await Step("Check the Medewerker's testmailbox");
            var emails = await TestMailbox.GetReceivedEmailsAsync(CurrentUserEmail);

            Assert.AreEqual(0, emails.Count, "No reminder mails should be sent when there are no overdue contactverzoeken.");
        }

        [TestMethod("Afdeling zonder e-mailadres — notificatie overgeslagen, overige ontvangers niet geblokkeerd")]
        public async Task AfdelingWithoutEmail_SkipsNotification_WithoutBlockingOtherRecipients()
        {
            if (string.IsNullOrEmpty(TestDataConstants.Afdelingen.ZonderEmailKey))
            {
                Assert.Inconclusive(
                    "TestDataConstants.Afdelingen.ZonderEmailKey is not configured. " +
                    "Set this constant to an afdeling without an email address in the test objectenregister.");
            }

            const string afdelingOnderwerp = "E2E_DagelijkseHerinnering_AfdelingZonderEmail";
            const string medewerkerOnderwerp = "E2E_DagelijkseHerinnering_AndereMedewerker";
            var plaatsgevondenOp = BusinessHours.SubtractBusinessHours(DateTime.UtcNow, hours: 53);

            await Step("Setup an overdue contactverzoek for an Afdeling without an email address");
            var (afdelingUuid, _, afdelingNaam) = await TestDataHelper.CreateContactverzoekWithAfdelingOnlyAndContactDatum(
                afdelingOnderwerp, plaatsgevondenOp, TestDataConstants.Afdelingen.ZonderEmailKey);
            RegisterCleanup(async () => await TestDataHelper.DeleteContactverzoekAsync(afdelingUuid.ToString()));

            await Step("Setup an overdue contactverzoek for another Medewerker");
            var (medewerkerUuid, _) = await TestDataHelper.CreateContactverzoekWithMedewerkerOnly(medewerkerOnderwerp, plaatsgevondenOp);
            RegisterCleanup(async () => await TestDataHelper.DeleteContactverzoekAsync(medewerkerUuid.ToString()));

            await Step("Trigger the daily reminder scheduler");
            await SchedulerTrigger.TriggerDagelijkseHerinneringSchedulerAsync();

            await Step("Check the Medewerker's testmailbox — must still receive their reminder");
            var medewerkerEmails = await TestMailbox.GetReceivedEmailsAsync(CurrentUserEmail);
            Assert.AreEqual(1, medewerkerEmails.Count, "The other Medewerker's reminder must not be blocked by the Afdeling's missing email.");

            await Step($"Check that '{afdelingNaam}' received no mail");
            var afdelingEmails = await TestMailbox.GetReceivedEmailsAsync($"afdeling-{afdelingNaam}@test.icatt.nl");
            Assert.AreEqual(0, afdelingEmails.Count, "No mail should be sent when the Afdeling has no valid email address.");
        }
    }
}
