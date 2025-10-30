using System.Net.Http.Headers;

namespace InterneTaakAfhandeling.EndToEndTest.Infrastructure.ApiClients
{
    public abstract class ApiClientBase
    {
        protected HttpClient HttpClient { get; }
        protected string Token { get; }
        protected string BaseUrl { get; }

        protected ApiClientBase(string baseUrl, string token)
        {
            HttpClient = new HttpClient();
            BaseUrl = baseUrl;
            Token = token;
        }

        protected HttpRequestMessage CreateRequest(HttpMethod method, string endpoint, HttpContent? content = null)
        {
            var url = $"{BaseUrl}{endpoint}";
            var request = new HttpRequestMessage(method, url);

            if (content != null)
            {
                request.Content = content;
            }

            SetAuthorizationHeader(request);
            return request;
        }

        private void SetAuthorizationHeader(HttpRequestMessage request)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Token", Token);
        }
    }
}
