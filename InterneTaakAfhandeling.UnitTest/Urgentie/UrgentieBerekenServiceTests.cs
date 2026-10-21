using InterneTaakAfhandeling.Common.Services.Urgentie;
using InterneTaakAfhandeling.Web.Server.Features.InternetakenOverviews.Shared.Urgentie;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace InterneTaakAfhandeling.UnitTest.Urgentie;

/// <summary>
/// Dekt het acceptatiecriterium van #329 "unittest voor een afwijkende termijn": verifieert
/// dat <see cref="UrgentieBerekenService"/> de geconfigureerde termijn en drempel gebruikt
/// in plaats van de voorheen hardgecodeerde 48/6.
/// </summary>
/// <remarks>
/// De service leest <see cref="DateTimeOffset.UtcNow"/> rechtstreeks (geen clock-abstractie),
/// dus we kiezen <c>contactDatum</c> ver genoeg in het verleden om weekend- en wall-clock-drift
/// irrelevant te maken.
/// </remarks>
[TestClass]
public class UrgentieBerekenServiceTests
{
    [TestMethod]
    public void Bereken_WithAfhandeltermijn24Uur_ReturnsVerlopen_WhenContactIsLangGeleden()
    {
        var service = CreateService(afhandeltermijnUren: 24, bijnaVerlopenDrempelUren: 6);
        // 14 kalenderdagen = minstens 10 werkdagen = ruim voorbij elke termijn ≤ 240 werkuren.
        var contactDatum = DateTimeOffset.UtcNow.AddDays(-14);

        var info = service.Bereken(contactDatum);

        Assert.IsNotNull(info);
        Assert.AreEqual("error", info.Status,
            $"Verwacht verlopen (error); kreeg {info.Status} met label {info.Label}.");
    }

    [TestMethod]
    public void Bereken_WithRuimeTermijn_ReturnsBinnenTermijn_VoorHetzelfdeContactverzoek()
    {
        // Zelfde contactDatum, veel ruimere termijn — het verlopen-geval wordt binnen-termijn.
        var service = CreateService(afhandeltermijnUren: 24 * 30, bijnaVerlopenDrempelUren: 6);
        var contactDatum = DateTimeOffset.UtcNow.AddDays(-14);

        var info = service.Bereken(contactDatum);

        Assert.IsNotNull(info);
        Assert.AreEqual("success", info.Status,
            $"Met een termijn van 30 werkdagen moet een contact van 14 kalenderdagen oud binnen termijn vallen; kreeg {info.Status} met label {info.Label}.");
    }

    [TestMethod]
    public void Bereken_WithDefault48Uur_HeeftAndereStatus_DanMetKorteTermijn()
    {
        var defaults = CreateService(afhandeltermijnUren: 48, bijnaVerlopenDrempelUren: 6);
        var kort = CreateService(afhandeltermijnUren: 1, bijnaVerlopenDrempelUren: 6);
        // Een vers contactverzoek (nu minus 2 uur) valt onder default binnen termijn, met termijn 1u is het verlopen.
        var contactDatum = DateTimeOffset.UtcNow.AddHours(-2);

        var defaultsInfo = defaults.Bereken(contactDatum);
        var kortInfo = kort.Bereken(contactDatum);

        Assert.IsNotNull(defaultsInfo);
        Assert.IsNotNull(kortInfo);
        Assert.AreNotEqual(defaultsInfo.Status, kortInfo.Status,
            "De urgentiestatus moet verschillen zodra de termijn wordt gewijzigd — anders werkt de configuratie niet door.");
    }

    private static UrgentieBerekenService CreateService(double afhandeltermijnUren, double bijnaVerlopenDrempelUren)
    {
        var options = Options.Create(new UrgentieOptions
        {
            AfhandeltermijnUren = afhandeltermijnUren,
            BijnaVerlopenDrempelUren = bijnaVerlopenDrempelUren,
        });
        return new UrgentieBerekenService(options, NullLogger<UrgentieBerekenService>.Instance);
    }
}
