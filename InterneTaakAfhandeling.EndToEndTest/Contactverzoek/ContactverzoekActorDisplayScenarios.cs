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

            await Step("Verify medewerker staat in de 'Behandelaar'-rij");
            await Expect(Page.GetBehandelaarValue()).ToBeVisibleAsync();
            await Expect(Page.GetBehandelaarValue()).ToHaveTextAsync("ICATT Integratietest");

            await Step("Verify 'Afdeling'-rij toont de OE-naam");
            await Expect(Page.GetOrganisatorischeEenheidKey("Afdeling")).ToBeVisibleAsync();
            await Expect(Page.GetOrganisatorischeEenheidValue("Afdeling")).ToBeVisibleAsync();
        }

        [TestMethod("Actorweergave toont medewerker en groep op twee regels")]
        public async Task User_CanSeeActorMedewerkerAndGroep_OnContactverzoekDetail()
        {
            var onderwerp = "Test_ActorDisplay_MedewerkerGroep";
            await Step("Setup contactverzoek met medewerker- en groep-actor via API");
            var (uuid, internetaakNummer) = await TestDataHelper.CreateContactverzoekWithGroepAndMedewerker(onderwerp);
            RegisterCleanup(async () => await TestDataHelper.DeleteContactverzoekAsync(uuid.ToString()));

            await NavigateToContactverzoekByNummer(internetaakNummer);

            await Step("Verify medewerker staat in de 'Behandelaar'-rij");
            await Expect(Page.GetBehandelaarValue()).ToBeVisibleAsync();
            await Expect(Page.GetBehandelaarValue()).ToHaveTextAsync("ICATT Integratietest");

            await Step("Verify 'Groep'-rij toont de OE-naam");
            await Expect(Page.GetOrganisatorischeEenheidKey("Groep")).ToBeVisibleAsync();
            await Expect(Page.GetOrganisatorischeEenheidValue("Groep")).ToBeVisibleAsync();
        }

        [TestMethod("Actorweergave toont alleen afdeling wanneer er geen medewerker is")]
        public async Task User_CanSeeAfdelingOnly_OnContactverzoekDetailWithoutMedewerker()
        {
            var onderwerp = "Test_ActorDisplay_AfdelingOnly";
            await Step("Setup contactverzoek met alleen afdeling-actor via API");
            var (uuid, internetaakNummer) = await TestDataHelper.CreateContactverzoekWithAfdelingOnly(onderwerp);
            RegisterCleanup(async () => await TestDataHelper.DeleteContactverzoekAsync(uuid.ToString()));

            await NavigateToContactverzoekByNummer(internetaakNummer);

            await Step("Verify 'Afdeling'-rij toont de OE-naam");
            await Expect(Page.GetOrganisatorischeEenheidKey("Afdeling")).ToBeVisibleAsync();
            await Expect(Page.GetOrganisatorischeEenheidValue("Afdeling")).ToBeVisibleAsync();

            await Step("Verify geen 'Behandelaar'-rij zichtbaar");
            await Expect(Page.GetBehandelaarValue()).Not.ToBeVisibleAsync();
        }

        [TestMethod("Actorweergave toont alleen groep wanneer er geen medewerker is")]
        public async Task User_CanSeeGroepOnly_OnContactverzoekDetailWithoutMedewerker()
        {
            var onderwerp = "Test_ActorDisplay_GroepOnly";
            await Step("Setup contactverzoek met alleen groep-actor via API");
            var (uuid, internetaakNummer) = await TestDataHelper.CreateContactverzoekWithGroepOnly(onderwerp);
            RegisterCleanup(async () => await TestDataHelper.DeleteContactverzoekAsync(uuid.ToString()));

            await NavigateToContactverzoekByNummer(internetaakNummer);

            await Step("Verify 'Groep'-rij toont de OE-naam");
            await Expect(Page.GetOrganisatorischeEenheidKey("Groep")).ToBeVisibleAsync();
            await Expect(Page.GetOrganisatorischeEenheidValue("Groep")).ToBeVisibleAsync();

            await Step("Verify geen 'Behandelaar'-rij zichtbaar");
            await Expect(Page.GetBehandelaarValue()).Not.ToBeVisibleAsync();
        }

        [TestMethod("Actorweergave toont fallbackweergave bij onbekend type organisatorische eenheid")]
        public async Task User_CanSeeFallbackActorDisplay_OnContactverzoekDetailWithUnknownOeType()
        {
            var onderwerp = "Test_ActorDisplay_UnknownOeType";
            await Step("Setup contactverzoek met onbekend OE-type actor via API");
            var (uuid, internetaakNummer) = await TestDataHelper.CreateContactverzoekWithUnknownOeType(onderwerp);
            RegisterCleanup(async () => await TestDataHelper.DeleteContactverzoekAsync(uuid.ToString()));

            await NavigateToContactverzoekByNummer(internetaakNummer);

            await Step("Verify OE-naam is zichtbaar met generiek label 'Organisatorische eenheid'");
            await Expect(Page.GetOrganisatorischeEenheidKey("Organisatorische eenheid")).ToBeVisibleAsync();
            await Expect(Page.GetOrganisatorischeEenheidValue("Organisatorische eenheid")).ToHaveTextAsync("e2e onbekende eenheid");

            await Step("Verify geen typespecifiek label 'Afdeling' of 'Groep' zichtbaar");
            await Expect(Page.GetOrganisatorischeEenheidKey("Afdeling")).Not.ToBeVisibleAsync();
            await Expect(Page.GetOrganisatorischeEenheidKey("Groep")).Not.ToBeVisibleAsync();
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
            await Expect(Page.GetOrganisatorischeEenheidKey("Afdeling")).ToBeVisibleAsync();
            await Expect(Page.GetOrganisatorischeEenheidValue("Afdeling")).ToBeVisibleAsync();

            await Step("Verify tweede OE (groep) is niet zichtbaar");
            await Expect(Page.GetOrganisatorischeEenheidKey("Groep")).Not.ToBeVisibleAsync();
        }
    }
}
