using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace EndToEndTest.Common.Infrastructure
{
    /// <summary>
    /// Helper class for handling Azure AD authentication flow in tests
    /// Based on KISS-frontend implementation patterns
    /// </summary>
    public class AzureAdLoginHelper
    {
        private readonly IPage _page;
        private readonly string _username;
        private readonly string _password;
        private readonly UniqueOtpHelper _uniqueOtpHelper;

        public AzureAdLoginHelper(IPage page, string username, string password, UniqueOtpHelper uniqueOtpHelper)
        {
            _page = page;
            _username = username;
            _password = password;
            _uniqueOtpHelper = uniqueOtpHelper;
        }

        public async Task LoginAsync()
        {
            await _page.GotoAsync("/");

            var uitloggenLink = _page.GetByRole(AriaRole.Link, new() { Name = "Uitloggen" });
            var usernameInput = _page.Locator("input[name='loginfmt']");

            await uitloggenLink.Or(usernameInput).WaitForAsync();

            if (await uitloggenLink.IsVisibleAsync())
            {
                return;
            }

            await usernameInput.FillAsync(_username);
            await _page.ClickAsync("input[type='submit']");

            try
            {
                // Wait for password page with longer timeout and better error handling
                await _page.WaitForSelectorAsync("input[name='passwd']", new() { Timeout = 15000 });
            }
            catch (TimeoutException)
            {
                var errorElement = _page.Locator(".error, .alert, [role='alert'], .message-error");
                if (await errorElement.IsVisibleAsync())
                {
                    var errorText = await errorElement.TextContentAsync();
                }

                // Check if we're stuck on the username page
                if (await usernameInput.IsVisibleAsync())
                {
                    // Try clicking a more specific Next button
                    var nextButton = _page.Locator("input[id='idSIButton9'], input[value='Next']");
                    if (await nextButton.IsVisibleAsync())
                    {
                        await nextButton.ClickAsync();
                        await _page.WaitForSelectorAsync("input[name='passwd']", new() { Timeout = 10000 });
                    }
                }
                else
                {
                    throw new Exception("Password page failed to load and unknown state reached");
                }
            }

            await _page.FillAsync("input[name='passwd']", _password);
            await _page.ClickAsync("input[type='submit']");

            await Handle2FAAsync();
        }

        private async Task Handle2FAAsync()
        {
            var enterManuallyLink = _page.GetByRole(AriaRole.Link, new() { Name = "Microsoft Authenticator" });

            var verifyButton = _page.GetByRole(AriaRole.Button, new() { Name = "Verifiëren" })
                .Or(_page.GetByRole(AriaRole.Button, new() { Name = "Verify" }));

            var declineStaySignedInButton = _page.GetByRole(AriaRole.Button, new() { Name = "Nee" })
                .Or(_page.GetByRole(AriaRole.Button, new() { Name = "No" }));

            var nieuwContactmomentSelector = _page.GetByRole(AriaRole.Button, new() { Name = "Nieuw contactmoment" });

            var totpBoxSelector = _page.GetByRole(AriaRole.Textbox, new() { Name = "Code" });

            // we will either get the 2FA code input and the submit button,
            // or an option to enter the code manually
            await totpBoxSelector.Or(enterManuallyLink).First.WaitForAsync(new() { Timeout = 30000 });

            if (await enterManuallyLink.IsVisibleAsync())
            {
                await enterManuallyLink.ClickAsync();
                await totpBoxSelector.WaitForAsync();
            }

            // Some identity broker flows (e.g. Keycloak) perform an extra redirect/reload right
            // around the moment the code field first appears. Let the page settle and re-confirm
            // the code field is still the final, stable one before we generate/fill a code,
            // otherwise we risk submitting a (correctly computed) code against a sign-in session
            // that has already been superseded by a subsequent navigation.
            try
            {
                await _page.WaitForLoadStateAsync(LoadState.NetworkIdle, new() { Timeout = 5000 });
            }
            catch (TimeoutException)
            {
                // Some pages keep long-lived connections open and never reach network idle; ignore.
            }

            await totpBoxSelector.WaitForAsync(new() { State = WaitForSelectorState.Visible });

            var errorMessageSelector = _page.Locator("#error, [role='alert'], .error, .alert");

            var maxAttempts = 2;
            for (var attempt = 1; attempt <= maxAttempts; attempt++)
            {
                // Generate TOTP code immediately before filling — any delay here
                // (page settle waits, etc.) eats into the code's validity window.
                await totpBoxSelector.FillAsync("");
                var totpCode = await _uniqueOtpHelper.GetUniqueCode();
                await totpBoxSelector.FillAsync(totpCode);
                await Expect(totpBoxSelector).ToHaveValueAsync(totpCode);

                // The Verify button can remain disabled for a brief moment after
                // fill while client-side validation runs; wait for it to be
                // enabled before clicking to avoid a no-op click.
                await Expect(verifyButton).ToBeEnabledAsync(new() { Timeout = 10000 });
                await verifyButton.ClickAsync();

                var resultLocator = declineStaySignedInButton
                    .Or(nieuwContactmomentSelector)
                    .Or(errorMessageSelector);

                await resultLocator.First.WaitForAsync(new() { Timeout = 15000 });

                if (await errorMessageSelector.IsVisibleAsync())
                {
                    if (attempt == maxAttempts)
                    {
                        var errorText = await errorMessageSelector.TextContentAsync();
                        throw new Exception($"OTP verification failed after {maxAttempts} attempts: {errorText}");
                    }

                    // Code was likely stale/reused; wait for the field to clear
                    // (or clear it ourselves) and retry with a fresh code.
                    await totpBoxSelector.FillAsync("");
                    continue;
                }

                break;
            }

            if (await declineStaySignedInButton.IsVisibleAsync())
            {
                await declineStaySignedInButton.ClickAsync();
            }
        }
    }
}
