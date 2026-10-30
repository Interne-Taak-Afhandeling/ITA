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

            // Some runs land on an intermediate Keycloak/broker "Sign in to your account" page
            // (generic username/password fields + an "Or sign in with ICATT" identity-provider
            // button) before being redirected to the actual Azure AD login (loginfmt) page.
            // Detect it and click through to Azure AD instead of hanging forever waiting for
            // a loginfmt field that will never appear on this intermediate page. Uses the same
            // specific selector as the known-working GlobalSetup.ClickIcattIfPromptedAsync race
            // (a generic role-based "ICATT" selector previously used here could match unrelated
            // elements or race unpredictably with concurrent callers).
            var icattIdpButton = _page.Locator("#social-oidc-icatt").First;

            await uitloggenLink.Or(usernameInput).Or(icattIdpButton).First.WaitForAsync(new() { Timeout = 30000 });

            if (await uitloggenLink.IsVisibleAsync())
            {
                return;
            }

            if (await icattIdpButton.IsVisibleAsync() && !await usernameInput.IsVisibleAsync())
            {
                await icattIdpButton.ClickAsync();
                await usernameInput.WaitForAsync(new() { Timeout = 30000 });
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

            // "Let's keep your account secure" / "We'll help you set up another way to verify
            // it's you" interstitial. This only appears the *first* time a given account
            // completes 2FA (Microsoft prompting to register an additional verification
            // method) — subsequent logins go straight to the decline/"Nieuw contactmoment"
            // outcome instead. Dismiss it via the "Not now" link so the flow can proceed.
            var notNowLink = _page.GetByRole(AriaRole.Link, new() { Name = "Not now" })
                .Or(_page.GetByRole(AriaRole.Link, new() { Name = "Niet nu" }));

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

                // Race the possible outcomes independently rather than combining them with
                // .Or().First — the error alert element is typically always present in the DOM
                // (just hidden when inactive) and can sort first in DOM order, which would make
                // .Or().First latch onto it and wait forever for it to become visible even on a
                // successful login. Waiting on each outcome separately and taking whichever
                // resolves first avoids that trap. 30s matches the timeout the original,
                // known-working implementation effectively relied on (Playwright's default
                // actionability timeout) — CI runners can be noticeably slower than local runs,
                // and a shorter timeout here risks false failures before the landing page settles.
                const int resultTimeoutMs = 30000;
                var declineTask = declineStaySignedInButton.WaitForAsync(new() { Timeout = resultTimeoutMs });
                var nieuwContactmomentTask = nieuwContactmomentSelector.WaitForAsync(new() { Timeout = resultTimeoutMs });
                var notNowTask = notNowLink.WaitForAsync(new() { Timeout = resultTimeoutMs });
                var errorTask = errorMessageSelector.WaitForAsync(new() { Timeout = resultTimeoutMs });

                var completedTask = await Task.WhenAny(declineTask, nieuwContactmomentTask, notNowTask, errorTask);

                // Observe the losing tasks' exceptions (timeouts) so they don't surface later
                // as unobserved task exceptions once they complete/fault in the background.
                _ = declineTask.ContinueWith(t => _ = t.Exception, TaskContinuationOptions.OnlyOnFaulted);
                _ = nieuwContactmomentTask.ContinueWith(t => _ = t.Exception, TaskContinuationOptions.OnlyOnFaulted);
                _ = notNowTask.ContinueWith(t => _ = t.Exception, TaskContinuationOptions.OnlyOnFaulted);
                _ = errorTask.ContinueWith(t => _ = t.Exception, TaskContinuationOptions.OnlyOnFaulted);

                try
                {
                    await completedTask; // propagate exception (e.g. timeout) if that's what "won"
                }
                catch (TimeoutException)
                {
                    // None of the expected post-verify outcomes appeared in time. Surface the
                    // current page URL/title so CI logs show what state the browser was actually
                    // in, instead of just "waiting for X to be visible" with no further context.
                    var currentUrl = _page.Url;
                    string? currentTitle = null;
                    try { currentTitle = await _page.TitleAsync(); } catch { /* best-effort */ }

                    // Only retry if we're demonstrably still on the code-entry screen — if the
                    // page has already navigated away (e.g. to an unexpected intermediate screen),
                    // the totp textbox may no longer exist at all, and blindly calling FillAsync on
                    // it here would itself hang for the full timeout instead of failing fast.
                    // IsVisibleAsync() has no timeout and returns immediately (true/false), so it's
                    // safe to call even if the element is no longer present.
                    var stillOnCodeScreen = await totpBoxSelector.IsVisibleAsync();

                    if (attempt == maxAttempts || !stillOnCodeScreen)
                    {
                        throw new TimeoutException(
                            $"Timed out waiting for a post-2FA-verify outcome (decline/\"Nieuw contactmoment\"/" +
                            $"\"Not now\"/error) after attempt {attempt}/{maxAttempts} (stillOnCodeScreen={stillOnCodeScreen}). " +
                            $"Current page: url='{currentUrl}', title='{currentTitle}'.");
                    }

                    // Treat an unresolved post-verify state the same as a detected error: retry
                    // once with a fresh code in case the first one was stale/already consumed.
                    await totpBoxSelector.FillAsync("");
                    continue;
                }

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

            // Only shown the first time an account completes 2FA ("Let's keep your account
            // secure" — set up another verification method). Dismiss it so we land on the
            // normal decline/"Nieuw contactmoment" screen; on subsequent runs it won't appear
            // and this is a no-op.
            if (await notNowLink.IsVisibleAsync())
            {
                await notNowLink.ClickAsync();
            }

            if (await declineStaySignedInButton.IsVisibleAsync())
            {
                await declineStaySignedInButton.ClickAsync();
            }
        }
    }
}
