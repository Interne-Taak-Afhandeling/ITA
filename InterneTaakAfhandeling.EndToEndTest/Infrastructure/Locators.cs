using Microsoft.Playwright;

namespace InterneTaakAfhandeling.EndToEndTest.Infrastructure
{ 
    public static class Locators
    {
        // Dashboard locators
        public static ILocator GetDetailsLink(this IPage page, string onderwerp)
        {
            var testRow = page.GetByRole(AriaRole.Row).Filter(new() { HasText = onderwerp }).First;
            return testRow.GetByRole(AriaRole.Link).Filter(new() { HasText = "→" });
        }

        // Werklijst locators (urgentie badge, Afdeling/Klantcontactnummer columns, row click)
        public static ILocator GetWerklijstRow(this IPage page, string onderwerp) =>
            page.GetByRole(AriaRole.Row).Filter(new() { HasText = onderwerp }).First;

        public static ILocator GetColumnHeader(this IPage page, string name) =>
            page.GetByRole(AriaRole.Columnheader, new() { Name = name, Exact = true });

        public static ILocator GetUrgentieBadge(this IPage page, string onderwerp) =>
            page.GetWerklijstRow(onderwerp).Locator(".utrecht-badge-status");

        // Locates a row's cell by matching column header text at runtime, since Afdeling and
        // Klantcontactnummer sit at different column indices on the two worklist tables.
        public static async Task<ILocator> GetColumnCellAsync(this IPage page, string onderwerp, string columnName)
        {
            // The table renders asynchronously after navigation; wait for the target row before
            // scanning headers, since CountAsync/InnerTextAsync below don't auto-retry like
            // Playwright's Expect assertions do.
            await page.GetWerklijstRow(onderwerp).WaitForAsync(new() { State = WaitForSelectorState.Visible });

            var headers = page.GetByRole(AriaRole.Columnheader);
            var count = await headers.CountAsync();
            for (var i = 0; i < count; i++)
            {
                var text = (await headers.Nth(i).InnerTextAsync()).Trim();
                if (text == columnName)
                {
                    return page.GetWerklijstRow(onderwerp).GetByRole(AriaRole.Cell).Nth(i);
                }
            }
            throw new InvalidOperationException($"Column header '{columnName}' not found in the worklist table.");
        }

        // Contactverzoek Details locators
        public static ILocator GetKlantnaamLabel(this IPage page) => page.GetByText("Klantnaam");

        public static ILocator GetKlantnaamValue(this IPage page)
        {
            var parentDiv = page.Locator("div.utrecht-data-list__item").Filter(new() { HasText = "Klantnaam" });
            return parentDiv.Locator("dd.utrecht-data-list__item-value");
        }

        public static ILocator GetTelefoonnummerLabel(this IPage page) => page.GetByText("Telefoonnummer");

        public static ILocator GetTelefoonnummerValue(this IPage page)
        {
            var parentDiv = page.Locator("div.utrecht-data-list__item").Filter(new() { HasText = "Telefoonnummer" });
            return parentDiv.Locator("dd.utrecht-data-list__item-value");
        }

        public static ILocator GetEmailLabel(this IPage page) => page.GetByText("E-mailadres");

        public static ILocator GetEmailValue(this IPage page)
        {
            var parentDiv = page.Locator("div.utrecht-data-list__item").Filter(new() { HasText = "E-mailadres" });
            return parentDiv.Locator("dd.utrecht-data-list__item-value");
        }

        public static ILocator GetContactmomentRegistrerenTab(this IPage page) =>
            page.GetByText("Contact registreren");

        public static ILocator GetDoorsturenTab(this IPage page) => page.GetByText("Doorsturen");

        public static ILocator GetAlleenToelichtingTab(this IPage page) => page.GetByText("Alleen toelichting");

        public static ILocator GetVraagLabel(this IPage page) => page.GetByText("Vraag", new() { Exact = true });

        public static ILocator GetVraagValue(this IPage page, string onderwerp)
        {
            var parentDiv = page.Locator("div.utrecht-data-list__item").Filter(new() { HasText = "Vraag" });
            return parentDiv.Locator("dd.utrecht-data-list__item-value");
        }

        public static ILocator GetInformatieVoorBurgerLabel(this IPage page) =>
            page.GetByRole(AriaRole.Term).Filter(new() { HasText = "Informatie voor burger /" });

        public static ILocator GetInformatieVoorBurgerValue(this IPage page) =>
            page.GetByText("This is a test contact request created during an end-to-end test run.");

        public static ILocator GetAangemaaktDoorLabel(this IPage page) => page.GetByText("Aangemaakt door");

        public static ILocator GetAangemaaktDoorValue(this IPage page)
        {
            var parentDiv = page.Locator("div.utrecht-data-list__item").Filter(new() { HasText = "Aangemaakt door" });
            return parentDiv.Locator("dd.utrecht-data-list__item-value");
        }

        public static ILocator GetBehandelaarLabel(this IPage page) => page.GetByText("Behandelaar");

        public static ILocator GetBehandelaarValue(this IPage page)
        {
            var parentDiv = page.Locator("div.utrecht-data-list__item").Filter(new() { HasText = "Behandelaar" });
            return parentDiv.Locator("dd.utrecht-data-list__item-value");
        }

        public static ILocator GetOrganisatorischeEenheidValue(this IPage page, string typeLabel)
        {
            // Scope by the dt key to avoid substring matches on other rows (e.g. onderwerp containing the label)
            var parentDiv = page.Locator("div.utrecht-data-list__item")
                .Filter(new() { Has = page.Locator("dt.utrecht-data-list__item-key").Filter(new() { HasText = typeLabel }) });
            return parentDiv.Locator("dd.utrecht-data-list__item-value");
        }

        public static ILocator GetOrganisatorischeEenheidKey(this IPage page, string typeLabel) =>
            page.Locator("dt.utrecht-data-list__item-key").Filter(new() { HasText = typeLabel });

        public static ILocator GetStatusLabel(this IPage page) => page.GetByText("Status");

        public static ILocator GetStatusValue(this IPage page)
        {
            var parentDiv = page.Locator("div.utrecht-data-list__item").Filter(new() { HasText = "Status" });
            return parentDiv.Locator("dd.utrecht-data-list__item-value");
        }

        public static ILocator GetKanaalLabel(this IPage page) =>
            page.Locator("dt.utrecht-data-list__item-key").Filter(new() { HasText = "Kanaal" });

        public static ILocator GetKanaalValue(this IPage page)
        {
            var parentDiv = page.Locator("div.utrecht-data-list__item").Filter(new() { HasText = "Kanaal" });
            return parentDiv.Locator("dd.utrecht-data-list__item-value");
        }

        public static ILocator GetInterneToelichtingLabel(this IPage page) =>
            page.GetByText("Interne toelichting KCC");

        public static ILocator GetInterneToelichtingValue(this IPage page) =>
            page.GetByText("Test contactverzoek from ITA E2E test");

        public static ILocator GetContactmomentOpslaanButton(this IPage page) =>
            page.GetByRole(AriaRole.Button, new() { Name = "Opslaan" });

        // Zaak connection locators
        public static ILocator GetGekoppeldeZaakKoppelenButton(this IPage page) =>
            page.GetByRole(AriaRole.Button, new() { Name = "Gekoppelde zaak Koppelen" });

        public static ILocator GetGekoppeldeZaakWijzigenButton(this IPage page) =>
            page.GetByRole(AriaRole.Button, new() { Name = "Gekoppelde zaak Wijzigen" });

        public static ILocator GetKoppelAanZaakHeading(this IPage page) =>
            page.GetByRole(AriaRole.Heading, new() { Name = "Koppel aan zaak" });

        public static ILocator GetKoppelAanZaakDescriptionText(this IPage page) =>
            page.GetByText("Aan welke zaak wil je dit");

        public static ILocator GetZaaknummerTextbox(this IPage page) =>
            page.GetByRole(AriaRole.Textbox, new() { Name = "Zaaknummer" });

        public static ILocator GetKoppelenButton(this IPage page) =>
            page.GetByRole(AriaRole.Button, new() { Name = "Koppelen", Exact = true });

        public static ILocator GetZaakSuccesvolGekoppeldMessage(this IPage page) =>
            page.GetByText("Zaak succesvol gekoppeld");

        public static ILocator GetGeenZaakGevondenMessage(this IPage page, string zaakIdentificatie) =>
            page.GetByText($"Geen zaak gevonden met identificatie: {zaakIdentificatie}");

        // Contactmoment locators
        public static ILocator GetContactOpnemenGeluktRadio(this IPage page) =>
            page.GetByRole(AriaRole.Radio, new() { Name = "Contact opnemen gelukt" });

        public static ILocator GetContactOpnemenNietGeluktRadio(this IPage page) =>
            page.GetByRole(AriaRole.Radio, new() { Name = "Contact opnemen niet gelukt" });

        public static ILocator GetOpslaanEnAfrondenButton(this IPage page) =>
            page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Button, new() { Name = "Opslaan & afronden" });

        // Assignment button locators
        public static ILocator GetToewijzenAanMezelfButton(this IPage page) =>
            page.GetByRole(AriaRole.Button, new() { Name = "Toewijzen aan mezelf" });

        public static ILocator GetToewijzenAanMezelfDialogButton(this IPage page) =>
            page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Button, new() { Name = "Toewijzen aan mezelf" });

        public static ILocator GetAnnulerenDialogButton(this IPage page) =>
            page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Button, new() { Name = "Annuleren" });

        // Form field locators
        public static ILocator GetKanalenSelect(this IPage page) => page.Locator("#kanalen");
        
        public static ILocator GetInformatieBurgerTextbox(this IPage page) => page.Locator("#informatie-burger");
        
        public static ILocator GetJaLabel(this IPage page) => page.GetByLabel("Ja");
        
        public static ILocator GetNeeLabel(this IPage page) => page.GetByLabel("Nee");
        
        // Navigation link locators
        public static ILocator GetMijnHistorieLink(this IPage page) =>
            page.GetByRole(AriaRole.Link, new() { Name = "Mijn historie", Exact = true });
        
        // Success/Error message locators
        public static ILocator GetSuccessToast(this IPage page, string? text = null) =>
            text == null 
                ? page.Locator("output[role='status']")
                : page.GetByRole(AriaRole.Status).Filter(new() { HasText = text });

        public static ILocator GetContactmomentSuccesvolOpgeslagenEnAfgerondMessage(this IPage page) =>
            page.GetByText("Contact vastgelegd en contactverzoek afgerond");

        public static ILocator GetContactmomentSuccesvolBijgewerktMessage(this IPage page) =>
            page.GetByText("Contact succesvol vastgelegd");

        // Message locators
        public static ILocator GetContactverzoekToegewezenMessage(this IPage page) =>
            page.GetByText("Contactverzoek toegewezen");

        public static ILocator GetAfgehandeldMessage(this IPage page) =>
            page.GetByText("Dit contactverzoek is afgehandeld en kan niet meer worden gewijzigd.");

        // Heropenen locators
        public static ILocator GetHeropenButton(this IPage page) =>
            page.GetByRole(AriaRole.Button, new() { Name = "Heropenen" });

        public static ILocator GetHeropenDialogBevestigenButton(this IPage page) =>
            page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Button, new() { Name = "Heropenen" });

        public static ILocator GetHeropenRedenTextbox(this IPage page) =>
            page.Locator("#reopen-reden");

        public static ILocator GetContactverzoekHeropendMessage(this IPage page) =>
            page.GetByRole(AriaRole.Status).Filter(new() { HasText = "Contactverzoek heropend" });

        // Doorsturen form locators
        public static ILocator GetDoorsturenAfdelingRadio(this IPage page) =>
            page.GetByRole(AriaRole.Radio, new() { Name = "Afdeling" });

        public static ILocator GetDoorsturenGroepRadio(this IPage page) =>
            page.GetByRole(AriaRole.Radio, new() { Name = "Groep" });

        public static ILocator GetDoorsturenMedewerkerRadio(this IPage page) =>
            page.GetByRole(AriaRole.Radio, new() { Name = "Medewerker" });

        public static ILocator GetAfdelingSelect(this IPage page) =>
            page.Locator("#afdelingSelect");

        public static ILocator GetGroepSelect(this IPage page) =>
            page.Locator("#groepSelect");

        public static ILocator GetMedewerkerCombobox(this IPage page) =>
            page.Locator("#medewerker-combobox");

        public static ILocator GetSecondaryPicker(this IPage page) =>
            page.Locator("#secondaryPicker");

        public static ILocator GetAfdelingGroepMedewerkerCombobox(this IPage page) =>
            page.Locator("#afdeling-groep-medewerker-combobox");

        public static ILocator GetGroepMedewerkerCombobox(this IPage page) =>
            page.Locator("#groep-medewerker-combobox");

        public static ILocator GetContactverzoekDoorsturenButton(this IPage page) =>
            page.GetByRole(AriaRole.Button, new() { Name = "Contactverzoek doorsturen" });
    }
}
