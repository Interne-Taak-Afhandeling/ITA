using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace InterneTaakAfhandeling.EndToEndTest.Infrastructure
{
    public static class Locators
    {
        // Dashboard locators
        public static ILocator GetDetailsLink(this IPage page, string onderwerp)
        {
            var testRow = page.GetByRole(AriaRole.Row).Filter(new() { HasText = onderwerp });
            return testRow.GetByRole(AriaRole.Link).Filter(new() { HasText = "Klik hier" });
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
            page.GetByText("Contactmoment registreren");

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
    }
}
