using Microsoft.Playwright;

namespace ITA.InterneTaakAfhandeling.EndToEndTest.Helpers
{
    public static class SharedCode
    {
        public static async Task CreateNewContactmomentAsync(this IPage page)
        {
            await page.GetByRole(AriaRole.Button, new() { Name = "Nieuw contactmoment" }).ClickAsync();
        }

        public static async Task CreateNewcontactVerzoekAsync(this IPage page)
        {
            await page.GetByRole(AriaRole.Tab, new() { Name = "Contactverzoek" }).ClickAsync();
        }

        public static ILocator GetAfdelingRadioButton(this IPage page)
        {
            return page.GetByRole(AriaRole.Radio, new() { Name = "Afdeling" });
        }

        public static ILocator GetGroupRadioButton(this IPage page)
        {
            return page.GetByRole(AriaRole.Radio, new() { Name = "Groep" });
        }

        public static ILocator GetMedewerkerRadioButton(this IPage page)
        {
            return page.GetByRole(AriaRole.Radio, new() { Name = "Medewerker" });
        }

        public static ILocator GetAfdelingCombobox(this IPage page)
        {
            return page.GetByRole(AriaRole.Combobox, new() { Name = "Afdeling" });
        }

        public static ILocator GetAfdelingGroepDropdown(this IPage page)
        {
            return page.GetByLabel("Afdeling / groep");
        }

        public static ILocator GetGroupCombobox(this IPage page)
        {
            return page.GetByRole(AriaRole.Combobox, new() { Name = "Groep" });
        }

        public static ILocator GetAfdelingTextbox(this IPage page)
        {
            return page.GetByRole(AriaRole.Textbox, new() { Name = "Afdeling" });
        }

        public static ILocator GetInterneToelichtingTextbox(this IPage page)
        {
            return page.GetByRole(AriaRole.Textbox, new() { Name = "Interne toelichting voor medewerker" });
        }

        public static ILocator GetEmailTextbox(this IPage page)
        {
            return page.GetByRole(AriaRole.Textbox, new() { Name = "E-mailadres" });
        }

        public static ILocator GetTelefoonnummerTextbox(this IPage page)
        {
            return page.GetByRole(AriaRole.Textbox, new() { Name = "Telefoonnummer" });
        }

        public static ILocator GetSpecifiekeVraagTextbox(this IPage page)
        {
            return page.GetByRole(AriaRole.Textbox, new() { Name = "Specifieke vraag" });
        }

        public static ILocator GetContactverzoekSearchBar(this IPage page)
        {
            return page.GetByRole(AriaRole.Textbox, new() { Name = "Telefoonnummer of e-mailadres" });
        }

        public static ILocator GetTelefoonnummer1field(this IPage page)
        {
            return page.Locator("input[name='telefoonnummer1']");
        }

        public static ILocator GetTelefoonnummer2field(this IPage page)
        {
            return page.Locator("input[name='telefoonnummer2']");
        }

        public static ILocator GetEmailfield(this IPage page)
        {
            return page.Locator("input[name='email']");
        }

        // Button Locators
        public static ILocator GetAfrondenButton(this IPage page)
        {
            return page.GetByRole(AriaRole.Button, new() { Name = "Afronden" });
        }

        public static ILocator GetOpslaanButton(this IPage page)
        {
            return page.GetByRole(AriaRole.Button, new() { Name = "Opslaan" });
        }

        public static ILocator GetZoekenButton(this IPage page)
        {
            return page.GetByRole(AriaRole.Main).GetByRole(AriaRole.Button, new() { Name = "Zoeken" });
        }

        // Navigation Link Locators
        public static ILocator GetContactverzoekenLink(this IPage page)
        {
            return page.GetByRole(AriaRole.Link, new() { Name = "Contactverzoeken" });
        }

        // Success Message Locators
        public static ILocator GetContactVerzoekSuccessToast(this IPage page)
        {
            return page.Locator(".toast.success");
        }

        public static ILocator GetKanaalField(this IPage page)
        {
            return page.GetByLabel("Kanaal");
        }

        public static ILocator GetAfdelingField(this IPage page)
        {
            return page.GetByLabel("Afdeling / groep Afdeling:");
        }

        public static ILocator GetOverviewTable(this IPage page)
        {
            return page.Locator("table.overview");
        }

        public static ILocator GetTableRows(this IPage page)
        {
            return page.Locator("table.overview tbody tr");
        }

        public static async Task SelectDropdownOptionAsync(this IPage page, string fieldName, string optionText)
        {
            await page.GetByRole(AriaRole.Combobox, new() { Name = fieldName }).FillAsync(optionText);
            await page.GetByText(optionText).ClickAsync();
        }

        public static async Task<ILocator> WaitForContactDetailsAsync(this IPage page, string contactInfo)
        {
            var contactDetails = page.GetByText(contactInfo).First;
            await contactDetails.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
            return contactDetails;
        }

        public static ILocator FilterTableByText(this IPage page, string searchText)
        {
            return page.Locator("table.overview tbody tr").Filter(new LocatorFilterOptions
            {
                Has = page.GetByText(searchText)
            });
        }
    }
}