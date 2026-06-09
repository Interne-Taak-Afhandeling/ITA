using System.Net;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace InterneTaakAfhandeling.Common.Test.Services.OpenKlantApi;

public class GetPartijDigitaleAdressenAsyncTests
{
    private readonly Mock<ILogger<OpenKlantApiClient>> _loggerMock = new();

    private OpenKlantApiClient CreateClient(HttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://openklant.test/api/v1/")
        };
        return new OpenKlantApiClient(httpClient, _loggerMock.Object);
    }

    [Fact]
    public async Task GetPartijDigitaleAdressenAsync_ReturnsDigitaleAdressen_WhenPartijHasAddresses()
    {
        // Arrange
        var partijUuid = "e8203b1b-f97a-4d4b-bf69-14a2a7c5e57a";
        var responseJson = """
        {
            "uuid": "e8203b1b-f97a-4d4b-bf69-14a2a7c5e57a",
            "url": "https://openklant.test/api/v1/partijen/e8203b1b-f97a-4d4b-bf69-14a2a7c5e57a",
            "naam": "Jan Jansen",
            "_expand": {
                "digitaleAdressen": [
                    {
                        "uuid": "aaa-111",
                        "url": "https://openklant.test/api/v1/digitaleadressen/aaa-111",
                        "adres": "jan@example.nl",
                        "soortDigitaalAdres": "email",
                        "isStandaardAdres": true,
                        "omschrijving": "Werk e-mail"
                    },
                    {
                        "uuid": "bbb-222",
                        "url": "https://openklant.test/api/v1/digitaleadressen/bbb-222",
                        "adres": "0612345678",
                        "soortDigitaalAdres": "telefoonnummer",
                        "isStandaardAdres": false,
                        "omschrijving": "Mobiel"
                    }
                ]
            }
        }
        """;

        var handler = MockHttpMessageHandler.WithJsonResponse(responseJson);
        var client = CreateClient(handler);

        // Act
        var result = await client.GetPartijDigitaleAdressenAsync(partijUuid);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);

        Assert.Equal("jan@example.nl", result[0].Adres);
        Assert.Equal("email", result[0].SoortDigitaalAdres);
        Assert.True(result[0].IsStandaardAdres);

        Assert.Equal("0612345678", result[1].Adres);
        Assert.Equal("telefoonnummer", result[1].SoortDigitaalAdres);
        Assert.False(result[1].IsStandaardAdres);

        // Verify correct endpoint was called
        Assert.NotNull(handler.LastRequest);
        Assert.Equal(HttpMethod.Get, handler.LastRequest.Method);
        Assert.Equal(
            $"https://openklant.test/api/v1/partijen/{partijUuid}?expand=digitaleAdressen",
            handler.LastRequest.RequestUri?.ToString());
    }

    [Fact]
    public async Task GetPartijDigitaleAdressenAsync_ReturnsEmptyList_WhenPartijHasNoAddresses()
    {
        // Arrange
        var partijUuid = "e8203b1b-f97a-4d4b-bf69-14a2a7c5e57a";
        var responseJson = """
        {
            "uuid": "e8203b1b-f97a-4d4b-bf69-14a2a7c5e57a",
            "url": "https://openklant.test/api/v1/partijen/e8203b1b-f97a-4d4b-bf69-14a2a7c5e57a",
            "naam": "Jan Jansen",
            "_expand": {
                "digitaleAdressen": []
            }
        }
        """;

        var handler = MockHttpMessageHandler.WithJsonResponse(responseJson);
        var client = CreateClient(handler);

        // Act
        var result = await client.GetPartijDigitaleAdressenAsync(partijUuid);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetPartijDigitaleAdressenAsync_ReturnsEmptyListAndLogs_WhenPartijNotFound()
    {
        // Arrange
        var partijUuid = "non-existent-uuid";
        var handler = MockHttpMessageHandler.WithStatusCode(HttpStatusCode.NotFound);
        var client = CreateClient(handler);

        // Act
        var result = await client.GetPartijDigitaleAdressenAsync(partijUuid);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains(partijUuid)),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetPartijDigitaleAdressenAsync_ReturnsEmptyList_WhenExpandFieldMissing()
    {
        // Arrange — response has no _expand field at all
        var partijUuid = "e8203b1b-f97a-4d4b-bf69-14a2a7c5e57a";
        var responseJson = """
        {
            "uuid": "e8203b1b-f97a-4d4b-bf69-14a2a7c5e57a",
            "url": "https://openklant.test/api/v1/partijen/e8203b1b-f97a-4d4b-bf69-14a2a7c5e57a",
            "naam": "Jan Jansen"
        }
        """;

        var handler = MockHttpMessageHandler.WithJsonResponse(responseJson);
        var client = CreateClient(handler);

        // Act
        var result = await client.GetPartijDigitaleAdressenAsync(partijUuid);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
}
