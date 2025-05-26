using System;

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