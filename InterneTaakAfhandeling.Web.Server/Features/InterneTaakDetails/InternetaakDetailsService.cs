using InterneTaakAfhandeling.Common.Exceptions;
using InterneTaakAfhandeling.Common.Services.Internetaken;
using InterneTaakAfhandeling.Common.ApiClients.OpenKlantApi.Models;
using InterneTaakAfhandeling.Common.ApiClients.OpenKlantApi;
using InterneTaakAfhandeling.Common.ApiClients.ZakenApi;
using InterneTaakAfhandeling.Web.Server.Authentication;
using InterneTaakAfhandeling.Web.Server.Guards;

namespace InterneTaakAfhandeling.Web.Server.Features.InterneTaakDetails
{
    public interface IInternetaakDetailsService
    {
        Task<Internetaak?> Get(InterneTaakQuery interneTaakQueryParameters);
        Task<Internetaak?> GetByKlantcontactNummer(string klantcontactNummer);
    }

    public class InternetaakDetailsService(
        IOpenKlantApiClient openKlantApiClient,
        IZakenApiClient zakenApiClient,
        IContactverzoekAutorisatieGuardService contactverzoekAutorisatieGuardService,
        ITAUser user,
        PartijIdentificatorConfig partijIdentificatorConfig,
        IInternetaakService internetaakService,
        ILogger<InternetaakDetailsService> logger) : IInternetaakDetailsService
    {
        private readonly IOpenKlantApiClient _openKlantApiClient = openKlantApiClient;
        private readonly IZakenApiClient _zakenApiClient = zakenApiClient;
        private readonly IInternetaakService _internetaakService = internetaakService;
        private readonly ILogger<InternetaakDetailsService> _logger = logger;

        public async Task<Internetaak?> Get(InterneTaakQuery interneTaakQueryParameters)
        {
            var interneTaakQuery = new InterneTaakQuery
            {
                Nummer = interneTaakQueryParameters.Nummer
            };

            var internetaken = await _internetaakService.QueryInterneTakenAsync(interneTaakQuery);


            if (internetaken?.Count != 1)
            {
                return null;
            }

            var interntaak = internetaken.Single();

            await contactverzoekAutorisatieGuardService.GuardAgainstGeenToegangAsync(interntaak, user);

            await EnrichWithZaakAsync(interntaak);
            await EnrichBetrokkenenWithPartijDigitaleAdressenAsync(interntaak);
            await EnrichWithPartijIdentificatieAsync(interntaak);

            return interntaak;
        }

        public async Task<Internetaak?> GetByKlantcontactNummer(string klantcontactNummer)
        {
            var query = new InterneTaakQuery
            {
                Klantcontact__Nummer = klantcontactNummer
            };

            var internetaken = await _internetaakService.QueryInterneTakenAsync(query);

            if (internetaken == null || internetaken.Count == 0)
            {
                return null;
            }

            if (internetaken.Count > 1)
            {
                var uuids = internetaken.Select(t => t.Uuid).ToList();

                _logger.LogWarning(
                    "Meerdere interne taken ({Count}) gevonden voor klantcontact nummer. Kan niet bepalen welke de juiste is. Interne taak UUID's: {Uuids}",
                    internetaken.Count,
                    uuids);

                throw new ConflictException(
                    $"Meerdere interne taken gevonden voor klantcontact met nummer: {klantcontactNummer}. Kan niet bepalen welke de juiste is.");
            }

            var interntaak = internetaken.Single();

            await contactverzoekAutorisatieGuardService.GuardAgainstGeenToegangAsync(interntaak, user);

            await EnrichWithZaakAsync(interntaak);
            await EnrichBetrokkenenWithPartijDigitaleAdressenAsync(interntaak);
            await EnrichWithPartijIdentificatieAsync(interntaak);

            return interntaak;
        }
        
        private async Task EnrichWithPartijIdentificatieAsync(Internetaak internetaak)
        {
            if (!partijIdentificatorConfig.Tonen)
            {
                return;
            }

            var partijUuids = internetaak.AanleidinggevendKlantcontact.Expand?.HadBetrokkenen?
                .Select(b => b.Expand?.WasPartij?.Uuid)
                .OfType<string>()
                .Where(uuid => uuid.Length > 0)
                .Distinct()
                .ToList() ?? [];

            if (partijUuids.Count == 0)
            {
                return;
            }

            var fetchedIdentificatoren = (await Task.WhenAll(partijUuids.Select(GetPartijIdentificatorenAsync)))
                .OfType<List<PartijIdentificator>>()
                .ToList();

            if (fetchedIdentificatoren.Count == 0)
            {
                return;
            }

            var identificatoren = fetchedIdentificatoren.SelectMany(i => i).ToList();

            List<string> Waarden(string codeSoortObjectId) => [.. identificatoren
                .Where(i => i.CodeSoortObjectId == codeSoortObjectId)
                .Select(i => i.ObjectId)
                .OfType<string>()
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()];

            internetaak.PartijIdentificatie = new PartijIdentificatie
            {
                Bsn = Waarden(KnownPartijIdentificatorSoorten.Bsn),
                KvkNummer = Waarden(KnownPartijIdentificatorSoorten.KvkNummer),
                Vestigingsnummer = Waarden(KnownPartijIdentificatorSoorten.Vestigingsnummer)
            };
        }

        // Returns null when the lookup fails, so one failing partij does not hide the identificatoren of the others
        private async Task<List<PartijIdentificator>?> GetPartijIdentificatorenAsync(string partijUuid)
        {
            try
            {
                var partij = await _openKlantApiClient.GetAsync<PartijMetIdentificatoren>($"partijen/{partijUuid}");

                return partij?.PartijIdentificatoren?
                    .Select(i => i.PartijIdentificator)
                    .OfType<PartijIdentificator>()
                    .ToList() ?? [];
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Ophalen partij-identificatoren gefaald voor partij {PartijUuid}", partijUuid);
                return null;
            }
        }



        private static string? GetZaakOnderwerpObject(Klantcontact klantContact)
        {
            return klantContact
                .Expand?
                .GingOverOnderwerpobjecten?
                .Where(x =>
                    x.Onderwerpobjectidentificator?.CodeObjecttype == KnownZgwIdentificators.OnderwerpobjectidentificatorCodeObjecttype &&
                    x.Onderwerpobjectidentificator?.CodeRegister == KnownZgwIdentificators.OnderwerpobjectidentificatorCodeRegister
                )
                .FirstOrDefault()?
                .Onderwerpobjectidentificator?
                .ObjectId;
        }


        private async Task EnrichWithZaakAsync(Internetaak? interntaak)
        {
            if (interntaak?.AanleidinggevendKlantcontact != null)
            {
                var onderwerpObjectId = GetZaakOnderwerpObject(interntaak.AanleidinggevendKlantcontact);

                if (!string.IsNullOrEmpty(onderwerpObjectId))
                {
                    interntaak.Zaak = await _zakenApiClient.GetZaakAsync(onderwerpObjectId);
                }
            }
        }

        private async Task EnrichBetrokkenenWithPartijDigitaleAdressenAsync(Internetaak? internetaak)
        {
            var betrokkenen = internetaak?.AanleidinggevendKlantcontact?.Expand?.HadBetrokkenen;
            if (betrokkenen == null) return;

            var partijAdressenCache = new Dictionary<string, List<DigitaleAdres>>();

            foreach (var betrokkene in betrokkenen)
            {
                if (betrokkene.Expand?.DigitaleAdressen != null && betrokkene.Expand.DigitaleAdressen.Count > 0)
                    continue;

                var partijUuid = betrokkene.Expand?.WasPartij?.Uuid;
                if (string.IsNullOrEmpty(partijUuid))
                    continue;

                try
                {
                    if (!partijAdressenCache.TryGetValue(partijUuid, out var partijAdressen))
                    {
                        _logger.LogInformation("Fallback: ophalen digitale adressen van partij {PartijUuid} voor betrokkene {BetrokkeneUuid}",
                            partijUuid, betrokkene.Uuid);

                        var partij = await _openKlantApiClient.GetAsync<Partij>($"partijen/{partijUuid}?expand=digitaleAdressen");
                        partijAdressen = partij?.Expand?.DigitaleAdressen ?? [];
                        partijAdressenCache[partijUuid] = partijAdressen;
                    }

                    betrokkene.Expand ??= new BetrokkeneExpand();
                    betrokkene.Expand.DigitaleAdressen = partijAdressen;
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Fallback partij-lookup gefaald voor partij {PartijUuid}: {Message}",
                        partijUuid, ex.Message);
                }
            }
        }

        private record PartijMetIdentificatoren(List<PartijIdentificatorItem>? PartijIdentificatoren);
        private record PartijIdentificatorItem(PartijIdentificator? PartijIdentificator);
        private record PartijIdentificator(string? CodeSoortObjectId, string? ObjectId);
    }
}
