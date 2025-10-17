using Microsoft.Playwright;

namespace EndToEndTest.Common.Helpers
{
    /// <summary>
    /// Extension methods for Azure AD authentication and common page interactions
    /// </summary>
    public static class AzureAdPageExtensions
    {
        // Azure AD Authentication locators
        public static ILocator GetMicrosoftEmailField(this IPage page)
        {
            return page.Locator("input[name='loginfmt']");
        }

        public static ILocator GetMicrosoftPasswordField(this IPage page)
        {
            return page.Locator("input[id='i0118'][name='passwd']");
        }

        public static ILocator GetMicrosoftNextButton(this IPage page)
        {
            return page.Locator("input[id='idSIButton9'][value='Next']");
        }

        public static ILocator GetMicrosoftSignInButton(this IPage page)
        {
            return page.Locator("input[id='idSIButton9'][value='Sign in']");
        }

        public static ILocator GetStaySignedInYesButton(this IPage page)
        {
            return page.Locator("input[type='submit'][value='Yes'], input[id='idSIButton9']");
        }

        public static ILocator GetStaySignedInNoButton(this IPage page)
        {
            return page.GetByRole(AriaRole.Button, new() { Name = "Nee" })
                .Or(page.GetByRole(AriaRole.Button, new() { Name = "No" }));
        }

        public static ILocator GetLogoutLink(this IPage page)
        {
            return page.GetByRole(AriaRole.Link, new() { Name = "Logout" })
                .Or(page.GetByRole(AriaRole.Link, new() { Name = "Uitloggen" }));
        }

    }
}
