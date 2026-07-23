using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InterneTaakAfhandeling.EndToEndTest.DagelijkseHerinnering
{
    internal sealed record ReceivedEmail(string Subject, string HtmlBody, string PlainTextBody);

    /// <summary>
    /// Reads emails delivered to a test mailbox in the deployed test environment, so a Playwright
    /// E2E test can assert on daily-reminder mail subject/body/links.
    ///
    /// No test mailbox reader exists yet: there is no Mailhog/Mailpit/SMTP-test container or
    /// mailbox API anywhere in this repo, and no SMTP host is configured for local/CI environments.
    /// Every scenario in DagelijkseHerinneringScenarios.cs calls SchedulerTrigger first, which
    /// already reports Inconclusive, so this stub's body is never reached today - it exists so the
    /// test structure is ready once a real test mailbox (e.g. Mailpit + HTTP API client) is added
    /// as follow-up infra work (Task #492 Edge Cases & Risks: "Testmailbox-toegang").
    /// </summary>
    internal static class TestMailbox
    {
        internal static Task<List<ReceivedEmail>> GetReceivedEmailsAsync(string recipientEmail, TimeSpan? timeout = null)
        {
            Assert.Inconclusive(
                "No test mailbox reader exists yet. Add a Mailpit/Mailhog-style container plus an " +
                "HTTP client here once test-mailbox infra is available (see Task #492 Technical Approach).");
            return Task.FromResult(new List<ReceivedEmail>());
        }
    }
}
