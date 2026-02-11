using Microsoft.Playwright;

namespace InterneTaakAfhandeling.EndToEndTest.Kanaal
{
    public class KanalenPage
    {
        private readonly BeheerLocators locators;

        public KanalenPage(IPage page)
        {
            locators = new BeheerLocators(page);
        }

        // Expose all locators
        public ILocator BeheerLink => locators.BeheerLink;
        public ILocator KanalenLink => locators.KanalenLink;
        public ILocator BeheerSubmenu => locators.BeheerSubmenu;
        public ILocator KanalenHeading => locators.KanalenHeading;
        public ILocator KanaalToevoegenLink => locators.KanaalToevoegenLink;
        public ILocator NaamTextbox => locators.NaamTextbox;
        public ILocator OpslaanButton => locators.OpslaanButton;
        public ILocator AnnulerenButton => locators.AnnulerenButton;

        // Expose dynamic locator methods
        public ILocator GetKanaalLink(string kanaalName) => locators.GetKanaalLink(kanaalName);
        public ILocator GetKanaalListItem(string kanaalName) => locators.GetKanaalListItem(kanaalName);
        public ILocator GetDeleteButton(string kanaalName) => locators.GetDeleteButton(kanaalName);
    }
}
