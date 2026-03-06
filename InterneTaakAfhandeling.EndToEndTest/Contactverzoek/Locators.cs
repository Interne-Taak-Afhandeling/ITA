using Microsoft.Playwright;

namespace InterneTaakAfhandeling.EndToEndTest.Contactverzoek
{
    public static class ContactverzoekLocators
    {
        // Button locators
        public static ILocator GetToewijzenAanMezelfButton(this IPage page) =>
            page.GetByRole(AriaRole.Button, new() { Name = "Toewijzen aan mezelf" });

        public static ILocator GetToewijzenAanMezelfDialogButton(this IPage page) =>
            page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Button, new() { Name = "Toewijzen aan mezelf" });

        public static ILocator GetAnnulerenDialogButton(this IPage page) =>
            page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Button, new() { Name = "Annuleren" });

        // Message locators
        public static ILocator GetContactverzoekToegwezenMessage(this IPage page) =>
            page.GetByText("Contactverzoek toegewezen");
    }
}
