using Microsoft.Playwright;

namespace InterneTaakAfhandeling.EndToEndTest.Kanaal
{
    public class BeheerLocators
    {
        private readonly IPage page;

        public BeheerLocators(IPage page)
        {
            this.page = page;
        }

        // Beheer Menu Locators
        public ILocator BeheerLink => page.GetByRole(AriaRole.Link, new() { Name = "Beheer" });
        public ILocator KanalenLink => page.GetByRole(AriaRole.Link, new() { Name = "Kanalen" });
        public ILocator BeheerSubmenu => page.GetByRole(AriaRole.Navigation, new() { Name = "Beheer submenu" }).GetByRole(AriaRole.List);

        // Kanalen Page Locators
        public ILocator KanalenHeading => page.GetByRole(AriaRole.Heading, new() { Name = "Kanalen" });
        public ILocator KanaalToevoegenLink => page.GetByRole(AriaRole.Link, new() { Name = "Kanaal toevoegen" });
        public ILocator NaamTextbox => page.GetByRole(AriaRole.Textbox, new() { Name = "Naam *" });
        public ILocator OpslaanButton => page.GetByRole(AriaRole.Button, new() { Name = "Opslaan" });
        public ILocator AnnulerenButton => page.GetByRole(AriaRole.Link, new() { Name = "Annuleren" });
        
        // Dynamic Locators
        public ILocator GetKanaalLink(string kanaalName) 
            => page.GetByRole(AriaRole.Link, new() { Name = kanaalName, Exact = true });

        public ILocator GetKanaalListItem(string kanaalName) 
            => page.Locator($"li:has(a:text-is('{kanaalName}'))");

        public ILocator GetDeleteButton(string kanaalName) 
            => GetKanaalListItem(kanaalName).GetByRole(AriaRole.Button);
    }
}
