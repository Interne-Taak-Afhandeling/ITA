using InterneTaakAfhandeling.EndToEndTest.Infrastructure;
using Microsoft.Playwright;

namespace InterneTaakAfhandeling.EndToEndTest.Werklijst
{
    /// <summary>
    /// Feature #632: Werklijst filteren op medewerker, afdeling en status.
    /// </summary>
    [TestClass]
    [DoNotParallelize]
    public class WerklijstFilterScenarios : ITAPlaywrightTest
    {
        private const string AfdelingNaam = "Burgerzaken_ibz";
        private const int ListTimeout = 30000;

        [TestMethod("Alle contactverzoeken: filter op medewerker toont alleen contactverzoeken van die medewerker")]
        public async Task AlleContactverzoeken_FilterOpMedewerker()
        {
            var (metMedewerker, alleenAfdeling) = await CreateMedewerkerEnAfdelingContactverzoekenAsync("AlleCV_Mdw");

            await Step("Given de beheerder is op de pagina Alle contactverzoeken");
            await Page.GotoAsync("/alle-contactverzoeken");
            await Expect(Page.GetByText(alleenAfdeling)).ToBeVisibleAsync(new() { Timeout = ListTimeout });

            await Step("When de beheerder een medewerker kiest uit de lijst");
            await SelectMedewerkerAsync();

            await Step("Then toont de lijst het contactverzoek dat aan die medewerker is toegewezen");
            await Expect(Page.GetByText(metMedewerker)).ToBeVisibleAsync(new() { Timeout = ListTimeout });

            await Step("And niet het contactverzoek dat alleen aan een afdeling is toegewezen");
            await Expect(Page.GetByText(alleenAfdeling)).Not.ToBeVisibleAsync();
        }

        [TestMethod("Alle contactverzoeken: filters zijn te combineren en te wissen")]
        public async Task AlleContactverzoeken_GecombineerdeFilters_EnWissen()
        {
            var (metMedewerker, alleenAfdeling) = await CreateMedewerkerEnAfdelingContactverzoekenAsync("AlleCV_Comb");

            await Step("Given de beheerder is op de pagina Alle contactverzoeken");
            await Page.GotoAsync("/alle-contactverzoeken");
            await Expect(Page.GetByText(alleenAfdeling)).ToBeVisibleAsync(new() { Timeout = ListTimeout });

            await Step("When de beheerder filtert op afdeling en medewerker");
            await Page.GetByLabel("Afdeling of groep", new() { Exact = true }).SelectOptionAsync(new[] { AfdelingNaam });
            await SelectMedewerkerAsync();

            await Step("Then toont de lijst alleen het contactverzoek van die medewerker binnen die afdeling");
            await Expect(Page.GetByText(metMedewerker)).ToBeVisibleAsync(new() { Timeout = ListTimeout });
            await Expect(Page.GetByText(alleenAfdeling)).Not.ToBeVisibleAsync();

            await Step("When de beheerder daarnaast de status afgehandeld kiest");
            await Page.GetByLabel("Status", new() { Exact = true }).SelectOptionAsync(new[] { "Afgehandeld" });

            await Step("Then verdwijnt het open contactverzoek uit de lijst");
            await Expect(Page.GetByRole(AriaRole.Columnheader, new() { Name = "Afgehandeld op" })).ToBeVisibleAsync(new() { Timeout = ListTimeout });
            await Expect(Page.GetByText(metMedewerker)).Not.ToBeVisibleAsync();

            await Step("When de beheerder de filters wist");
            await Page.GetByLabel("Status", new() { Exact = true }).SelectOptionAsync(new[] { "Open" });
            await Page.GetByLabel("Afdeling of groep", new() { Exact = true }).SelectOptionAsync(new[] { "" });
            await Page.GetByRole(AriaRole.Combobox, new() { Name = "Medewerker", Exact = true }).FillAsync("");

            await Step("Then toont de lijst weer alle open contactverzoeken");
            await Expect(Page.GetByText(alleenAfdeling)).ToBeVisibleAsync(new() { Timeout = ListTimeout });
        }

        [TestMethod("Alle contactverzoeken: standaard open, filter op afgehandeld toont afgehandelde contactverzoeken")]
        public async Task AlleContactverzoeken_FilterOpStatus()
        {
            var onderwerp = $"Test_AlleCV_Status_{Guid.NewGuid().ToString()[..8]}";
            var (contactmomentUuid, _, _) = await TestDataHelper.CreateVerwerktContactverzoekAsync(onderwerp);
            RegisterCleanup(async () => await TestDataHelper.DeleteContactverzoekAsync(contactmomentUuid.ToString()));

            await Step("Given de beheerder is op de pagina Alle contactverzoeken");
            await Page.GotoAsync("/alle-contactverzoeken");

            await Step("Then staat de status standaard op open en is het afgehandelde contactverzoek niet zichtbaar");
            await Expect(Page.GetByLabel("Status", new() { Exact = true })).ToHaveValueAsync("open");
            await Expect(Page.GetByRole(AriaRole.Columnheader, new() { Name = "Urgentie" })).ToBeVisibleAsync(new() { Timeout = ListTimeout });
            await Expect(Page.GetByText(onderwerp)).Not.ToBeVisibleAsync();

            await Step("When de beheerder de status afgehandeld kiest");
            await Page.GetByLabel("Status", new() { Exact = true }).SelectOptionAsync(new[] { "Afgehandeld" });

            await Step("Then is het afgehandelde contactverzoek zichtbaar");
            await Expect(Page.GetByText(onderwerp)).ToBeVisibleAsync(new() { Timeout = ListTimeout });
        }

        [TestMethod("Afdelingswerkvoorraad: filter op medewerker toont alleen contactverzoeken van die medewerker")]
        public async Task Afdelingswerkvoorraad_FilterOpMedewerker()
        {
            var (metMedewerker, alleenAfdeling) = await CreateMedewerkerEnAfdelingContactverzoekenAsync("AfdWv_Mdw");

            await Step("Given de gebruiker bekijkt de afdelingswerkvoorraad van de afdeling");
            await Page.GotoAsync("/afdelings-contacten");
            var afdelingDropdown = Page.GetByLabel("Selecteer een afdeling of");
            await afdelingDropdown.WaitForAsync(new() { State = WaitForSelectorState.Visible });
            await afdelingDropdown.SelectOptionAsync(new[] { AfdelingNaam });
            await Expect(Page.GetByText(alleenAfdeling)).ToBeVisibleAsync(new() { Timeout = ListTimeout });

            await Step("When de gebruiker een medewerker kiest uit de lijst");
            await SelectMedewerkerAsync();

            await Step("Then toont de lijst alleen het contactverzoek dat aan die medewerker is toegewezen");
            await Expect(Page.GetByText(metMedewerker)).ToBeVisibleAsync(new() { Timeout = ListTimeout });
            await Expect(Page.GetByText(alleenAfdeling)).Not.ToBeVisibleAsync();
        }

        // NOT IMPLEMENTED — scenarios excluded from E2E coverage (Feature #632):
        //
        // "Geen contactverzoeken gevonden voor dit filter":
        //   the test environment is shared, so no filter combination is guaranteed to give an empty list.
        //   Covered by the emptyMessage prop of AllInterneTakenTable; check manually.
        //
        // "Medewerker die in OpenKlant onder meerdere identificaties bekend is":
        //   requires creating Entra-ID/handmatig e-mail actors for the test user next to the objectregister actor.
        //   The merge is done server-side in InterneTakenOverzichtService; check manually.
        //
        // "Alle contactverzoeken is alleen voor de functioneel beheerder":
        //   unchanged by this feature and requires a non-admin test user (see AlleContactverzoekenAccessScenarios).

        /// <summary>
        /// Creates two open contactverzoeken for the test afdeling: one also assigned to the current user, one assigned only to the afdeling.
        /// </summary>
        private async Task<(string MetMedewerker, string AlleenAfdeling)> CreateMedewerkerEnAfdelingContactverzoekenAsync(string prefix)
        {
            await Step("Given een contactverzoek toegewezen aan de afdeling en een medewerker, en een contactverzoek alleen aan de afdeling");
            var suffix = Guid.NewGuid().ToString()[..8];
            var metMedewerker = $"Test_{prefix}_Medewerker_{suffix}";
            var alleenAfdeling = $"Test_{prefix}_Afdeling_{suffix}";

            var (metMedewerkerUuid, _) = await TestDataHelper.CreateContactverzoekWithCurrentUserAssignedViaObjectRegisterId(metMedewerker);
            RegisterCleanup(async () => await TestDataHelper.DeleteContactverzoekAsync(metMedewerkerUuid.ToString()));

            var (alleenAfdelingUuid, _) = await TestDataHelper.CreateContactverzoekWithTeamAssignmentOnly(alleenAfdeling);
            RegisterCleanup(async () => await TestDataHelper.DeleteContactverzoekAsync(alleenAfdelingUuid.ToString()));

            return (metMedewerker, alleenAfdeling);
        }

        private async Task SelectMedewerkerAsync()
        {
            var naam = await TestDataHelper.GetCurrentUserMedewerkerNaamAsync();
            var combobox = Page.GetByRole(AriaRole.Combobox, new() { Name = "Medewerker", Exact = true });
            await combobox.FillAsync(naam);
            var option = Page.GetByRole(AriaRole.Option, new() { Name = naam, Exact = true });
            await option.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = ListTimeout });
            await option.ClickAsync();
            await Expect(combobox).ToHaveValueAsync(naam);
        }
    }
}
