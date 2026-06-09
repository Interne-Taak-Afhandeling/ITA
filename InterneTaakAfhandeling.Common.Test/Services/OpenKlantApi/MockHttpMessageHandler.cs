using System.Net;

namespace InterneTaakAfhandeling.Common.Test.Services.OpenKlantApi;

public class MockHttpMessageHandler(
    Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return handler(request, cancellationToken);
    }

    public static MockHttpMessageHandler WithJsonResponse(string json, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        return new MockHttpMessageHandler((_, _) =>
            Task.FromResult(new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
            }));
    }

    public static MockHttpMessageHandler WithStatusCode(HttpStatusCode statusCode)
    {
        return new MockHttpMessageHandler((_, _) =>
            Task.FromResult(new HttpResponseMessage(statusCode)));
    }
}
