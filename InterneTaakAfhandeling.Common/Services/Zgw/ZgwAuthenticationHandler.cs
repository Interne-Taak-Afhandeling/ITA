using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;

namespace InterneTaakAfhandeling.Common.Services.Zgw
{
    /// <summary>
    /// DelegatingHandler that uses ZgwTokenProvider to generate JWT tokens per request.
    /// This is required for the podiumd-adapter which passes the user_id to the e-Suite.
    /// Also works with OpenZaak directly (which ignores the extra claims).
    /// Can be used for any ZGW API that needs to go through the adapter (Zaken, OpenKlant, etc.)
    /// </summary>
    public class ZgwAuthenticationHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ZgwTokenProvider _tokenProvider;

        public ZgwAuthenticationHandler(
            IHttpContextAccessor httpContextAccessor,
            ZgwTokenProvider tokenProvider)
        {
            _httpContextAccessor = httpContextAccessor;
            _tokenProvider = tokenProvider;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var token = _tokenProvider.GenerateToken(user);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
