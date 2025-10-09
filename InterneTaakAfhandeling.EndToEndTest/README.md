# ITA End-to-End Tests

Automated end-to-end tests for the Interne Taak Afhandeling (ITA) application using Playwright and MSTest.

Built following **KISS-frontend patterns** with automatic Azure AD authentication, 2FA support, and comprehensive test reporting.

## Project Structure

```
InterneTaakAfhandeling.EndToEndTest/
├── Infrastructure/                    # Core KISS framework
│   ├── ITAPlaywrightTest.cs              # Base test class with auto-login
│   ├── AzureAdLoginHelper.cs             # Azure AD authentication
│   └── UniqueOtpHelper.cs                # 2FA/TOTP handling
├── Authentication/                    # Test scenarios
│   └── AuthenticationScenarios.cs        # Login/authentication tests
├── Helpers/                          # Utilities (add as needed)
├── .env                              # Environment configuration (non-sensitive)
├── .env.example                      # Example environment file
├── .gitignore                        # Git ignore patterns
└── README.md
```

### Prerequisites

- .NET 8.0 SDK
- Visual Studio 2022 or VS Code

### Setup & Run

1. **Configure credentials (optional for authenticated tests):**

   ```bash
   cd InterneTaakAfhandeling.EndToEndTest
   dotnet user-secrets set "TestSettings:TEST_BASE_URL" "xx"
   dotnet user-secrets set "TestSettings:TEST_USERNAME" "xx"
   dotnet user-secrets set "TestSettings:TEST_PASSWORD" "xx"
   dotnet user-secrets set "TestSettings:TEST_TOTP_SECRET" "xx"  # Optional for 2FA
   ```

2. **Run tests:**

   ```bash
   # Headless mode (CI/CD)
   dotnet test

   # With visible browser (development/debugging)
   HEADED=1 dotnet test

   # Specific test with browser
   HEADED=1 dotnet test --filter "CanLoginAndAccessHomePage"
   ```
