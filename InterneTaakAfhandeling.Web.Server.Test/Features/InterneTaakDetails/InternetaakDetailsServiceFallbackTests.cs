using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using InterneTaakAfhandeling.Common.Services.ZakenApi;
using InterneTaakAfhandeling.Web.Server.Features.InterneTaak;
using Microsoft.Extensions.Logging;
using Moq;

namespace InterneTaakAfhandeling.Web.Server.Test.Features.InterneTaakDetails;

public class InternetaakDetailsServiceFallbackTests
{
    private readonly Mock<IOpenKlantApiClient> _openKlantApiClientMock = new();
    private readonly Mock<IZakenApiClient> _zakenApiClientMock = new();
    private readonly Mock<IContactmomentenService> _contactmomentenServiceMock = new();
    private readonly Mock<ILogger<InternetaakDetailsService>> _loggerMock = new();

    private InternetaakDetailsService CreateService()
    {
        return new InternetaakDetailsService(
            _openKlantApiClientMock.Object,
            _zakenApiClientMock.Object,
            _contactmomentenServiceMock.Object,
            _loggerMock.Object);
    }

    private void SetupQueryReturnsInternetaak(Internetaak internetaak)
    {
        _openKlantApiClientMock
            .Setup(x => x.QueryInterneTakenAsync(It.IsAny<InterneTaakQuery>()))
            .ReturnsAsync([internetaak]);

        _contactmomentenServiceMock
            .Setup(x => x.GetZaakOnderwerpObject(It.IsAny<Klantcontact>()))
            .Returns((string?)null);
    }

    [Fact]
    public async Task Get_EnrichesBetrokkeneWithPartijAdressen_WhenBetrokkeneHasNoOwnAdressen()
    {
        // Arrange — betrokkene without own addresses, linked to partij with addresses
        var partijUuid = "partij-uuid-123";
        var partijAdressen = new List<DigitaleAdres>
        {
            new() { Uuid = "da-1", Url = "http://test/da/1", Adres = "jan@example.nl", SoortDigitaalAdres = "email", IsStandaardAdres = true, Omschrijving = "Werk" },
            new() { Uuid = "da-2", Url = "http://test/da/2", Adres = "0612345678", SoortDigitaalAdres = "telefoonnummer", IsStandaardAdres = false, Omschrijving = "Mobiel" }
        };

        var internetaak = CreateInternetaakWithBetrokkene(
            digitaleAdressen: [],
            wasPartijUuid: partijUuid);

        SetupQueryReturnsInternetaak(internetaak);

        _openKlantApiClientMock
            .Setup(x => x.GetPartijDigitaleAdressenAsync(partijUuid))
            .ReturnsAsync(partijAdressen);

        var service = CreateService();

        // Act
        var result = await service.Get(new InterneTaakQuery { Nummer = "123" });

        // Assert
        var betrokkene = result!.AanleidinggevendKlantcontact.Expand!.HadBetrokkenen!.First();
        Assert.Equal(2, betrokkene.Expand!.DigitaleAdressen!.Count);
        Assert.Equal("jan@example.nl", betrokkene.Expand.DigitaleAdressen[0].Adres);
        Assert.Equal("0612345678", betrokkene.Expand.DigitaleAdressen[1].Adres);
    }

    [Fact]
    public async Task Get_KeepsOwnAdressen_WhenBetrokkeneHasOwnAdressen()
    {
        // Arrange — betrokkene with own addresses + linked partij
        var ownAdressen = new List<DigitaleAdres>
        {
            new() { Uuid = "own-1", Url = "http://test/da/own", Adres = "eigen@example.nl", SoortDigitaalAdres = "email", IsStandaardAdres = true, Omschrijving = "Eigen" }
        };

        var internetaak = CreateInternetaakWithBetrokkene(
            digitaleAdressen: ownAdressen,
            wasPartijUuid: "partij-uuid-456");

        SetupQueryReturnsInternetaak(internetaak);

        var service = CreateService();

        // Act
        var result = await service.Get(new InterneTaakQuery { Nummer = "123" });

        // Assert — partij should NOT be called
        _openKlantApiClientMock.Verify(
            x => x.GetPartijDigitaleAdressenAsync(It.IsAny<string>()), Times.Never);

        var betrokkene = result!.AanleidinggevendKlantcontact.Expand!.HadBetrokkenen!.First();
        Assert.Single(betrokkene.Expand!.DigitaleAdressen!);
        Assert.Equal("eigen@example.nl", betrokkene.Expand.DigitaleAdressen![0].Adres);
    }

    [Fact]
    public async Task Get_DoesNotCallPartij_WhenBetrokkeneHasNoPartijIdentifier()
    {
        // Arrange — betrokkene without addresses and without partij link
        var internetaak = CreateInternetaakWithBetrokkene(
            digitaleAdressen: [],
            wasPartijUuid: null);

        SetupQueryReturnsInternetaak(internetaak);

        var service = CreateService();

        // Act
        var result = await service.Get(new InterneTaakQuery { Nummer = "123" });

        // Assert
        _openKlantApiClientMock.Verify(
            x => x.GetPartijDigitaleAdressenAsync(It.IsAny<string>()), Times.Never);

        var betrokkene = result!.AanleidinggevendKlantcontact.Expand!.HadBetrokkenen!.First();
        Assert.Empty(betrokkene.Expand!.DigitaleAdressen!);
    }

    [Fact]
    public async Task Get_ReturnsEmptyAdressen_WhenPartijHasNoAdressen()
    {
        // Arrange — betrokkene without addresses, partij also has no addresses
        var partijUuid = "partij-uuid-789";
        var internetaak = CreateInternetaakWithBetrokkene(
            digitaleAdressen: [],
            wasPartijUuid: partijUuid);

        SetupQueryReturnsInternetaak(internetaak);

        _openKlantApiClientMock
            .Setup(x => x.GetPartijDigitaleAdressenAsync(partijUuid))
            .ReturnsAsync([]);

        var service = CreateService();

        // Act
        var result = await service.Get(new InterneTaakQuery { Nummer = "123" });

        // Assert
        var betrokkene = result!.AanleidinggevendKlantcontact.Expand!.HadBetrokkenen!.First();
        Assert.Empty(betrokkene.Expand!.DigitaleAdressen!);
    }

    [Fact]
    public async Task Get_HandlesPartijLookupFailure_Gracefully()
    {
        // Arrange — partij lookup throws exception
        var partijUuid = "partij-uuid-fail";
        var internetaak = CreateInternetaakWithBetrokkene(
            digitaleAdressen: [],
            wasPartijUuid: partijUuid);

        SetupQueryReturnsInternetaak(internetaak);

        _openKlantApiClientMock
            .Setup(x => x.GetPartijDigitaleAdressenAsync(partijUuid))
            .ThrowsAsync(new HttpRequestException("API not reachable"));

        var service = CreateService();

        // Act
        var result = await service.Get(new InterneTaakQuery { Nummer = "123" });

        // Assert — should not throw, contact section remains empty
        Assert.NotNull(result);
        var betrokkene = result!.AanleidinggevendKlantcontact.Expand!.HadBetrokkenen!.First();
        Assert.Empty(betrokkene.Expand!.DigitaleAdressen!);
    }

    private static Internetaak CreateInternetaakWithBetrokkene(
        List<DigitaleAdres> digitaleAdressen,
        string? wasPartijUuid)
    {
        var betrokkene = new Betrokkene
        {
            Uuid = "betrokkene-uuid",
            Url = "http://test/betrokkenen/1",
            DigitaleAdressen = [],
            Initiator = true,
            Expand = new BetrokkeneExpand
            {
                DigitaleAdressen = digitaleAdressen,
                WasPartij = wasPartijUuid != null
                    ? new Partij { Uuid = wasPartijUuid, Url = $"http://test/partijen/{wasPartijUuid}" }
                    : null
            }
        };

        return new Internetaak
        {
            Uuid = "internetaak-uuid",
            Url = "http://test/internetaken/1",
            AanleidinggevendKlantcontact = new Klantcontact
            {
                Uuid = Guid.NewGuid(),
                Url = "http://test/klantcontacten/1",
                Expand = new Expand
                {
                    HadBetrokkenen = [betrokkene]
                }
            }
        };
    }
}
