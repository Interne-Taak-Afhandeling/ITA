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
            var secretBytes = Base32Encoding.ToBytes(totpSecret);
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
                var remainingSeconds = _otp.RemainingSeconds();
                var newCode = _otp.ComputeTotp();

                if (newCode == _oldCode || remainingSeconds < MinRemainingSeconds)
                {
                    // Either the code was already used, or it's about to expire.
                    // Wait for the next TOTP window so we get a fresh code with max lifetime.
                    await Task.Delay(TimeSpan.FromSeconds(remainingSeconds + 1));
                    newCode = _otp.ComputeTotp();
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
