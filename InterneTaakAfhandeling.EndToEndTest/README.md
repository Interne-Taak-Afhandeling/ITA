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
   dotnet user-secrets set "TestSettings:TEST_TOTP_SECRET" "base32-encoded-secret"
   ```

2. **Install Playwright browsers (required):**

   ```bash
   dotnet build
   pwsh bin/Debug/net8.0/playwright.ps1 install
   ```

## Running Tests

```bash
# Headless mode (CI/CD)
dotnet test

# Run specific test
dotnet test --filter "CanLoginAndAccessHomePage"
```

Tests automatically handle Azure AD login and 2FA authentication.
