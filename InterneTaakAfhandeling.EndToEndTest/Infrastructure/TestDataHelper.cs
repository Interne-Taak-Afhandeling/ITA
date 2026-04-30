using InterneTaakAfhandeling.Common.Extensions;
using InterneTaakAfhandeling.Common.Services;
using InterneTaakAfhandeling.Common.Services.ObjectApi;
using InterneTaakAfhandeling.Common.Services.ObjectApi.Models;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using InterneTaakAfhandeling.Common.Services.ZakenApi;
using InterneTaakAfhandeling.Common.Services.Zgw;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

namespace InterneTaakAfhandeling.EndToEndTest.Infrastructure
{
    public class TestDataHelper
    {
        private OpenKlantApiClient OpenKlantApiClient { get; }
        private ObjectApiClient ObjectApiClient { get; }
        private ZakenApiClient ZakenApiClient { get; }
        private string Username { get; }
        private ILogger<TestDataHelper> Logger { get; }

        public TestDataHelper(
            string openKlantBaseUrl, 
            string openKlantApiKey, 
            string objectenApiBaseUrl, 
            string objectenApiKey,
            string zakenApiBaseUrl, 
            string zakenApiKey, 
            string zakenApiClientId,
            IOptions<LogboekOptions> l,
            IOptions<AfdelingOptions> a,
            IOptions<GroepOptions> g,
            string username)
        {
            Username = username;

            var loggerFactory = new LoggerFactory();
            Logger = loggerFactory.CreateLogger<TestDataHelper>();

            OpenKlantApiClient = CreateOpenKlantApiClient(openKlantBaseUrl, openKlantApiKey, loggerFactory);
            ObjectApiClient = CreateObjectApiClient(objectenApiBaseUrl, objectenApiKey, loggerFactory, l, a, g);
            ZakenApiClient = CreateZakenApiClient(zakenApiBaseUrl, zakenApiKey, zakenApiClientId, loggerFactory);
        }

        // Client Creation

        private static OpenKlantApiClient CreateOpenKlantApiClient(
            string baseUrl, 
            string apiKey, 
            ILoggerFactory loggerFactory)
        {
            var httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", apiKey);
            return new OpenKlantApiClient(httpClient, loggerFactory.CreateLogger<OpenKlantApiClient>());
        }

        private static ObjectApiClient CreateObjectApiClient(
            string baseUrl, 
            string apiKey, 
            ILoggerFactory loggerFactory,
            IOptions<LogboekOptions> l,
            IOptions<AfdelingOptions> a,
            IOptions<GroepOptions> g)
        {
            var httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", apiKey);
            httpClient.DefaultRequestHeaders.Add("Content-Crs", "EPSG:4326");
            return new ObjectApiClient(httpClient, loggerFactory.CreateLogger<ObjectApiClient>(), l, a, g);
        }

        private static ZakenApiClient CreateZakenApiClient(
            string baseUrl, 
            string apiKey, 
            string clientId, 
            ILoggerFactory loggerFactory)
        {
            var httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
            var provider = new ZgwTokenProvider(apiKey, clientId);
            var token = provider.GenerateToken(null);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            httpClient.DefaultRequestHeaders.Add("Accept-Crs", "EPSG:4326");
            return new ZakenApiClient(httpClient, loggerFactory.CreateLogger<ZakenApiClient>());
        }

  

    //  API

        public async Task<Guid> CreateContactverzoek(
            string onderwerp, 
            bool attachZaak = true,
            string? internetaakNummer = null)
        {
            var contactverzoekNummer = internetaakNummer ?? (attachZaak
                ? TestDataConstants.ContactverzoekNummers.WithZaak
                : TestDataConstants.ContactverzoekNummers.WithoutZaak);

            var contactmoment = await GetOrCreateContactmoment(
                onderwerp, 
                "This is a test contact request created during an end-to-end test run.");

            var submitterActor = await GetOrCreateSubmitterActor();
            
            await ConnectActorToContactmoment(submitterActor, contactmoment.Uuid);

            var afdelingActor = await GetOrCreateAfdelingActor("Burgerzaken_ibz");
            
            var medewerkerActor = await GetOrCreateMedewerkerActor("icatt-integratie-test@icatt.nl");

            await CreateInternetaakIfNotExists(
                contactverzoekNummer,
                contactmoment.Uuid,
                new List<Guid> 
                { 
                    Guid.Parse(medewerkerActor.Uuid), 
                    Guid.Parse(afdelingActor.Uuid) 
                },
                isExplicitNummer: internetaakNummer != null);

            if (attachZaak)
            {
                await AttachZaakToContactmomentAsync(contactmoment.Uuid, TestDataConstants.Zaken.TestZaakIdentificatie);
            }

            return contactmoment.Uuid;
        }

        public async Task<Guid> CreateContactverzoekWithAfdelingMedewerkerAndPartij(
            string onderwerp,
            string bsn = TestDataConstants.Partijen.TestBsn,
            bool attachZaak = false)
        {
            await CleanupExistingContactmomenten(onderwerp);

            var partij = await GetPartijByBsn(bsn);
            var contactmoment = await CreateContactmomentWithPartij(
                onderwerp, 
                "This is a test contact request created during an end-to-end test run.",
                partij);

            var submitterActor = await GetOrCreateSubmitterActor();
            await ConnectActorToContactmoment(submitterActor, contactmoment.Uuid);

            var afdelingActor = await GetOrCreateAfdelingActor("Burgerzaken_ibz");
            var medewerkerActor = await GetOrCreateMedewerkerActor("icatt-integratie-test@icatt.nl");

            await CreateInternetaakIfNotExists(
                TestDataConstants.ContactverzoekNummers.WithPartij,
                contactmoment.Uuid,
                new List<Guid> 
                { 
                    Guid.Parse(medewerkerActor.Uuid), 
                    Guid.Parse(afdelingActor.Uuid) 
                },
                isExplicitNummer: true);

            if (attachZaak)
            {
                await AttachZaakToContactmomentAsync(contactmoment.Uuid, TestDataConstants.Zaken.TestZaakIdentificatie);
            }

            return contactmoment.Uuid;
        }

        public async Task<string?> GetZaakIdentificatieFromContactverzoek(Guid klantcontactUuid)
        {
            try
            {
                var contactmoment = await OpenKlantApiClient.GetKlantcontactAsync(klantcontactUuid);

                if (contactmoment?.GingOverOnderwerpobjecten == null || 
                    contactmoment.GingOverOnderwerpobjecten.Count == 0)
                {
                    return null;
                }

                foreach (var onderwerpobjectRef in contactmoment.GingOverOnderwerpobjecten)
                {
                    var zaakIdentificatie = await TryGetZaakIdentificatieFromOnderwerpobject(onderwerpobjectRef);
                    if (zaakIdentificatie != null)
                    {
                        return zaakIdentificatie;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Failed to get zaak identificatie from klantcontact {KlantcontactUuid}", klantcontactUuid);
                return null;
            }
        }

        public async Task DeleteContactverzoekAsync(string klantcontactUuid)
        {
            var uuid = Guid.Parse(klantcontactUuid);
            try
            {
                await OpenKlantApiClient.DeleteKlantcontactAsync(uuid);
            }
            catch (Exception)
            {
                // Silent catch for deletion errors
            }
        }

        public async Task<Internetaak> GetInternetaakByIdAsync(Guid uuid)
        {
            return await OpenKlantApiClient.GetInternetaakByIdAsync(uuid);
        }

        public async Task<Guid?> GetInternetaakUuidFromContactmomentAsync(Guid contactmomentUuid)
        {
            var contactmoment = await OpenKlantApiClient.GetKlantcontactAsync(contactmomentUuid);
            var internetaak = contactmoment?.Expand?.LeiddeTotInterneTaken?.FirstOrDefault();
            
            if (internetaak?.Uuid == null)
            {
                return null;
            }

            if (Guid.TryParse(internetaak.Uuid, out var parsedGuid))
            {
                return parsedGuid;
            }

            Logger.LogWarning("Invalid UUID string received for internetaak: '{UuidString}' from contactmoment {ContactmomentUuid}", 
                internetaak.Uuid, contactmomentUuid);
            return null;
        }

  

        // Contactmoment Operations

        private async Task<Klantcontact> GetOrCreateContactmoment(string onderwerp, string inhoud)
        {
            var existing = await QueryContactmoment(onderwerp);
            return existing ?? await CreateContactmoment(onderwerp, inhoud, klantnaam: null)
                ?? throw new Exception("Failed to create contactmoment for testing.");
        }

        private async Task<Klantcontact> CreateContactmomentWithPartij(
            string onderwerp, 
            string inhoud, 
            Partij partij)
        {
            var contactmoment = await CreateContactmoment(onderwerp, inhoud, partij.Naam);
            await AttachPartijToContactmoment(Guid.Parse(partij.Uuid), contactmoment.Uuid);
            return contactmoment;
        }

        private Task<Klantcontact> CreateContactmoment(string onderwerp, string inhoud, string? klantnaam)
        {
            return OpenKlantApiClient.CreateKlantcontactAsync(new KlantcontactRequest
            {
                IndicatieContactGelukt = true,
                Onderwerp = onderwerp,
                Inhoud = inhoud,
                Kanaal = "e-mail",
                PlaatsgevondenOp = DateTime.UtcNow,
                Taal = "nl",
                Vertrouwelijk = false,
                Klantnaam = klantnaam
            });
        }

        private async Task<Klantcontact?> QueryContactmoment(string onderwerp)
        {
            var contactmomenten = await OpenKlantApiClient.QueryKlantcontactAsync(
                new KlantcontactQuery { Onderwerp = onderwerp });

            // If multiple exist (from failed previous runs), clean them all up
            if (contactmomenten.Count > 1)
            {
                try
                {
                    await CleanupExistingContactmomenten(onderwerp);
                }
                catch
                {
                    // Ignore cleanup failures
                }
                return null; // Force creation of a fresh one
            }
    
            return contactmomenten.FirstOrDefault();
        }

        private async Task CleanupExistingContactmomenten(string onderwerp)
        {
            var contactmomenten = await OpenKlantApiClient.QueryKlantcontactAsync(
                new KlantcontactQuery { Onderwerp = onderwerp });

            foreach (var existing in contactmomenten)
            {
                await OpenKlantApiClient.DeleteKlantcontactAsync(existing.Uuid);
            }

            var remaining = await OpenKlantApiClient.QueryKlantcontactAsync(
                new KlantcontactQuery { Onderwerp = onderwerp });
            
            if (remaining.Count > 0)
            {
                throw new InvalidOperationException($"Failed to cleanup contactmomenten. Still found {remaining.Count} contactmomenten after deletion.");
            }
        }

        private async Task ConnectActorToContactmoment(Actor actor, Guid contactmomentUuid)
        {
            var contactmoment = await OpenKlantApiClient.GetKlantcontactAsync(contactmomentUuid);

            if (contactmoment.HadBetrokkenActoren.Any(a => a.Uuid == actor.Uuid))
            {
                return; // Already connected
            }

            await OpenKlantApiClient.CreateActorKlantcontactAsync(new ActorKlantcontactRequest
            {
                Actor = new ActorReference { Uuid = actor.Uuid },
                Klantcontact = new KlantcontactReference { Uuid = contactmomentUuid }
            });
        }

        //  Actor Operations

        private Task<Actor> GetOrCreateSubmitterActor()
        {
            return GetOrCreateActor(
                query: new ActorQuery
                {
                    ActoridentificatorCodeObjecttype = KnownMedewerkerIdentificators.EmailFromEntraId.CodeObjecttype,
                    ActoridentificatorCodeRegister = KnownMedewerkerIdentificators.EmailFromEntraId.CodeRegister,
                    ActoridentificatorCodeSoortObjectId = KnownMedewerkerIdentificators.EmailFromEntraId.CodeSoortObjectId,
                    IndicatieActief = true,
                    SoortActor = SoortActor.medewerker,
                    ActoridentificatorObjectId = Username
                },
                request: new ActorRequest
                {
                    Naam = "E2E test contactverzoek creator",
                    SoortActor = SoortActor.medewerker,
                    IndicatieActief = true,
                    Actoridentificator = new Actoridentificator
                    {
                        ObjectId = Username,
                        CodeObjecttype = KnownMedewerkerIdentificators.EmailFromEntraId.CodeObjecttype,
                        CodeRegister = KnownMedewerkerIdentificators.EmailFromEntraId.CodeRegister,
                        CodeSoortObjectId = KnownMedewerkerIdentificators.EmailFromEntraId.CodeSoortObjectId
                    }
                });
        }

        private async Task<Actor> GetOrCreateAfdelingActor(string afdelingName)
{
    var afdeling = await GetAfdelingByName(afdelingName);

    return await GetOrCreateActor(
        query: new ActorQuery
        {
            ActoridentificatorCodeObjecttype = KnownAfdelingIdentificators.ObjectRegisterId.CodeObjecttype,
            ActoridentificatorCodeRegister = KnownAfdelingIdentificators.ObjectRegisterId.CodeRegister,
            ActoridentificatorCodeSoortObjectId = KnownMedewerkerIdentificators.ObjectRegisterId.CodeSoortObjectId,
            IndicatieActief = true,
            SoortActor = SoortActor.organisatorische_eenheid,
            ActoridentificatorObjectId = afdeling.Record.Data.Identificatie
        },
        request: new ActorRequest
        {
            Naam = "e2e afdeling",
            SoortActor = SoortActor.organisatorische_eenheid,
            IndicatieActief = true,
            Actoridentificator = new Actoridentificator
            {
                ObjectId = afdeling.Record.Data.Identificatie,
                CodeObjecttype = KnownAfdelingIdentificators.ObjectRegisterId.CodeObjecttype,
                CodeRegister = KnownAfdelingIdentificators.ObjectRegisterId.CodeRegister,
                CodeSoortObjectId = KnownMedewerkerIdentificators.ObjectRegisterId.CodeSoortObjectId
            }
        });
}

        private async Task<Actor> GetOrCreateMedewerkerActor(string email)
        {
            var medewerker = await GetMedewerkerByEmail(email);

            return await GetOrCreateActor(
                query: new ActorQuery
                {
                    ActoridentificatorCodeObjecttype = KnownMedewerkerIdentificators.ObjectRegisterId.CodeObjecttype,
                    ActoridentificatorCodeRegister = KnownMedewerkerIdentificators.ObjectRegisterId.CodeRegister,
                    ActoridentificatorCodeSoortObjectId = KnownMedewerkerIdentificators.ObjectRegisterId.CodeSoortObjectId,
                    IndicatieActief = true,
                    SoortActor = SoortActor.medewerker,
                    ActoridentificatorObjectId = medewerker.Identificatie
                },
                request: new ActorRequest
                {
                    Naam = "ICATT Integratietest",
                    SoortActor = SoortActor.medewerker,
                    IndicatieActief = true,
                    Actoridentificator = new Actoridentificator
                    {
                        ObjectId = medewerker.Identificatie!,
                        CodeObjecttype = KnownMedewerkerIdentificators.ObjectRegisterId.CodeObjecttype,
                        CodeRegister = KnownMedewerkerIdentificators.ObjectRegisterId.CodeRegister,
                        CodeSoortObjectId = KnownMedewerkerIdentificators.ObjectRegisterId.CodeSoortObjectId
                    }
                });
        }

        private async Task<Actor> GetOrCreateActor(ActorQuery query, ActorRequest request)
        {
            var actor = await OpenKlantApiClient.QueryActorAsync(query);
            return actor ?? await OpenKlantApiClient.CreateActorAsync(request);
        }

        private async Task<ObjectResult<Afdeling>> GetAfdelingByName(string name)
        {
            var afdelingen = await ObjectApiClient.FindAfdelingen(name);
            if (afdelingen.Results.Count != 1)
            {
                throw new InvalidOperationException($"Expected exactly one afdeling with name '{name}', but found {afdelingen.Results.Count}.");
            }
    
            return afdelingen.Results.First();
        }

        private async Task<MedewerkerObjectData> GetMedewerkerByEmail(string email)
        {
            var medewerkers = await ObjectApiClient.GetMedewerkersByIdentificatie(email);
            if (medewerkers.Count != 1)
            {
                 throw new InvalidOperationException($"Expected exactly one medewerker with identificatie '{email}', but found {medewerkers.Count}.");
            }
    
            return medewerkers.First();
        }

//  Partij Operations

        private async Task<Partij> GetPartijByBsn(string bsn)
        {
            var partijen = await OpenKlantApiClient.GetPartijenByBsnAsync(bsn);
            var partij = partijen.FirstOrDefault();
            
            if (partij == null)
            {
                throw new Exception($"Partij with BSN {bsn} not found");
            }

            return partij;
        }

        private Task<Betrokkene> AttachPartijToContactmoment(Guid partijUuid, Guid contactmomentUuid)
        {
            return OpenKlantApiClient.CreateBetrokkeneAsync(new BetrokkeneRequest
            {
                WasPartij = new PartijReference { Uuid = partijUuid },
                HadKlantcontact = new KlantcontactReference { Uuid = contactmomentUuid },
                Rol = "klant",
                Initiator = true,
                Contactnaam = new Contactnaam
                {
                    Voorletters = TestDataConstants.Partijen.Contactnaam.Voorletters,
                    Voornaam = TestDataConstants.Partijen.Contactnaam.Voornaam,
                    Achternaam = TestDataConstants.Partijen.Contactnaam.Achternaam,
                    VoorvoegselAchternaam = TestDataConstants.Partijen.Contactnaam.VoorvoegselAchternaam
                }
            });
        }

    //   Internetaak Operations

        private static string GenerateUniqueInternetaakNummer()
        {
            var millisPart = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() % 100_000_000;
            var randomPart = Random.Shared.Next(0, 100);
            return $"{millisPart:00000000}{randomPart:00}";
        }

        private async Task CreateInternetaakIfNotExists(
            string nummer,
            Guid contactmomentUuid,
            List<Guid> actorUuids,
            bool isExplicitNummer = false)
        {
            // First check if internetaak with this nummer already exists (idempotent behavior)
            var existing = await OpenKlantApiClient.QueryInterneTakenAsync(new InterneTaakQuery
            {
                Nummer = nummer
            });

            if (existing.Count > 1)
            {
                throw new InvalidOperationException($"Found {existing.Count} internetaken with nummer '{nummer}', expected at most 1.");
            }

            if (existing.Count > 0)
            {
                // Already exists - idempotent success
                Logger.LogInformation("Internetaak with nummer '{Nummer}' already exists, skipping creation", nummer);
                return;
            }

            // Doesn't exist, try to create it
            var currentNummer = nummer;
            Exception? lastConflictException = null;

            for (var attempt = 1; attempt <= 3; attempt++)
            {
                try
                {
                    await OpenKlantApiClient.CreateInterneTaak(new InternetaakPostRequest
                    {
                        AanleidinggevendKlantcontact = new UuidObject { Uuid = contactmomentUuid },
                        GevraagdeHandeling = "terugbellen svp",
                        Nummer = currentNummer,
                        Status = KnownInternetaakStatussen.TeVerwerken,
                        ToegewezenAanActoren = actorUuids.Select(id => new UuidObject { Uuid = id }).ToList(),
                        Toelichting = "Test contactverzoek from ITA E2E test"
                    });

                    Logger.LogInformation("Successfully created internetaak with nummer '{Nummer}'", currentNummer);
                    return; // Success
                }
                catch (HttpRequestException ex) when (ex.Message.Contains("409") || ex.Message.Contains("conflict"))
                {
                    lastConflictException = ex;

                    if (isExplicitNummer)
                    {
                        // Fail fast for explicit nummers - don't mutate them
                        throw new InvalidOperationException(
                            $"Cannot create internetaak with explicit nummer '{nummer}' because it already exists. " +
                            "This breaks test contracts that expect this exact nummer for navigation/verification.", 
                            ex);
                    }

                    // Only auto-generate new nummers when no explicit nummer was provided
                    Logger.LogWarning("Internetaak nummer '{Nummer}' already exists, attempting with different nummer (attempt {Attempt}/3)", 
                        currentNummer, attempt);
                    
                    if (attempt < 3)
                    {
                        currentNummer = GenerateUniqueInternetaakNummer();
                        await Task.Delay(50);
                    }
                }
                catch (Exception ex)
                {
                    // Auth, connection, server errors, etc. - don't retry, rethrow immediately
                    Logger.LogError(ex, "Failed to create internetaak due to non-retryable error: {Message}", ex.Message);
                    throw;
                }
            }

            // If we get here, we've exhausted retries for conflict errors (only for auto-generated nummers)
            throw new InvalidOperationException(
                $"Failed to create internetaak after {3} attempts due to nummer conflicts. Last conflict was with nummer '{currentNummer}'.", 
                lastConflictException);
        }

        // Zaak Operations

        private async Task AttachZaakToContactmomentAsync(Guid klantcontactUuid, string zaakIdentificatie)
        {
            var zaak = await ZakenApiClient.GetZaakByIdentificatieAsync(zaakIdentificatie);
            if (zaak == null)
            {
                return;
            }

            if (await IsZaakAlreadyConnected(klantcontactUuid, zaak.Uuid))
            {
                return;
            }

            await CreateOnderwerpobjectForZaak(klantcontactUuid, zaak.Uuid);
        }

        private async Task<bool> IsZaakAlreadyConnected(Guid klantcontactUuid, string zaakUuid)
        {
            var contactmoment = await OpenKlantApiClient.GetKlantcontactAsync(klantcontactUuid);

            if (contactmoment.GingOverOnderwerpobjecten?.Count == 0)
            {
                return false;
            }

            foreach (var onderwerpobjectRef in contactmoment.GingOverOnderwerpobjecten)
            {
                if (onderwerpobjectRef?.Uuid == null) continue;

                var onderwerpobject = await OpenKlantApiClient.GetOnderwerpobjectAsync(onderwerpobjectRef.Uuid.Value);

                if (IsZaakOnderwerpobject(onderwerpobject, zaakUuid))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsZaakOnderwerpobject(Onderwerpobject? onderwerpobject, string zaakUuid)
        {
            return onderwerpobject?.Onderwerpobjectidentificator != null &&
                   onderwerpobject.Onderwerpobjectidentificator.CodeObjecttype == "zgw-Zaak" &&
                   onderwerpobject.Onderwerpobjectidentificator.CodeRegister == "openzaak" &&
                   onderwerpobject.Onderwerpobjectidentificator.ObjectId == zaakUuid;
        }

        private Task CreateOnderwerpobjectForZaak(Guid klantcontactUuid, string zaakUuid)
        {
            return OpenKlantApiClient.CreateOnderwerpobjectAsync(new KlantcontactOnderwerpobjectRequest
            {
                Klantcontact = new KlantcontactReference { Uuid = klantcontactUuid },
                WasKlantcontact = null,
                Onderwerpobjectidentificator = new Onderwerpobjectidentificator
                {
                    ObjectId = zaakUuid,
                    CodeObjecttype = "zgw-Zaak",
                    CodeRegister = "openzaak",
                    CodeSoortObjectId = "uuid"
                }
            });
        }

        private async Task<string?> TryGetZaakIdentificatieFromOnderwerpobject(Onderwerpobject? onderwerpobject)
        {
            if (onderwerpobject?.Uuid == null)
            {
                return null;
            }

            if (onderwerpobject?.Onderwerpobjectidentificator == null ||
                onderwerpobject.Onderwerpobjectidentificator.CodeObjecttype != "zgw-Zaak" ||
                onderwerpobject.Onderwerpobjectidentificator.CodeRegister != "openzaak")
            {
                return null;
            }

            try
            {
                var zaakUuid = onderwerpobject.Onderwerpobjectidentificator.ObjectId;
                var zaak = await ZakenApiClient.GetZaakAsync(zaakUuid);
                return zaak?.Identificatie;
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Failed to resolve zaak identificatie from onderwerpobject {OnderwerpobjectUuid} with zaak UUID {ZaakUuid}", 
                    onderwerpobject.Uuid, onderwerpobject.Onderwerpobjectidentificator?.ObjectId);
                return null;
            }
        }

    }
}