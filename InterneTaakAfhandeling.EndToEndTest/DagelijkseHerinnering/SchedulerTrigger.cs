using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InterneTaakAfhandeling.EndToEndTest.DagelijkseHerinnering
{
    /// <summary>
    /// Triggers the daily-reminder Poller job (VerlopenInternetakenProcessor) from a Playwright
    /// E2E test, without waiting for its 07:00 weekday CronJob schedule.
    ///
    /// No such trigger exists yet: the Poller is a separate Kubernetes CronJob console app with
    /// no HTTP surface reachable from the Web.Server/Playwright, so there is nothing to call here
    /// today. Every scenario in DagelijkseHerinneringScenarios.cs calls this first and will report
    /// Inconclusive until a real trigger (e.g. a debug endpoint or a scripted CronJob invocation)
    /// is built as follow-up infra work (Task #492 Edge Cases & Risks: "Scheduler-timing").
    /// </summary>
    internal static class SchedulerTrigger
    {
        internal static Task TriggerDagelijkseHerinneringSchedulerAsync()
        {
            Assert.Inconclusive(
                "No scheduler test-trigger exists yet for VerlopenInternetakenProcessor. " +
                "The Poller runs as a separate Kubernetes CronJob console app with no HTTP endpoint " +
                "reachable from this test suite. Wire this method to a real trigger mechanism once " +
                "one is available (see Task #492 Technical Approach).");
            return Task.CompletedTask;
        }
    }
}
