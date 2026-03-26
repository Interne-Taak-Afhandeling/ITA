using Microsoft.Playwright;

namespace InterneTaakAfhandeling.EndToEndTest.Werklijst
{
    public static class WerklijstLocators
    {
        public static ILocator GetWerklijstNavLink(this IPage page) =>
            page.GetByRole(AriaRole.Link, new() { Name = "Werklijst" });

        public static ILocator GetWerklijstHeading(this IPage page) =>
            page.GetByRole(AriaRole.Heading, new() { Name = "Werklijst" });

        public static ILocator GetWerklijstTable(this IPage page) =>
            page.Locator("table.utrecht-table");

        public static ILocator GetWerklijstTableRows(this IPage page) =>
            page.Locator("table.utrecht-table tbody tr");

        public static ILocator GetWerklijstPaginering(this IPage page) =>
            page.GetByRole(AriaRole.Navigation, new() { Name = "Paginering" });

        public static ILocator GetWerklijstVolgendeButton(this IPage page) =>
            page.GetByRole(AriaRole.Button, new() { Name = "Volgende" });

        public static ILocator GetWerklijstVorigeButton(this IPage page) =>
            page.GetByRole(AriaRole.Button, new() { Name = "Vorige" });
    }
}
