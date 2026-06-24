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
using System.Net.Http.Json;

namespace InterneTaakAfhandeling.EndToEndTest.Infrastructure
{
    public class TestDataHelper
    {
        private OpenKlantApiClient OpenKlantApiClient { get; }
        private ObjectApiClient ObjectApiClient { get; }
        private ZakenApiClient ZakenApiClient { get; }
        private HttpClient OpenKlantHttpClient { get; }
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
            IOptions<MedewerkerOptions> m,
            string username)
        {
            Username = username;

            var loggerFactory = new LoggerFactory();
            Logger = loggerFactory.CreateLogger<TestDataHelper>();

            OpenKlantApiClient = CreateOpenKlantApiClient(openKlantBaseUrl, openKlantApiKey, loggerFactory);
            OpenKlantHttpClient = CreateOpenKlantHttpClient(openKlantBaseUrl, openKlantApiKey);
            ObjectApiClient = CreateObjectApiClient(objectenApiBaseUrl, objectenApiKey, loggerFactory, l, a, g, m);
            ZakenApiClient = CreateZakenApiClient(zakenApiBaseUrl, zakenApiKey, zakenApiClientId, loggerFactory);
        }

        // Client Creation

        private static HttpClient CreateOpenKlantHttpClient(string baseUrl, string apiKey)
        {
            var httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", apiKey);
            return httpClient;
        }

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
            IOptions<GroepOptions> g,
            IOptions<MedewerkerOptions> m)
        {
            var httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", apiKey);
            httpClient.DefaultRequestHeaders.Add("Content-Crs", "EPSG:4326");
            return new ObjectApiClient(httpClient, loggerFactory.CreateLogger<ObjectApiClient>(), l, a, g, m);
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
            bool attachZaak = true)
        {
            var contactverzoekNummer = GenerateUniqueInternetaakNummer();

            var contactmoment = await CreateContactmoment(
                onderwerp, 
                "This is a test contact request created during an end-to-end test run.",
                klantnaam: null);

            var submitterActor = await GetOrCreateSubmitterActor();
            
            await ConnectActorToContactmoment(submitterActor, contactmoment.Uuid);

            var afdelingActor = await GetOrCreateAfdelingActor("Burgerzaken_ibz");
            
            var medewerkerActor = await GetOrCreateMedewerkerActor("icatt-integratie-test@icatt.nl");

            await CreateInternetaak(
                contactverzoekNummer,
                contactmoment.Uuid,
                new List<Guid> 
                { 
                    Guid.Parse(medewerkerActor.Uuid), 
                    Guid.Parse(afdelingActor.Uuid) 
                });

            if (attachZaak)
            {
                await AttachZaakToContactmomentAsync(contactmoment.Uuid, TestDataConstants.Zaken.TestZaakIdentificatie);
            }

            return contactmoment.Uuid;
        }

        public async Task<(Guid ContactmomentUuid, string InternetaakNummer)> CreateContactverzoekWithAfdelingMedewerkerAndPartij(
            string onderwerp,
            string bsn = TestDataConstants.Partijen.TestBsn,
            bool attachZaak = false)
        {
            var partij = await GetPartijByBsn(bsn);
            var contactmoment = await CreateContactmomentWithPartij(
                onderwerp, 
                "This is a test contact request created during an end-to-end test run.",
                partij);

            var submitterActor = await GetOrCreateSubmitterActor();
            await ConnectActorToContactmoment(submitterActor, contactmoment.Uuid);

            var afdelingActor = await GetOrCreateAfdelingActor("Burgerzaken_ibz");
            var medewerkerActor = await GetOrCreateMedewerkerActor("icatt-integratie-test@icatt.nl");

            var nummer = await CreateInternetaak(
                GenerateUniqueInternetaakNummer(),
                contactmoment.Uuid,
                new List<Guid> 
                { 
                    Guid.Parse(medewerkerActor.Uuid), 
                    Guid.Parse(afdelingActor.Uuid) 
                });

            if (attachZaak)
            {
                await AttachZaakToContactmomentAsync(contactmoment.Uuid, TestDataConstants.Zaken.TestZaakIdentificatie);
            }

            return (contactmoment.Uuid, nummer);
        }

        public async Task<(Guid ContactmomentUuid, string InternetaakNummer)> CreateContactverzoekWithCurrentUserAssigned(string onderwerp)
        {
            var contactmoment = await CreateContactmoment(
                onderwerp,
                "This is a test contact request created during an end-to-end test run.",
                klantnaam: null);

            var submitterActor = await GetOrCreateSubmitterActor();
            await ConnectActorToContactmoment(submitterActor, contactmoment.Uuid);

            var afdelingActor = await GetOrCreateAfdelingActor("Burgerzaken_ibz");

            var nummer = await CreateInternetaak(
                GenerateUniqueInternetaakNummer(),
                contactmoment.Uuid,
                new List<Guid>
                {
                    Guid.Parse(submitterActor.Uuid),
                    Guid.Parse(afdelingActor.Uuid)
                });

            return (contactmoment.Uuid, nummer);
        }

        public async Task<(Guid ContactmomentUuid, string InternetaakNummer)> CreateContactverzoekWithTeamAssignmentOnly(string onderwerp)
        {
            var contactmoment = await CreateContactmoment(
                onderwerp,
                "This is a test contact request created during an end-to-end test run.",
                klantnaam: null);

            var submitterActor = await GetOrCreateSubmitterActor();
            await ConnectActorToContactmoment(submitterActor, contactmoment.Uuid);

            var afdelingActor = await GetOrCreateAfdelingActor("Burgerzaken_ibz");

            var nummer = await CreateInternetaak(
                GenerateUniqueInternetaakNummer(),
                contactmoment.Uuid,
                new List<Guid> { Guid.Parse(afdelingActor.Uuid) });

            return (contactmoment.Uuid, nummer);
        }

        public async Task<(Guid ContactmomentUuid, string InternetaakNummer)> CreateContactverzoekWithNoAssignments(string onderwerp)
        {
            var contactmoment = await CreateContactmoment(
                onderwerp,
                "This is a test contact request created during an end-to-end test run.",
                klantnaam: null);

            var submitterActor = await GetOrCreateSubmitterActor();
            await ConnectActorToContactmoment(submitterActor, contactmoment.Uuid);

            var nummer = await CreateInternetaak(
                GenerateUniqueInternetaakNummer(),
                contactmoment.Uuid,
                new List<Guid>());

            return (contactmoment.Uuid, nummer);
        }

        public async Task<(Guid ContactmomentUuid, string InternetaakNummer)> CreateContactverzoekWithGroepAndMedewerker(string onderwerp)
        {
            await CleanupExistingContactmomenten(onderwerp);

            var contactmoment = await CreateContactmoment(
                onderwerp,
                "This is a test contact request created during an end-to-end test run.",
                klantnaam: null);

            var submitterActor = await GetOrCreateSubmitterActor();
            await ConnectActorToContactmoment(submitterActor, contactmoment.Uuid);

            var groepActor = await GetOrCreateGroepActor();
            var medewerkerActor = await GetOrCreateMedewerkerActor("icatt-integratie-test@icatt.nl");

            await CreateInternetaakIfNotExists(
                GenerateUniqueInternetaakNummer(),
                contactmoment.Uuid,
                new List<Guid>
                {
                    Guid.Parse(medewerkerActor.Uuid),
                    Guid.Parse(groepActor.Uuid)
                },
                isExplicitNummer: false);

            var internetaakUuid = await GetInternetaakUuidFromContactmomentAsync(contactmoment.Uuid)
                ?? throw new InvalidOperationException($"Internetaak not found after creation for contactmoment {contactmoment.Uuid}");
            var internetaak = await GetInternetaakByIdAsync(internetaakUuid);
            return (contactmoment.Uuid, internetaak.Nummer
                ?? throw new InvalidOperationException("Internetaak nummer is null after creation"));
        }

        public async Task<(Guid ContactmomentUuid, string InternetaakNummer)> CreateContactverzoekWithAfdelingAndMedewerker(string onderwerp)
        {
            await CleanupExistingContactmomenten(onderwerp);

            var contactmoment = await CreateContactmoment(
                onderwerp,
                "This is a test contact request created during an end-to-end test run.",
                klantnaam: null);

            var submitterActor = await GetOrCreateSubmitterActor();
            await ConnectActorToContactmoment(submitterActor, contactmoment.Uuid);

            var afdelingActor = await GetOrCreateAfdelingActor("Burgerzaken_ibz");
            var medewerkerActor = await GetOrCreateMedewerkerActor("icatt-integratie-test@icatt.nl");

            await CreateInternetaakIfNotExists(
                GenerateUniqueInternetaakNummer(),
                contactmoment.Uuid,
                new List<Guid>
                {
                    Guid.Parse(medewerkerActor.Uuid),
                    Guid.Parse(afdelingActor.Uuid)
                },
                isExplicitNummer: false);

            var internetaakUuid = await GetInternetaakUuidFromContactmomentAsync(contactmoment.Uuid)
                ?? throw new InvalidOperationException($"Internetaak not found after creation for contactmoment {contactmoment.Uuid}");
            var internetaak = await GetInternetaakByIdAsync(internetaakUuid);
            return (contactmoment.Uuid, internetaak.Nummer
                ?? throw new InvalidOperationException("Internetaak nummer is null after creation"));
        }

        public async Task<(Guid ContactmomentUuid, string InternetaakNummer)> CreateContactverzoekWithAfdelingOnly(string onderwerp)
        {
            await CleanupExistingContactmomenten(onderwerp);

            var contactmoment = await CreateContactmoment(
                onderwerp,
                "This is a test contact request created during an end-to-end test run.",
                klantnaam: null);

            var submitterActor = await GetOrCreateSubmitterActor();
            await ConnectActorToContactmoment(submitterActor, contactmoment.Uuid);

            var afdelingActor = await GetOrCreateAfdelingActor("Burgerzaken_ibz");

            await CreateInternetaakIfNotExists(
                GenerateUniqueInternetaakNummer(),
                contactmoment.Uuid,
                new List<Guid> { Guid.Parse(afdelingActor.Uuid) },
                isExplicitNummer: false);

            var internetaakUuid = await GetInternetaakUuidFromContactmomentAsync(contactmoment.Uuid)
                ?? throw new InvalidOperationException($"Internetaak not found after creation for contactmoment {contactmoment.Uuid}");
            var internetaak = await GetInternetaakByIdAsync(internetaakUuid);
            return (contactmoment.Uuid, internetaak.Nummer
                ?? throw new InvalidOperationException("Internetaak nummer is null after creation"));
        }

        public async Task<(Guid ContactmomentUuid, string InternetaakNummer)> CreateContactverzoekWithGroepOnly(string onderwerp)
        {
            await CleanupExistingContactmomenten(onderwerp);

            var contactmoment = await CreateContactmoment(
                onderwerp,
                "This is a test contact request created during an end-to-end test run.",
                klantnaam: null);

            var submitterActor = await GetOrCreateSubmitterActor();
            await ConnectActorToContactmoment(submitterActor, contactmoment.Uuid);

            var groepActor = await GetOrCreateGroepActor();

            await CreateInternetaakIfNotExists(
                GenerateUniqueInternetaakNummer(),
                contactmoment.Uuid,
                new List<Guid> { Guid.Parse(groepActor.Uuid) },
                isExplicitNummer: false);

            var internetaakUuid = await GetInternetaakUuidFromContactmomentAsync(contactmoment.Uuid)
                ?? throw new InvalidOperationException($"Internetaak not found after creation for contactmoment {contactmoment.Uuid}");
            var internetaak = await GetInternetaakByIdAsync(internetaakUuid);
            return (contactmoment.Uuid, internetaak.Nummer
                ?? throw new InvalidOperationException("Internetaak nummer is null after creation"));
        }

        public async Task<(Guid ContactmomentUuid, string InternetaakNummer)> CreateContactverzoekWithUnknownOeType(string onderwerp)
        {
            await CleanupExistingContactmomenten(onderwerp);

            var contactmoment = await CreateContactmoment(
                onderwerp,
                "This is a test contact request created during an end-to-end test run.",
                klantnaam: null);

            var submitterActor = await GetOrCreateSubmitterActor();
            await ConnectActorToContactmoment(submitterActor, contactmoment.Uuid);

            var unknownOeActor = await GetOrCreateUnknownTypeOeActor();

            await CreateInternetaakIfNotExists(
                GenerateUniqueInternetaakNummer(),
                contactmoment.Uuid,
                new List<Guid> { Guid.Parse(unknownOeActor.Uuid) },
                isExplicitNummer: false);

            var internetaakUuid = await GetInternetaakUuidFromContactmomentAsync(contactmoment.Uuid)
                ?? throw new InvalidOperationException($"Internetaak not found after creation for contactmoment {contactmoment.Uuid}");
            var internetaak = await GetInternetaakByIdAsync(internetaakUuid);
            return (contactmoment.Uuid, internetaak.Nummer
                ?? throw new InvalidOperationException("Internetaak nummer is null after creation"));
        }

        public async Task<(Guid ContactmomentUuid, string InternetaakNummer)> CreateContactverzoekWithMultipleOeActors(string onderwerp)
        {
            await CleanupExistingContactmomenten(onderwerp);

            var contactmoment = await CreateContactmoment(
                onderwerp,
                "This is a test contact request created during an end-to-end test run.",
                klantnaam: null);

            var submitterActor = await GetOrCreateSubmitterActor();
            await ConnectActorToContactmoment(submitterActor, contactmoment.Uuid);

            var afdelingActor = await GetOrCreateAfdelingActor("Burgerzaken_ibz");
            var groepActor = await GetOrCreateGroepActor();

            await CreateInternetaakIfNotExists(
                GenerateUniqueInternetaakNummer(),
                contactmoment.Uuid,
                new List<Guid>
                {
                    Guid.Parse(afdelingActor.Uuid),
                    Guid.Parse(groepActor.Uuid)
                },
                isExplicitNummer: false);

            var internetaakUuid = await GetInternetaakUuidFromContactmomentAsync(contactmoment.Uuid)
                ?? throw new InvalidOperationException($"Internetaak not found after creation for contactmoment {contactmoment.Uuid}");
            var internetaak = await GetInternetaakByIdAsync(internetaakUuid);
            return (contactmoment.Uuid, internetaak.Nummer
                ?? throw new InvalidOperationException("Internetaak nummer is null after creation"));
        }

        public async Task<(Guid ContactmomentUuid, Guid InternetaakUuid, string InternetaakNummer)> CreateVerwerktContactverzoekAsync(string onderwerp)
        {
            var contactmoment = await CreateContactmoment(
                onderwerp,
                "Test contactverzoek for readonly guard verification",
                klantnaam: null);

            var submitterActor = await GetOrCreateSubmitterActor();
            await ConnectActorToContactmoment(submitterActor, contactmoment.Uuid);

            var nummer = await CreateInternetaak(
                GenerateUniqueInternetaakNummer(),
                contactmoment.Uuid,
                new List<Guid>());

            var internetaakUuid = await GetInternetaakUuidFromContactmomentAsync(contactmoment.Uuid)
                ?? throw new InvalidOperationException($"Internetaak not found after creation for contactmoment {contactmoment.Uuid}");

            await OpenKlantApiClient.PatchInternetaakStatusAsync(
                new InternetakenPatchStatusRequest { Status = KnownInternetaakStatussen.Verwerkt },
                internetaakUuid.ToString());

            return (contactmoment.Uuid, internetaakUuid, nummer);
        }

        public async Task<(Guid ContactmomentUuid, Guid InternetaakUuid, string InternetaakNummer)> CreateTeVerwerkenContactverzoekAsync(string onderwerp)
        {
            var contactmoment = await CreateContactmoment(
                onderwerp,
                "Test contactverzoek for heropenen E2E verification",
                klantnaam: null);

            var submitterActor = await GetOrCreateSubmitterActor();
            await ConnectActorToContactmoment(submitterActor, contactmoment.Uuid);

            var nummer = await CreateInternetaak(
                GenerateUniqueInternetaakNummer(),
                contactmoment.Uuid,
                new List<Guid>());

            var internetaakUuid = await GetInternetaakUuidFromContactmomentAsync(contactmoment.Uuid)
                ?? throw new InvalidOperationException($"Internetaak not found after creation for contactmoment {contactmoment.Uuid}");

            return (contactmoment.Uuid, internetaakUuid, nummer);
        }

        public async Task<(Guid ContactmomentUuid, string InternetaakNummer)> CreateContactverzoekWithTeamAssignmentNotCurrentUser(string onderwerp)
        {
            var contactmoment = await CreateContactmoment(
                onderwerp,
                "This is a test contact request created during an end-to-end test run.",
                klantnaam: null);

            var submitterActor = await GetOrCreateSubmitterActor();
            await ConnectActorToContactmoment(submitterActor, contactmoment.Uuid);

            var afdelingActor = await GetOrCreateAfdelingActor("Burgerzaken_ibz");
            var otherMedewerkerActor = await GetOrCreateMedewerkerActorDirectly("surbhi@info.nl");

            var nummer = await CreateInternetaak(
                GenerateUniqueInternetaakNummer(),
                contactmoment.Uuid,
                new List<Guid>
                {
                    Guid.Parse(otherMedewerkerActor.Uuid),
                    Guid.Parse(afdelingActor.Uuid)
                });

            return (contactmoment.Uuid, nummer);
        }

        public async Task<(Guid ContactmomentUuid, string InternetaakNummer)> CreateContactverzoekWithCurrentUserAssignedViaObjectRegisterId(string onderwerp)
        {
            var contactmoment = await CreateContactmoment(
                onderwerp,
                "This is a test contact request created during an end-to-end test run.",
                klantnaam: null);

            var submitterActor = await GetOrCreateSubmitterActor();
            await ConnectActorToContactmoment(submitterActor, contactmoment.Uuid);

            var afdelingActor = await GetOrCreateAfdelingActor("Burgerzaken_ibz");
            var medewerkerActorViaObjectRegisterId = await GetOrCreateMedewerkerActor(Username);

            var nummer = await CreateInternetaak(
                GenerateUniqueInternetaakNummer(),
                contactmoment.Uuid,
                new List<Guid>
                {
                    Guid.Parse(afdelingActor.Uuid),
                    Guid.Parse(medewerkerActorViaObjectRegisterId.Uuid)
                });

            return (contactmoment.Uuid, nummer);
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
                var internetaakUuid = await GetInternetaakUuidFromContactmomentAsync(uuid);
                if (internetaakUuid.HasValue)
                {
                    await OpenKlantApiClient.DeleteInterneTaakAsync(internetaakUuid.Value);
                }
            }
            catch (Exception) { }

            try
            {
                await OpenKlantApiClient.DeleteKlantcontactAsync(uuid);
            }
            catch (Exception) { }
        }

        public async Task<Internetaak> GetInternetaakByIdAsync(Guid uuid)
        {
            var internetaak = await OpenKlantApiClient.GetInternetaakByIdAsync(uuid);

            if (internetaak.AanleidinggevendKlantcontact?.Uuid != null && internetaak.AanleidinggevendKlantcontact.Uuid != Guid.Empty)
            {
                internetaak.AanleidinggevendKlantcontact = await OpenKlantApiClient.GetKlantcontactAsync(internetaak.AanleidinggevendKlantcontact.Uuid);
            }

            return internetaak;
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

            // The API can transiently reject the request (400) when the
            // contactmoment was just created. Retry once after a short delay.
            try
            {
                await OpenKlantApiClient.CreateActorKlantcontactAsync(new ActorKlantcontactRequest
                {
                    Actor = new ActorReference { Uuid = actor.Uuid },
                    Klantcontact = new KlantcontactReference { Uuid = contactmomentUuid }
                });
            }
            catch (Exception)
            {
                await Task.Delay(2000);
                await OpenKlantApiClient.CreateActorKlantcontactAsync(new ActorKlantcontactRequest
                {
                    Actor = new ActorReference { Uuid = actor.Uuid },
                    Klantcontact = new KlantcontactReference { Uuid = contactmomentUuid }
                });
            }
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

        /// <summary>
        /// Returns the display name of the current user's actor in OpenKlant.
        /// This may differ from the hardcoded name if the actor was created externally.
        /// </summary>
        public async Task<string> GetCurrentUserActorNameAsync()
        {
            var actor = await GetOrCreateSubmitterActor();
            return actor.Naam;
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

        /// <summary>
        /// Creates or gets a medewerker actor directly in OpenKlant without requiring the medewerker to exist in the Object API.
        /// </summary>
        private async Task<Actor> GetOrCreateMedewerkerActorDirectly(string identifier)
        {
            return await GetOrCreateActor(
                query: new ActorQuery
                {
                    ActoridentificatorCodeObjecttype = KnownMedewerkerIdentificators.ObjectRegisterId.CodeObjecttype,
                    ActoridentificatorCodeRegister = KnownMedewerkerIdentificators.ObjectRegisterId.CodeRegister,
                    ActoridentificatorCodeSoortObjectId = KnownMedewerkerIdentificators.ObjectRegisterId.CodeSoortObjectId,
                    IndicatieActief = true,
                    SoortActor = SoortActor.medewerker,
                    ActoridentificatorObjectId = identifier
                },
                request: new ActorRequest
                {
                    Naam = "Other Medewerker (Test)",
                    SoortActor = SoortActor.medewerker,
                    IndicatieActief = true,
                    Actoridentificator = new Actoridentificator
                    {
                        ObjectId = identifier,
                        CodeObjecttype = KnownMedewerkerIdentificators.ObjectRegisterId.CodeObjecttype,
                        CodeRegister = KnownMedewerkerIdentificators.ObjectRegisterId.CodeRegister,
                        CodeSoortObjectId = KnownMedewerkerIdentificators.ObjectRegisterId.CodeSoortObjectId
                    }
                });
        }

        private async Task<Actor> GetOrCreateGroepActor()
        {
            var groepen = await ObjectApiClient.GetGroepen(1);
            if (groepen.Results.Count == 0)
                throw new InvalidOperationException("No groepen found in the Objects API — cannot create groep actor.");

            var groep = groepen.Results.First().Record.Data;

            return await GetOrCreateActor(
                query: new ActorQuery
                {
                    ActoridentificatorCodeObjecttype = KnownGroepIdentificators.ObjectRegisterId.CodeObjecttype,
                    ActoridentificatorCodeRegister = KnownGroepIdentificators.ObjectRegisterId.CodeRegister,
                    ActoridentificatorCodeSoortObjectId = KnownGroepIdentificators.ObjectRegisterId.CodeSoortObjectId,
                    IndicatieActief = true,
                    SoortActor = SoortActor.organisatorische_eenheid,
                    ActoridentificatorObjectId = groep.Identificatie
                },
                request: new ActorRequest
                {
                    Naam = "e2e groep",
                    SoortActor = SoortActor.organisatorische_eenheid,
                    IndicatieActief = true,
                    Actoridentificator = new Actoridentificator
                    {
                        ObjectId = groep.Identificatie,
                        CodeObjecttype = KnownGroepIdentificators.ObjectRegisterId.CodeObjecttype,
                        CodeRegister = KnownGroepIdentificators.ObjectRegisterId.CodeRegister,
                        CodeSoortObjectId = KnownGroepIdentificators.ObjectRegisterId.CodeSoortObjectId
                    }
                });
        }

        private Task<Actor> GetOrCreateUnknownTypeOeActor()
        {
            return GetOrCreateActor(
                query: new ActorQuery
                {
                    ActoridentificatorCodeObjecttype = "onbekend",
                    ActoridentificatorCodeRegister = "obj",
                    ActoridentificatorCodeSoortObjectId = "idf",
                    IndicatieActief = true,
                    SoortActor = SoortActor.organisatorische_eenheid,
                    ActoridentificatorObjectId = "e2e-onbekend-oe-type"
                },
                request: new ActorRequest
                {
                    Naam = "e2e onbekende eenheid",
                    SoortActor = SoortActor.organisatorische_eenheid,
                    IndicatieActief = true,
                    Actoridentificator = new Actoridentificator
                    {
                        ObjectId = "e2e-onbekend-oe-type",
                        CodeObjecttype = "onbekend",
                        CodeRegister = "obj",
                        CodeSoortObjectId = "idf"
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

        private async Task<string> CreateInternetaak(
            string nummer,
            Guid contactmomentUuid,
            List<Guid> actorUuids)
        {
            await OpenKlantApiClient.CreateInterneTaak(new InternetaakPostRequest
            {
                AanleidinggevendKlantcontact = new UuidObject { Uuid = contactmomentUuid },
                GevraagdeHandeling = "terugbellen svp",
                Nummer = nummer,
                Status = KnownInternetaakStatussen.TeVerwerken,
                ToegewezenAanActoren = actorUuids.Select(id => new UuidObject { Uuid = id }).ToList(),
                Toelichting = "Test contactverzoek from ITA E2E test"
            });

            Logger.LogInformation("Successfully created internetaak with nummer '{Nummer}'", nummer);
            return nummer;
        }

        private async Task CreateInternetaakIfNotExists(
            string nummer,
            Guid contactmomentUuid,
            List<Guid> actorUuids,
            bool isExplicitNummer = false)
        {
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
                Logger.LogInformation("Internetaak with nummer '{Nummer}' already exists, skipping creation", nummer);
                return;
            }

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
                    return;
                }
                catch (HttpRequestException ex) when (ex.Message.Contains("409") || ex.Message.Contains("conflict"))
                {
                    lastConflictException = ex;

                    if (isExplicitNummer)
                    {
                        throw new InvalidOperationException(
                            $"Cannot create internetaak with explicit nummer '{nummer}' because it already exists. " +
                            "This breaks test contracts that expect this exact nummer for navigation/verification.",
                            ex);
                    }

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
                    Logger.LogError(ex, "Failed to create internetaak due to non-retryable error: {Message}", ex.Message);
                    throw;
                }
            }

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

        // Digitale Adressen Operations

        /// <summary>
        /// Creates a digitale adres linked to a betrokkene via the OpenKlant API.
        /// Returns the UUID of the created digitale adres for cleanup.
        /// </summary>
        public async Task<string> CreateDigitaalAdresForBetrokkeneAsync(
            string betrokkeneUuid,
            string adres,
            string soortDigitaalAdres,
            string omschrijving = "")
        {
            var request = new
            {
                verstrektDoorBetrokkene = new { uuid = betrokkeneUuid },
                verstrektDoorPartij = (object?)null,
                adres,
                soortDigitaalAdres,
                omschrijving
            };

            var response = await OpenKlantHttpClient.PostAsJsonAsync("digitaleadressen", request);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<DigitaleAdres>();
            Logger.LogInformation("Created digitale adres {Uuid} ({SoortDigitaalAdres}: {Adres}) for betrokkene {BetrokkeneUuid}",
                result!.Uuid, soortDigitaalAdres, adres, betrokkeneUuid);

            return result.Uuid;
        }

        /// <summary>
        /// Creates a digitale adres linked to a partij via the OpenKlant API.
        /// Returns the UUID of the created digitale adres for cleanup.
        /// </summary>
        public async Task<string> CreateDigitaalAdresForPartijAsync(
            string partijUuid,
            string adres,
            string soortDigitaalAdres,
            string omschrijving = "")
        {
            var request = new
            {
                verstrektDoorBetrokkene = (object?)null,
                verstrektDoorPartij = new { uuid = partijUuid },
                adres,
                soortDigitaalAdres,
                omschrijving
            };

            var response = await OpenKlantHttpClient.PostAsJsonAsync("digitaleadressen", request);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<DigitaleAdres>();
            Logger.LogInformation("Created digitale adres {Uuid} ({SoortDigitaalAdres}: {Adres}) for partij {PartijUuid}",
                result!.Uuid, soortDigitaalAdres, adres, partijUuid);

            return result.Uuid;
        }

        /// <summary>
        /// Looks up a partij by BSN and returns the UUID.
        /// </summary>
        public async Task<string> GetPartijUuidByBsnAsync(string bsn = TestDataConstants.Partijen.TestBsn)
        {
            var partij = await GetPartijByBsn(bsn);
            return partij.Uuid;
        }

        /// <summary>
        /// Returns existing digitale adressen for a partij.
        /// </summary>
        public Task<List<DigitaleAdres>> GetPartijDigitaleAdressenAsync(string partijUuid)
        {
            return OpenKlantApiClient.GetPartijDigitaleAdressenAsync(partijUuid);
        }

        /// <summary>
        /// Deletes a digitale adres from the OpenKlant API.
        /// </summary>
        public async Task DeleteDigitaalAdresAsync(string uuid)
        {
            try
            {
                var response = await OpenKlantHttpClient.DeleteAsync($"digitaleadressen/{uuid}");
                response.EnsureSuccessStatusCode();
                Logger.LogInformation("Deleted digitale adres {Uuid}", uuid);
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Failed to delete digitale adres {Uuid}", uuid);
            }
        }

        /// <summary>
        /// Creates a contactverzoek with a partij and betrokkene's own digitale adressen.
        /// Returns (contactmomentUuid, betrokkeneUuid, digitaalAdresUuid) for assertions and cleanup.
        /// </summary>
        public async Task<(Guid contactmomentUuid, string betrokkeneUuid, string digitaalAdresUuid)> CreateContactverzoekWithPartijAndOwnAdressenAsync(
            string onderwerp,
            string ownEmail,
            string bsn = TestDataConstants.Partijen.TestBsn)
        {
            var partij = await GetPartijByBsn(bsn);
            var contactmoment = await CreateContactmoment(onderwerp,
                "This is a test contact request created during an end-to-end test run.",
                partij.Naam);

            var betrokkene = await AttachPartijToContactmoment(Guid.Parse(partij.Uuid), contactmoment.Uuid);

            // Create own digitale adres on the betrokkene
            var digitaalAdresUuid = await CreateDigitaalAdresForBetrokkeneAsync(
                betrokkene.Uuid,
                ownEmail,
                "email",
                "Eigen e-mail");

            var submitterActor = await GetOrCreateSubmitterActor();
            await ConnectActorToContactmoment(submitterActor, contactmoment.Uuid);

            var afdelingActor = await GetOrCreateAfdelingActor("Burgerzaken_ibz");
            var medewerkerActor = await GetOrCreateMedewerkerActor("icatt-integratie-test@icatt.nl");

            var nummer = GenerateUniqueInternetaakNummer();
            await CreateInternetaak(
                nummer,
                contactmoment.Uuid,
                new List<Guid>
                {
                    Guid.Parse(medewerkerActor.Uuid),
                    Guid.Parse(afdelingActor.Uuid)
                });

            return (contactmoment.Uuid, betrokkene.Uuid, digitaalAdresUuid);
        }

        /// <summary>
        /// Creates a minimal partij without digitale adressen via the OpenKlant API.
        /// Returns the UUID of the created partij for cleanup.
        /// </summary>
        public async Task<string> CreatePartijWithoutAdressenAsync()
        {
            var request = new
            {
                digitaleAdressen = Array.Empty<object>(),
                voorkeursDigitaalAdres = (object?)null,
                partijIdentificatoren = Array.Empty<object>(),
                soortPartij = "persoon",
                indicatieActief = true,
                indicatieGeheimhouding = false,
                voorkeurstaal = "nl",
                bezoekadres = (object?)null,
                correspondentieadres = (object?)null
            };

            var response = await OpenKlantHttpClient.PostAsJsonAsync("partijen", request);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<Common.Services.OpenKlantApi.Models.Partij>();
            Logger.LogInformation("Created partij without adressen: {Uuid}", result!.Uuid);
            return result.Uuid;
        }

        /// <summary>
        /// Deletes a partij from the OpenKlant API.
        /// </summary>
        public async Task DeletePartijAsync(string uuid)
        {
            try
            {
                var response = await OpenKlantHttpClient.DeleteAsync($"partijen/{uuid}");
                if (response.IsSuccessStatusCode)
                {
                    Logger.LogInformation("Deleted partij {Uuid}", uuid);
                }
                else
                {
                    Logger.LogWarning("Failed to delete partij {Uuid}: {StatusCode}", uuid, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Failed to delete partij {Uuid}", uuid);
            }
        }

        /// <summary>
        /// Creates a contactverzoek with a specific partij UUID (without BSN lookup).
        /// Used for testing scenarios where the partij has specific characteristics.
        /// </summary>
        public async Task<Guid> CreateContactverzoekWithSpecificPartijAsync(string onderwerp, string partijUuid)
        {
            var contactmoment = await CreateContactmoment(onderwerp,
                "This is a test contact request created during an end-to-end test run.",
                klantnaam: null);

            await AttachPartijToContactmoment(Guid.Parse(partijUuid), contactmoment.Uuid);

            var submitterActor = await GetOrCreateSubmitterActor();
            await ConnectActorToContactmoment(submitterActor, contactmoment.Uuid);

            var afdelingActor = await GetOrCreateAfdelingActor("Burgerzaken_ibz");
            var medewerkerActor = await GetOrCreateMedewerkerActor("icatt-integratie-test@icatt.nl");

            var nummer = GenerateUniqueInternetaakNummer();
            await CreateInternetaak(
                nummer,
                contactmoment.Uuid,
                new List<Guid>
                {
                    Guid.Parse(medewerkerActor.Uuid),
                    Guid.Parse(afdelingActor.Uuid)
                });

            return contactmoment.Uuid;
        }

    }
}
