using OtpNet;

namespace EndToEndTest.Common.Infrastructure
{
    /// <summary>
    /// Helper for generating unique TOTP codes for Azure AD 2FA
    /// The Azure AD 2FA login method checks to see the same one-time-password is only used once.
    /// Because of this, we need to make sure the same OTP is not re-used across tests.
    /// We cannot change the lifetime of OTPs, so we actually need to wait until the previous one expires.
    /// </summary>
    public class UniqueOtpHelper
    {
        private readonly Totp _otp;
        private string? _oldCode;
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public UniqueOtpHelper(string totpSecret)
        {
            // Secrets copied from setup UIs often contain spaces or are lowercase;
            // Base32Encoding.ToBytes requires clean, uppercase Base32 without padding/spaces.
            var sanitized = totpSecret
                .Replace(" ", "")
                .Replace("-", "")
                .Trim()
                .ToUpperInvariant();

            var secretBytes = Base32Encoding.ToBytes(sanitized);
            _otp = new Totp(secretBytes);
        }

        /// <summary>
        /// Minimum seconds remaining on a TOTP code to consider it "fresh enough"
        /// to survive the network round-trip to Azure AD.
        /// </summary>
        private const int MinRemainingSeconds = 10;

        public async Task<string> GetUniqueCode()
        {
            await _semaphore.WaitAsync();
            try
            {
                string newCode;
                while (true)
                {
                    var remainingSeconds = _otp.RemainingSeconds();
                    newCode = _otp.ComputeTotp(DateTime.UtcNow);

                    if (newCode != _oldCode && remainingSeconds >= MinRemainingSeconds)
                    {
                        break;
                    }

                    await Task.Delay(TimeSpan.FromSeconds(remainingSeconds + 1));
                }

                _oldCode = newCode;
                return newCode;
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
