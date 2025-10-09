using Microsoft.Playwright;

namespace InterneTaakAfhandeling.EndToEndTest.Helpers
{
    /// <summary>
    /// Extension methods for common page interactions in ITA
    /// </summary>
    public static class PageExtensions
    {
        // Azure AD Authentication helpers
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
            return page.Locator("input[type='submit'][value='No'], input[id='idBtn_Back']");
        }

        // Legacy navigation helpers (for compatibility)
        public static ILocator GetLoginButton(this IPage page)
        {
            return page.GetByRole(AriaRole.Button, new() { Name = "Login" });
        }

        public static ILocator GetUsernameField(this IPage page)
        {
            return page.GetByLabel("Username");
        }

        public static ILocator GetPasswordField(this IPage page)
        {
            return page.GetByLabel("Password");
        }

        // Main navigation
        public static ILocator GetNavigationMenu(this IPage page)
        {
            return page.GetByRole(AriaRole.Navigation);
        }

        public static ILocator GetDashboardLink(this IPage page)
        {
            return page.GetByRole(AriaRole.Link, new() { Name = "Dashboard" });
        }

        public static ILocator GetTasksLink(this IPage page)
        {
            return page.GetByRole(AriaRole.Link, new() { Name = "Tasks" });
        }

        // Task management
        public static ILocator GetCreateTaskButton(this IPage page)
        {
            return page.GetByRole(AriaRole.Button, new() { Name = "Create Task" });
        }

        public static ILocator GetTaskTitleField(this IPage page)
        {
            return page.GetByLabel("Task Title");
        }

        public static ILocator GetTaskDescriptionField(this IPage page)
        {
            return page.GetByLabel("Description");
        }

        public static ILocator GetSaveTaskButton(this IPage page)
        {
            return page.GetByRole(AriaRole.Button, new() { Name = "Save" });
        }

        public static ILocator GetTasksList(this IPage page)
        {
            return page.GetByRole(AriaRole.List, new() { Name = "Tasks" });
        }

        public static ILocator GetTaskByTitle(this IPage page, string title)
        {
            return page.GetByText(title);
        }

        // Search functionality
        public static ILocator GetSearchField(this IPage page)
        {
            return page.GetByRole(AriaRole.Searchbox);
        }

        public static ILocator GetSearchButton(this IPage page)
        {
            return page.GetByRole(AriaRole.Button, new() { Name = "Search" });
        }

        // Common form elements
        public static ILocator GetSubmitButton(this IPage page)
        {
            return page.GetByRole(AriaRole.Button, new() { Name = "Submit" });
        }

        public static ILocator GetCancelButton(this IPage page)
        {
            return page.GetByRole(AriaRole.Button, new() { Name = "Cancel" });
        }

        public static ILocator GetDeleteButton(this IPage page)
        {
            return page.GetByRole(AriaRole.Button, new() { Name = "Delete" });
        }

        public static ILocator GetEditButton(this IPage page)
        {
            return page.GetByRole(AriaRole.Button, new() { Name = "Edit" });
        }

        // Alert and notification helpers
        public static ILocator GetSuccessMessage(this IPage page)
        {
            return page.GetByRole(AriaRole.Alert).Filter(new() { HasText = "success" });
        }

        public static ILocator GetErrorMessage(this IPage page)
        {
            return page.GetByRole(AriaRole.Alert).Filter(new() { HasText = "error" });
        }

        // Table helpers
        public static ILocator GetTableHeader(this IPage page, string headerText)
        {
            return page.GetByRole(AriaRole.Columnheader, new() { Name = headerText });
        }

        public static ILocator GetTableRow(this IPage page, string rowText)
        {
            return page.GetByRole(AriaRole.Row).Filter(new() { HasText = rowText });
        }

        // Modal helpers
        public static ILocator GetModal(this IPage page)
        {
            return page.GetByRole(AriaRole.Dialog);
        }

        public static ILocator GetModalTitle(this IPage page)
        {
            return page.GetByRole(AriaRole.Heading).First;
        }

        public static ILocator GetModalCloseButton(this IPage page)
        {
            return page.GetByRole(AriaRole.Button, new() { Name = "Close" });
        }

        // Helper methods for common actions
        public static async Task NavigateToTasksAsync(this IPage page)
        {
            await page.GetTasksLink().ClickAsync();
        }

        public static async Task NavigateToDashboardAsync(this IPage page)
        {
            await page.GetDashboardLink().ClickAsync();
        }

        public static async Task CreateNewTaskAsync(this IPage page, string title, string description)
        {
            await page.GetCreateTaskButton().ClickAsync();
            await page.GetTaskTitleField().FillAsync(title);
            await page.GetTaskDescriptionField().FillAsync(description);
            await page.GetSaveTaskButton().ClickAsync();
        }

        public static async Task SearchForAsync(this IPage page, string searchTerm)
        {
            await page.GetSearchField().FillAsync(searchTerm);
            await page.GetSearchButton().ClickAsync();
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
