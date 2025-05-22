using System;
using System.Security.Cryptography;
using System.Text;

namespace InterneTaakAfhandeling.Common.Helpers
{
    /// <summary>
    /// Helper class for secure logging that prevents log injection and protects sensitive data
    /// </summary>
    public static class SecureLogging
    {
        /// <summary>
        /// Sanitizes a string for safe logging by removing potentially harmful characters
        /// </summary>
        /// <param name="input">The input string to sanitize</param>
        /// <returns>A sanitized string safe for logging</returns>
        public static string SanitizeForLogging(string? input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            // Remove newline characters that could be used for log injection
            return input
                .Replace("\n", "")
                .Replace("\r", "")
                .Replace("\t", " ") // Replace tabs with spaces
                .Trim();
        }

        /// <summary>
        /// Creates a privacy-safe identifier for logging purposes
        /// Generates a consistent hash that can be used for correlation without exposing the original value
        /// </summary>
        /// <param name="sensitiveValue">The sensitive value to create an identifier for</param>
        /// <returns>A safe identifier for logging</returns>
        public static string CreateSafeIdentifier(string? sensitiveValue)
        {
            if (string.IsNullOrEmpty(sensitiveValue))
                return "[EMPTY]";

            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(sensitiveValue));

            // Take first 8 characters of hash for a short, safe identifier
            var hashString = Convert.ToHexString(hashBytes);
            return $"[{hashString[..8]}]";
        }

        /// <summary>
        /// Masks an email address for logging, showing only first character and domain
        /// </summary>
        /// <param name="email">The email to mask</param>
        /// <returns>A masked email safe for logging</returns>
        public static string MaskEmail(string? email)
        {
            if (string.IsNullOrEmpty(email))
                return "[NO_EMAIL]";

            var sanitized = SanitizeForLogging(email);

            if (!sanitized.Contains('@'))
                return "[INVALID_EMAIL]";

            var parts = sanitized.Split('@');
            if (parts.Length != 2)
                return "[INVALID_EMAIL]";

            var localPart = parts[0];
            var domain = parts[1];

            // Show first character of local part + *** + @domain
            var maskedLocal = localPart.Length > 0 ? $"{localPart[0]}***" : "***";
            return $"{maskedLocal}@{domain}";
        }

        /// <summary>
        /// Sanitizes and truncates a string for safe logging with length limit
        /// </summary>
        /// <param name="input">The input string</param>
        /// <param name="maxLength">Maximum length to log (default 100)</param>
        /// <returns>A sanitized and truncated string</returns>
        public static string SanitizeAndTruncate(string? input, int maxLength = 100)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            var sanitized = SanitizeForLogging(input);

            if (sanitized.Length <= maxLength)
                return sanitized;

            return sanitized[..(maxLength - 3)] + "...";
        }

        /// <summary>
        /// Creates a safe UUID string for logging
        /// </summary>
        /// <param name="uuid">The UUID to sanitize</param>
        /// <returns>A sanitized UUID or safe placeholder</returns>
        public static string SanitizeUuid(string? uuid)
        {
            if (string.IsNullOrEmpty(uuid))
                return "[NO_UUID]";

            var sanitized = SanitizeForLogging(uuid);

            // Validate it looks like a UUID
            if (Guid.TryParse(sanitized, out _))
                return sanitized;

            return "[INVALID_UUID]";
        }
    }
}