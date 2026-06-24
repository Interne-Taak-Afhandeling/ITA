using InterneTaakAfhandeling.EndToEndTest.Infrastructure;
using Microsoft.Playwright;

namespace InterneTaakAfhandeling.EndToEndTest.Dashboard
{
    public partial class ContactverzoekScenarios
    {
        [TestMethod("Actorweergave toont medewerker en afdeling op twee regels")]
        public async Task User_CanSeeActorMedewerkerAndAfdeling_OnContactverzoekDetail()
        {
            var onderwerp = "Test_ActorDisplay_MedewerkerAfdeling";
            await Step("Setup contactverzoek met medewerker- en afdeling-actor via API");
            var (uuid, internetaakNummer) = await TestDataHelper.CreateContactverzoekWithAfdelingAndMedewerker(onderwerp);
            RegisterCleanup(async () => await TestDataHelper.DeleteContactverzoekAsync(uuid.ToString()));

            await NavigateToContactverzoekByNummer(internetaakNummer);

            await Step("Verify medewerker staat op de eerste regel van de actorweergave");
            await Expect(Page.GetBehandelaarActorLine("ICATT Integratietest")).ToBeVisibleAsync();

            await Step("Verify 'Afdeling: [naam]' staat op de tweede regel");
            await Expect(Page.GetBehandelaarActorLine("Afdeling: e2e afdeling")).ToBeVisibleAsync();
        }

        [TestMethod("Actorweergave toont medewerker en groep op twee regels")]
        public async Task User_CanSeeActorMedewerkerAndGroep_OnContactverzoekDetail()
        {
            var onderwerp = "Test_ActorDisplay_MedewerkerGroep";
            await Step("Setup contactverzoek met medewerker- en groep-actor via API");
            var (uuid, internetaakNummer) = await TestDataHelper.CreateContactverzoekWithGroepAndMedewerker(onderwerp);
            RegisterCleanup(async () => await TestDataHelper.DeleteContactverzoekAsync(uuid.ToString()));

            await NavigateToContactverzoekByNummer(internetaakNummer);

            await Step("Verify medewerker staat op de eerste regel van de actorweergave");
            await Expect(Page.GetBehandelaarActorLine("ICATT Integratietest")).ToBeVisibleAsync();

            await Step("Verify 'Groep: [naam]' staat op de tweede regel");
            await Expect(Page.GetBehandelaarActorLine("Groep: e2e groep")).ToBeVisibleAsync();
        }

        [TestMethod("Actorweergave toont alleen afdeling wanneer er geen medewerker is")]
        public async Task User_CanSeeAfdelingOnly_OnContactverzoekDetailWithoutMedewerker()
        {
            var onderwerp = "Test_ActorDisplay_AfdelingOnly";
            await Step("Setup contactverzoek met alleen afdeling-actor via API");
            var (uuid, internetaakNummer) = await TestDataHelper.CreateContactverzoekWithAfdelingOnly(onderwerp);
            RegisterCleanup(async () => await TestDataHelper.DeleteContactverzoekAsync(uuid.ToString()));

            await NavigateToContactverzoekByNummer(internetaakNummer);

            await Step("Verify 'Afdeling: [naam]' is zichtbaar");
            await Expect(Page.GetBehandelaarActorLine("Afdeling: e2e afdeling")).ToBeVisibleAsync();

            await Step("Verify geen medewerkersnaamregel zichtbaar");
            await Expect(Page.GetBehandelaarActorLine("ICATT Integratietest")).Not.ToBeVisibleAsync();
        }

        [TestMethod("Actorweergave toont alleen groep wanneer er geen medewerker is")]
        public async Task User_CanSeeGroepOnly_OnContactverzoekDetailWithoutMedewerker()
        {
            var onderwerp = "Test_ActorDisplay_GroepOnly";
            await Step("Setup contactverzoek met alleen groep-actor via API");
            var (uuid, internetaakNummer) = await TestDataHelper.CreateContactverzoekWithGroepOnly(onderwerp);
            RegisterCleanup(async () => await TestDataHelper.DeleteContactverzoekAsync(uuid.ToString()));

            await NavigateToContactverzoekByNummer(internetaakNummer);

            await Step("Verify 'Groep: [naam]' is zichtbaar");
            await Expect(Page.GetBehandelaarActorLine("Groep: e2e groep")).ToBeVisibleAsync();

            await Step("Verify geen medewerkersnaamregel zichtbaar");
            await Expect(Page.GetBehandelaarActorLine("ICATT Integratietest")).Not.ToBeVisibleAsync();
        }

        [TestMethod("Actorweergave toont geen typespecifiek label bij onbekend OE-type")]
        public async Task User_CannotSeeWrongLabel_OnContactverzoekDetailWithUnknownOeType()
        {
            var onderwerp = "Test_ActorDisplay_UnknownOeType";
            await Step("Setup contactverzoek met onbekend OE-type actor via API");
            var (uuid, internetaakNummer) = await TestDataHelper.CreateContactverzoekWithUnknownOeType(onderwerp);
            RegisterCleanup(async () => await TestDataHelper.DeleteContactverzoekAsync(uuid.ToString()));

            await NavigateToContactverzoekByNummer(internetaakNummer);

            await Step("Verify geen typespecifiek label 'Afdeling' of 'Groep' zichtbaar voor onbekend type");
            await Expect(Page.GetBehandelaarActorLine("Afdeling")).Not.ToBeVisibleAsync();
            await Expect(Page.GetBehandelaarActorLine("Groep")).Not.ToBeVisibleAsync();
        }

        [TestMethod("Actorweergave toont alleen de eerste organisatorische eenheid bij meerdere gekoppelde actoren")]
        public async Task User_CanSeeFirstOeOnly_OnContactverzoekDetailWithMultipleOeActors()
        {
            var onderwerp = "Test_ActorDisplay_MultipleOeActors";
            await Step("Setup contactverzoek met meerdere OE-actoren (afdeling + groep) via API");
            var (uuid, internetaakNummer) = await TestDataHelper.CreateContactverzoekWithMultipleOeActors(onderwerp);
            RegisterCleanup(async () => await TestDataHelper.DeleteContactverzoekAsync(uuid.ToString()));

            await NavigateToContactverzoekByNummer(internetaakNummer);

            await Step("Verify eerste OE (afdeling) is zichtbaar");
            await Expect(Page.GetBehandelaarActorLine("Afdeling: e2e afdeling")).ToBeVisibleAsync();

            await Step("Verify tweede OE (groep) is niet zichtbaar");
            await Expect(Page.GetBehandelaarActorLine("Groep")).Not.ToBeVisibleAsync();
        }
    }
}
