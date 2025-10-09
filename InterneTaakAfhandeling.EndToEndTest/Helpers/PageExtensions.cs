using Microsoft.Playwright;

namespace InterneTaakAfhandeling.EndToEndTest.Helpers
{
    /// <summary>
    /// Extension methods for Azure AD authentication and ITA page interactions
    /// </summary>
    public static class PageExtensions
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

        // Azure AD Authentication flow helpers
        public static async Task<bool> LoginWithAzureAdAsync(this IPage page, string username, string password)
        {
            try
            {
                // Wait for redirect to Microsoft login
                await page.WaitForURLAsync(url => url.Contains("login.microsoftonline.com"), new() { Timeout = 10000 });

                // Enter username/email
                var emailField = page.GetMicrosoftEmailField();
                await emailField.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 10000 });
                await emailField.FillAsync(username);

                // Click Next
                await page.GetMicrosoftNextButton().ClickAsync();

                // Wait for password page and enter password
                var passwordField = page.GetMicrosoftPasswordField();
                await passwordField.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 10000 });
                await passwordField.FillAsync(password);

                // Click Sign In
                await page.GetMicrosoftSignInButton().ClickAsync();

                // Handle "Stay signed in?" prompt if it appears
                var staySignedInPrompt = page.Locator("text=Stay signed in?");
                if (await staySignedInPrompt.IsVisibleAsync())
                {
                    await page.GetStaySignedInYesButton().ClickAsync();
                }

                // Wait for redirect back to ITA application
                await page.WaitForURLAsync(url => url.Contains("ita.test.icatt.nl") && !url.Contains("signin-oidc"),
                    new() { Timeout = 15000 });

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static async Task<bool> IsLoggedInAsync(this IPage page)
        {
            // Navigate to root and check if we stay on ITA domain
            await page.GotoAsync("/");
            await Task.Delay(2000); // Allow time for potential redirect

            var currentUrl = page.Url;
            return currentUrl.Contains("ita.test.icatt.nl") &&
                   !currentUrl.Contains("login.microsoftonline.com");
        }

        public static async Task LogoutAsync(this IPage page)
        {
            // Look for logout/sign out button or link
            var logoutSelectors = new[]
            {
                "text=Sign out",
                "text=Logout",
                "text=Log out",
                "[aria-label*='logout']",
                "[aria-label*='sign out']",
                ".logout",
                "#logout"
            };

            foreach (var selector in logoutSelectors)
            {
                var element = page.Locator(selector);
                if (await element.IsVisibleAsync())
                {
                    await element.ClickAsync();
                    break;
                }
            }
        }
    }
}
