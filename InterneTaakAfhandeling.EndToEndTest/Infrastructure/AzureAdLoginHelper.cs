using Microsoft.Playwright;

namespace InterneTaakAfhandeling.EndToEndTest.Infrastructure
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

            // we share auth state between tests, so we may already be logged in
            if (await uitloggenLink.IsVisibleAsync())
            {
                Console.WriteLine("✅ User already logged in, skipping login flow");
                return;
            }

            Console.WriteLine($"🔤 Entering username: {_username}");
            await usernameInput.FillAsync(_username);
            
            Console.WriteLine("🖱️ Clicking Next button...");
            await _page.ClickAsync("input[type='submit']");

            Console.WriteLine("⏳ Waiting for password page to load...");
            
            try
            {
                // Wait for password page with longer timeout and better error handling
                await _page.WaitForSelectorAsync("input[name='passwd']", new() { Timeout = 15000 });
                Console.WriteLine("✅ Password page loaded successfully");
            }
            catch (TimeoutException)
            {
                Console.WriteLine("❌ Password page did not load - checking current state");
                Console.WriteLine($"Current URL: {_page.Url}");
                
                // Try to find any error messages
                var errorElement = _page.Locator(".error, .alert, [role='alert'], .message-error");
                if (await errorElement.IsVisibleAsync())
                {
                    var errorText = await errorElement.TextContentAsync();
                    Console.WriteLine($"Error message found: {errorText}");
                }
                
                // Check if we're stuck on the username page
                if (await usernameInput.IsVisibleAsync())
                {
                    Console.WriteLine("⚠️ Still on username page - trying alternative approach");
                    
                    // Try clicking a more specific Next button
                    var nextButton = _page.Locator("input[id='idSIButton9'], input[value='Next']");
                    if (await nextButton.IsVisibleAsync())
                    {
                        Console.WriteLine("🔄 Trying specific Next button...");
                        await nextButton.ClickAsync();
                        await _page.WaitForSelectorAsync("input[name='passwd']", new() { Timeout = 10000 });
                    }
                }
                else
                {
                    throw new Exception("Password page failed to load and unknown state reached");
                }
            }

            Console.WriteLine("🔐 Entering password...");
            await _page.FillAsync("input[name='passwd']", _password);
            
            Console.WriteLine("🖱️ Clicking Sign In button...");
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
            await totpBoxSelector.Or(enterManuallyLink).First.WaitForAsync();

            if (await enterManuallyLink.IsVisibleAsync())
            {
                await enterManuallyLink.ClickAsync();
                await totpBoxSelector.WaitForAsync();
            }

            // Generate TOTP code
            var totpCode = await _uniqueOtpHelper.GetUniqueCode();
            // Fill in the TOTP code
            await totpBoxSelector.FillAsync(totpCode);
            await verifyButton.ClickAsync();

            // we will now either go back to the home page, or get an option to stay signed in
            await declineStaySignedInButton
                .Or(nieuwContactmomentSelector)
                .WaitForAsync();

            if (await declineStaySignedInButton.IsVisibleAsync())
            {
                await declineStaySignedInButton.ClickAsync();
            }
        }
    }
}

// using Microsoft.Playwright;

// namespace InterneTaakAfhandeling.EndToEndTest.Infrastructure
// {
//     /// <summary>
//     /// Helper class for handling Azure AD authentication flow in tests
//     /// Based on KISS-frontend implementation patterns
//     /// </summary>
//     public class AzureAdLoginHelper
//     {
//         private readonly IPage _page;
//         private readonly string _username;
//         private readonly string _password;
//         private readonly UniqueOtpHelper? _uniqueOtpHelper;

//         public AzureAdLoginHelper(IPage page, string username, string password, UniqueOtpHelper? uniqueOtpHelper = null)
//         {
//             _page = page;
//             _username = username;
//             _password = password;
//             _uniqueOtpHelper = uniqueOtpHelper;
//         }

//         public async Task LoginAsync()
//         {
//             await _page.GotoAsync("/");

//             var uitloggenLink = _page.GetByRole(AriaRole.Link, new() { Name = "Uitloggen" });
//             var usernameInput = _page.Locator("input[name='loginfmt']");

//             await uitloggenLink.Or(usernameInput).WaitForAsync();

//             // we share auth state between tests, so we may already be logged in
//             if (await uitloggenLink.IsVisibleAsync())
//             {
//                 return;
//             }

//             await usernameInput.FillAsync(_username);
//             await _page.ClickAsync("input[type='submit']");

//             await _page.WaitForSelectorAsync("input[name='passwd']");
//             await _page.FillAsync("input[name='passwd']", _password);
//             await _page.ClickAsync("input[type='submit']");

//             await Handle2FAAsync();
//         }

//         private async Task Handle2FAAsync()
//         {
//             var enterManuallyLink = _page.GetByRole(AriaRole.Link, new() { Name = "Microsoft Authenticator" });

//             var verifyButton = _page.GetByRole(AriaRole.Button, new() { Name = "Verifiëren" })
//                 .Or(_page.GetByRole(AriaRole.Button, new() { Name = "Verify" }));

//             var declineStaySignedInButton = _page.GetByRole(AriaRole.Button, new() { Name = "Nee" })
//                 .Or(_page.GetByRole(AriaRole.Button, new() { Name = "No" }));

//             var nieuwContactmomentSelector = _page.GetByRole(AriaRole.Button, new() { Name = "Nieuw contactmoment" });

//             var totpBoxSelector = _page.GetByRole(AriaRole.Textbox, new() { Name = "Code" });



//             // we will either get the 2FA code input and the submit button,
//             // or an option to enter the code manually
//             await totpBoxSelector.Or(enterManuallyLink).First.WaitForAsync();

//             if (await enterManuallyLink.IsVisibleAsync())
//             {
//                 await enterManuallyLink.ClickAsync();
//                 await totpBoxSelector.WaitForAsync();
//             }

//             // Generate TOTP code
//             var totpCode = await _uniqueOtpHelper.GetUniqueCode();
//             // Fill in the TOTP code
//             await totpBoxSelector.FillAsync(totpCode);
//             await verifyButton.ClickAsync();

//             // we will now either go back to the home page, or get an option to stay signed in
//             await declineStaySignedInButton
//                 .Or(nieuwContactmomentSelector)
//                 .WaitForAsync();

//             if (await declineStaySignedInButton.IsVisibleAsync())
//             {
//                 await declineStaySignedInButton.ClickAsync();
//             }
//         }
//     }
// }

//             await usernameInput.FillAsync(_username);
            
//             // Wait for the form to be ready before clicking
//             await Task.Delay(1000);
            
//             Console.WriteLine("🖱️ Clicking Next button...");
//             await _page.ClickAsync("input[type='submit']");

//             // Wait for password page and enter password (KISS pattern)
//             Console.WriteLine("⏳ Waiting for password page...");
//             try
//             {
//                 await _page.WaitForSelectorAsync("input[name='passwd']", new() { Timeout = 15000 });
//             }
//             catch (TimeoutException)
//             {
//                 Console.WriteLine($"⚠️ Timeout waiting for password page. Current URL: {_page.Url}");
                
//                 // Check if we're still on the username page or somewhere else
//                 var currentUrl = _page.Url;
//                 if (currentUrl.Contains("login.microsoftonline.com"))
//                 {
//                     // Try to find any visible input fields to debug
//                     var visibleInputs = await _page.Locator("input[type='password'], input[name='passwd'], input[type='email'], input[name='loginfmt']").CountAsync();
//                     Console.WriteLine($"🔍 Found {visibleInputs} visible input fields on current page");
                    
//                     if (visibleInputs == 0)
//                     {
//                         Console.WriteLine("💡 No input fields found - page might be loading. Waiting longer...");
//                         await Task.Delay(5000);
//                         await _page.WaitForSelectorAsync("input[name='passwd']", new() { Timeout = 10000 });
//                     }
//                 }
//                 else
//                 {
//                     throw;
//                 }
//             }
            
//             Console.WriteLine("🔐 Entering password...");
//             var passwordInput = _page.Locator("input[name='passwd']");
//             await passwordInput.WaitForAsync(new() { State = WaitForSelectorState.Visible });
//             await passwordInput.FillAsync(_password);
            
//             // Wait a moment before clicking submit
//             await Task.Delay(1000);
//             Console.WriteLine("🖱️ Clicking Sign in button...");
//             await _page.ClickAsync("input[type='submit']");deling.EndToEndTest.Infrastructure
// {
//     /// <summary>
//     /// Helper class for handling Azure AD authentication flow in tests
//     /// Based on KISS-frontend implementation patterns
//     /// </summary>
//     public class AzureAdLoginHelper
//     {
//         private readonly IPage _page;
//         private readonly string _username;
//         private readonly string _password;
//         private readonly UniqueOtpHelper? _uniqueOtpHelper;

//         public AzureAdLoginHelper(IPage page, string username, string password, UniqueOtpHelper? uniqueOtpHelper = null)
//         {
//             _page = page;
//             _username = username;
//             _password = password;
//             _uniqueOtpHelper = uniqueOtpHelper;
//         }

//         public async Task LoginAsync()
//         {
//             // Ensure the page is properly maximized and positioned
//             await EnsureProperWindowDisplayAsync();
            
//             await _page.GotoAsync("/");

//             // Check if already logged in by looking for logout link (KISS pattern)
//             var uitloggenLink = _page.GetByRole(AriaRole.Link, new() { Name = "Uitloggen" });
//             var usernameInput = _page.Locator("input[name='loginfmt']");

//             await uitloggenLink.Or(usernameInput).WaitForAsync();

//             // We share auth state between tests, so we may already be logged in (KISS pattern)
//             if (await uitloggenLink.IsVisibleAsync())
//             {
//                 Console.WriteLine("✅ User already logged in, skipping login flow");
//                 return;
//             }

//             // Enter username and click Next (KISS pattern)
//             Console.WriteLine($"� Entering username: {_username}");
//             await usernameInput.FillAsync(_username);
//             await _page.ClickAsync("input[type='submit']");

//             // Wait for password page and enter password (KISS pattern)
//             Console.WriteLine("⏳ Waiting for password page...");
//             await _page.WaitForSelectorAsync("input[name='passwd']");
            
//             Console.WriteLine("🔐 Entering password...");
//             await _page.FillAsync("input[name='passwd']", _password);
//             await _page.ClickAsync("input[type='submit']");

//             // Handle 2FA and Stay signed in prompts (KISS pattern)
//             await Handle2FAAsync();
//         }

//         private async Task Handle2FAAsync()
//         {
//             if (_uniqueOtpHelper == null) 
//             {
//                 Console.WriteLine("ℹ️ No OTP helper configured - skipping 2FA automation");
//                 return;
//             }

//             // KISS pattern: Simple locators and direct flow
//             var enterManuallyLink = _page.GetByRole(AriaRole.Link, new() { Name = "Microsoft Authenticator" });
//             var verifyButton = _page.GetByRole(AriaRole.Button, new() { Name = "Verifiëren" })
//                 .Or(_page.GetByRole(AriaRole.Button, new() { Name = "Verify" }));
//             var declineStaySignedInButton = _page.GetByRole(AriaRole.Button, new() { Name = "Nee" })
//                 .Or(_page.GetByRole(AriaRole.Button, new() { Name = "No" }));
//             var nieuwContactmomentSelector = _page.GetByRole(AriaRole.Button, new() { Name = "Nieuw contactmoment" });
//             var totpBoxSelector = _page.GetByRole(AriaRole.Textbox, new() { Name = "Code" });

//             // We will either get the 2FA code input and the submit button,
//             // or an option to enter the code manually (KISS pattern)
//             await totpBoxSelector.Or(enterManuallyLink).First.WaitForAsync();

//             if (await enterManuallyLink.IsVisibleAsync())
//             {
//                 Console.WriteLine("🖱️ Clicking 'Enter code manually' link");
//                 await enterManuallyLink.ClickAsync();
//                 await totpBoxSelector.WaitForAsync();
//             }

//             // Generate TOTP code (KISS pattern)
//             Console.WriteLine("🔑 Generating TOTP code...");
//             var totpCode = await _uniqueOtpHelper.GetUniqueCode();
//             Console.WriteLine($"✅ Generated TOTP code: {totpCode}");
            
//             // Fill in the TOTP code (KISS pattern)
//             await totpBoxSelector.FillAsync(totpCode);
//             await verifyButton.ClickAsync();
//             Console.WriteLine("✅ 2FA verification completed");

//             // We will now either go back to the home page, or get an option to stay signed in (KISS pattern)
//             await declineStaySignedInButton
//                 .Or(nieuwContactmomentSelector)
//                 .WaitForAsync();

//             if (await declineStaySignedInButton.IsVisibleAsync())
//             {
//                 Console.WriteLine("💡 Clicking 'No' to decline staying signed in (KISS pattern)");
//                 await declineStaySignedInButton.ClickAsync();
//             }
//         }

//         /// <summary>
//         /// Check if the user is currently logged in
//         /// </summary>
//         public async Task<bool> IsLoggedInAsync()
//         {
//             await _page.GotoAsync("/");

//             // Wait a moment for potential redirects
//             await Task.Delay(2000);

//             var currentUrl = _page.Url;

//             // If we're on the ITA domain and not being redirected to Microsoft login, we're logged in
//             return currentUrl.Contains("ita.test.icatt.nl") &&
//                    !currentUrl.Contains("login.microsoftonline.com");
//         }

//         /// <summary>
//         /// Logout the current user
//         /// </summary>
//         public async Task LogoutAsync()
//         {
//             var logoutSelectors = new[]
//             {
//                 "text=Sign out",
//                 "text=Logout",
//                 "text=Uitloggen",
//                 "[aria-label*='logout']",
//                 "[aria-label*='sign out']",
//                 ".logout",
//                 "#logout"
//             };

//             foreach (var selector in logoutSelectors)
//             {
//                 var element = _page.Locator(selector);
//                 if (await element.IsVisibleAsync())
//                 {
//                     await element.ClickAsync();
//                     break;
//                 }
//             }
//         }

//         /// <summary>
//         /// Ensures the browser window is properly displayed and maximized
//         /// Simple approach that doesn't interfere with Microsoft login functionality
//         /// </summary>
//         private async Task EnsureProperWindowDisplayAsync()
//         {
//             try
//             {
//                 // Set viewport to full size to ensure proper display
//                 await _page.SetViewportSizeAsync(1920, 1080);
                
//                 // Simple window maximization without aggressive CSS modifications
//                 await _page.EvaluateAsync(@"() => {
//                     // Try to maximize the window
//                     if (window.outerWidth < screen.availWidth || window.outerHeight < screen.availHeight) {
//                         window.moveTo(0, 0);
//                         window.resizeTo(screen.availWidth, screen.availHeight);
//                     }
//                 }");
                
//                 Console.WriteLine("✅ Window display optimized for login flow");
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine($"⚠️ Could not optimize window display: {ex.Message}");
//                 // Continue anyway - this is not critical for functionality
//             }
//         }
//     }
// }
