using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace InterneTaakAfhandeling.Common.Services.Zgw
{
    /// <summary>
    /// Extension methods for registering the ZgwTokenProvider.
    /// </summary>
    public static class ZgwTokenExtensions
    {
        public static IServiceCollection AddZgwTokenProvider(this IServiceCollection services, string apiKey, string clientId, string? userIdClaimType = null)
        {
            services.AddSingleton(new ZgwTokenProvider(apiKey, clientId, userIdClaimType));
            return services;
        }
    }

    /// <summary>
    /// Generates JWT tokens for ZGW APIs (OpenZaak, podiumd-adapter, etc.)
    /// </summary>
    public class ZgwTokenProvider
    {
        private readonly string _apiKey;
        private readonly string _clientId;
        private readonly string? _userIdClaimType;

        public ZgwTokenProvider(string apiKey, string clientId, string? userIdClaimType = null)
        {
            _apiKey = apiKey;
            _clientId = clientId;
            _userIdClaimType = userIdClaimType;
        }

        public string GenerateToken(ClaimsPrincipal? claimsPrincipal)
        {
            var userId = GetUserIdentifier(claimsPrincipal);
            var userRepresentation = claimsPrincipal?.Identity?.Name ?? string.Empty;

            // one minute leeway to account for clock differences between machines
            var timeNow = DateTime.UtcNow.AddMinutes(-1);
            var issuer = _clientId;

            var claims = new Dictionary<string, object>
            {
                { "client_id", _clientId },
                { "user_id", userId },
                { "user_representation", userRepresentation }
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_apiKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                IssuedAt = timeNow,
                // nbf and expires required or else it is automatically added by the library with a default value
                // this keeps time consistent and adds our leeway
                NotBefore = timeNow,
                Expires = timeNow.AddHours(1),
                Issuer = issuer,
                Claims = claims,
                Subject = new ClaimsIdentity(),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private string GetUserIdentifier(ClaimsPrincipal? user)
        {
            if (user == null || string.IsNullOrWhiteSpace(_userIdClaimType))
            {
                return string.Empty;
            }

            return user.FindFirst(_userIdClaimType)?.Value ?? string.Empty;
        }
    }
}
