# ITA End-to-End Tests

Automated end-to-end tests for the ITA application using Playwright and MSTest with Azure AD authentication.

## Prerequisites

- .NET 8.0 SDK

## Setup

1. **Configure test credentials:**

   ```bash
   cd InterneTaakAfhandeling.EndToEndTest
   dotnet user-secrets set "TestSettings:TEST_BASE_URL" "https://your-ita-environment-url"
   dotnet user-secrets set "TestSettings:TEST_USERNAME" "test-account@domain.com"
   dotnet user-secrets set "TestSettings:TEST_PASSWORD" "test-account-password"
   dotnet user-secrets set "TestSettings:TEST_TOTP_SECRET" "base32-encoded-secret"  # See below
   ```

   **Getting the TOTP secret:**

   - The `TEST_TOTP_SECRET` is the base32-encoded secret key for 2FA authentication
   - This is usually obtained when setting up 2FA on the test account (the QR code contains this secret)
   - Contact your team lead or DevOps for the dedicated test account credentials and TOTP secret
   - If you don't have 2FA enabled on your test account, you can skip the `TEST_TOTP_SECRET` setting

   **Test Account:**

   - Ask your team for the dedicated ITA test account credentials
   - This should be a separate account specifically for automated testing
   - Do not use personal accounts for automated tests

2. **Install Playwright browsers (required):**

   ```bash
   dotnet build
   pwsh bin/Debug/net8.0/playwright.ps1 install
   ```

   _Note: This installs the browser binaries needed for Playwright tests._

## Running Tests

```bash
# Headless mode (CI/CD)
dotnet test

# With visible browser (local development)
HEADED=1 dotnet test

# Run specific test
dotnet test --filter "CanLoginAndAccessHomePage"
```

Tests automatically handle Azure AD login and 2FA authentication.
