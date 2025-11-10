using InterneTaakAfhandeling.Common.Extensions;
using InterneTaakAfhandeling.Common.Services;
using InterneTaakAfhandeling.Common.Services.ObjectApi;
using InterneTaakAfhandeling.Common.Services.ObjectApi.Models;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using InterneTaakAfhandeling.Common.Services.ZakenApi;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace InterneTaakAfhandeling.EndToEndTest.Infrastructure
{
    public class TestDataHelper
    {
        private OpenKlantApiClient OpenKlantApiClient { get; }
        private ObjectApiClient ObjectApiClient { get; }
        private ZakenApiClient ZakenApiClient { get; }
        private string Username { get; }
        private string OpenKlantBaseUrl { get; }
        private string OpenKlantApiKey { get; }

        public TestDataHelper(string openKlantBaseUrl, string openKlantApiKey, string objectenApiBaseUrl, string objectenApiKey,
            string zakenApiBaseUrl, string zakenApiKey, string zakenApiClientId,
            IOptions<LogboekOptions> l,
            IOptions<AfdelingOptions> a,
            IOptions<GroepOptions> g,
            string username)
        {
            // Store the values for use in delete methods
            OpenKlantBaseUrl = openKlantBaseUrl;
            OpenKlantApiKey = openKlantApiKey;
            Username = username;

            var loggerFactory = new LoggerFactory();

            var openKlantHttpClient = new HttpClient
            {
                BaseAddress = new Uri(openKlantBaseUrl)
            };

            openKlantHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", openKlantApiKey);

            OpenKlantApiClient = new OpenKlantApiClient(openKlantHttpClient, loggerFactory.CreateLogger<OpenKlantApiClient>());

            // create the objectenapiclient instance
            var objectApiHttpClient = new HttpClient
            {
                BaseAddress = new Uri(objectenApiBaseUrl)
            };

            objectApiHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", objectenApiKey);
            objectApiHttpClient.DefaultRequestHeaders.Add("Content-Crs", "EPSG:4326");

            ObjectApiClient = new ObjectApiClient(objectApiHttpClient, loggerFactory.CreateLogger<ObjectApiClient>(), l, a, g);

            // create the ZakenApiClient instance
            var zakenApiHttpClient = new HttpClient
            {
                BaseAddress = new Uri(zakenApiBaseUrl)
            };

            var zakenApiToken = ApiClientExtensions.GenerateZakenApiToken(zakenApiKey, zakenApiClientId);
            zakenApiHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", zakenApiToken);
            zakenApiHttpClient.DefaultRequestHeaders.Add("Accept-Crs", "EPSG:4326");

            ZakenApiClient = new ZakenApiClient(zakenApiHttpClient, loggerFactory.CreateLogger<ZakenApiClient>());
        }

        public async Task<Guid> CreateContactverzoek(string onderwerp = "Test_Contact_from_ITA_E2E_test", bool attachZaak = true)
        {
            var testContactmomentOnderwerp = onderwerp;
            var testContactverzoekNummer = attachZaak ? "8001321008" : "8001321009";
            var testZaakIdentificatie = "ZAAK-2023-002";

            Console.WriteLine("=== Starting CreateContactverzoek ===");

            // Check if contactmoment already exists
            var contactmomenten = await OpenKlantApiClient.QueryKlantcontactAsync(new KlantcontactQuery
            {
                Onderwerp = testContactmomentOnderwerp,
            });

            Assert.IsTrue(contactmomenten.Count <= 1, "Did not expect multiple test klantcontacten.");

            var contactmoment = contactmomenten.Count == 1
            ? contactmomenten.First()
            : await OpenKlantApiClient.CreateKlantcontactAsync(new KlantcontactRequest
            {
                IndicatieContactGelukt = true,
                Onderwerp = testContactmomentOnderwerp,
                Inhoud = "This is a test contact request created during an end-to-end test run.",
                Kanaal = "e-mail",
                PlaatsgevondenOp = DateTime.UtcNow,
                Taal = "nl",
                Vertrouwelijk = false
            }) ?? throw new Exception("Failed to create contactmoment for testing.");

            Console.WriteLine($"Contactmoment UUID: {contactmoment.Uuid}");

            // Get or create actor who submitted the request
            var actorWhoSubmittedTheContactrequest = await OpenKlantApiClient.QueryActorAsync(new ActorQuery
            {
                ActoridentificatorCodeObjecttype = KnownMedewerkerIdentificators.EmailFromEntraId.CodeObjecttype,
                ActoridentificatorCodeRegister = KnownMedewerkerIdentificators.EmailFromEntraId.CodeRegister,
                ActoridentificatorCodeSoortObjectId = KnownMedewerkerIdentificators.EmailFromEntraId.CodeSoortObjectId,
                IndicatieActief = true,
                SoortActor = SoortActor.medewerker,
                ActoridentificatorObjectId = Username
            });

            actorWhoSubmittedTheContactrequest ??= await OpenKlantApiClient.CreateActorAsync(new ActorRequest
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

            // Connect actor to contactmoment if not already connected
            if (!contactmoment.HadBetrokkenActoren.Any(a => a.Uuid == actorWhoSubmittedTheContactrequest.Uuid))
            {
                await OpenKlantApiClient.CreateActorKlantcontactAsync(new ActorKlantcontactRequest
                {
                    Actor = new ActorReference { Uuid = actorWhoSubmittedTheContactrequest.Uuid },
                    Klantcontact = new KlantcontactReference { Uuid = contactmoment.Uuid }
                });
                Console.WriteLine("Connected submitter actor to contactmoment");
            }

            // Get afdeling from ObjectApi
            var afdelingen = await ObjectApiClient.FindAfdelingen("Burgerzaken_ibz");
            Assert.AreEqual(1, afdelingen.Results.Count, "Expected exactly one afdeling with name 'Burgerzaken_ibz' in objectenapi for testing.");
            var afdeling = afdelingen.Results.First();

            // Get or create actor for afdeling
            var actorForAfdelingToWhichTheContactrequestWillBeAssigned = await OpenKlantApiClient.QueryActorAsync(new ActorQuery
            {
                ActoridentificatorCodeObjecttype = KnownAfdelingIdentificators.ObjectRegisterId.CodeObjecttype,
                ActoridentificatorCodeRegister = KnownAfdelingIdentificators.ObjectRegisterId.CodeRegister,
                ActoridentificatorCodeSoortObjectId = KnownMedewerkerIdentificators.ObjectRegisterId.CodeSoortObjectId,
                IndicatieActief = true,
                SoortActor = SoortActor.organisatorische_eenheid,
                ActoridentificatorObjectId = afdeling.Record.Data.Identificatie
            });

            actorForAfdelingToWhichTheContactrequestWillBeAssigned ??= await OpenKlantApiClient.CreateActorAsync(new ActorRequest
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

            Console.WriteLine($"Afdeling actor UUID: {actorForAfdelingToWhichTheContactrequestWillBeAssigned.Uuid}");

            // Get medewerker from ObjectApi
            var medewerkers = await ObjectApiClient.GetMedewerkersByIdentificatie("icatt-integratie-test@icatt.nl");
            Assert.AreEqual(1, medewerkers.Count, "Expected exactly one medewerker with identificatie 'icatt-integratie-test@icatt.nl' in objectenapi for testing.");
            var medewerker = medewerkers.First();

            // Get or create actor for medewerker
            var actorForMedewerkerToWhichTheContactrequestWillBeAssigned = await OpenKlantApiClient.QueryActorAsync(new ActorQuery
            {
                ActoridentificatorCodeObjecttype = KnownMedewerkerIdentificators.ObjectRegisterId.CodeObjecttype,
                ActoridentificatorCodeRegister = KnownMedewerkerIdentificators.ObjectRegisterId.CodeRegister,
                ActoridentificatorCodeSoortObjectId = KnownMedewerkerIdentificators.ObjectRegisterId.CodeSoortObjectId,
                IndicatieActief = true,
                SoortActor = SoortActor.medewerker,
                ActoridentificatorObjectId = medewerker.Identificatie
            });

            actorForMedewerkerToWhichTheContactrequestWillBeAssigned ??= await OpenKlantApiClient.CreateActorAsync(new ActorRequest
            {
                Naam = "ICATT Integratietest",
                SoortActor = SoortActor.medewerker,
                IndicatieActief = true,
                Actoridentificator = new Actoridentificator
                {
                    ObjectId = medewerker.Identificatie ?? throw new InvalidOperationException("Medewerker identificatie cannot be null"),
                    CodeObjecttype = KnownMedewerkerIdentificators.ObjectRegisterId.CodeObjecttype,
                    CodeRegister = KnownMedewerkerIdentificators.ObjectRegisterId.CodeRegister,
                    CodeSoortObjectId = KnownMedewerkerIdentificators.ObjectRegisterId.CodeSoortObjectId
                }
            });

            Console.WriteLine($"Medewerker actor UUID: {actorForMedewerkerToWhichTheContactrequestWillBeAssigned.Uuid}");

            // Check if internetaak already exists
            var internetaken = await OpenKlantApiClient.QueryInterneTakenAsync(new InterneTaakQuery
            {
                Nummer = testContactverzoekNummer
            });

            Assert.IsFalse(internetaken.Count > 1, "Did not expect multiple test internetaken.");

            if (internetaken.Count == 0)
            {
                Console.WriteLine("Creating new internetaak");
                var createdInternetaak = await OpenKlantApiClient.CreateInterneTaak(new InternetaakPostRequest
                {
                    AanleidinggevendKlantcontact = new UuidObject { Uuid = contactmoment.Uuid },
                    GevraagdeHandeling = "terugbellen svp",
                    Nummer = testContactverzoekNummer,
                    Status = KnownInternetaakStatussen.TeVerwerken,
                    ToegewezenAanActoren = [
                        new UuidObject { Uuid = Guid.Parse(actorForMedewerkerToWhichTheContactrequestWillBeAssigned.Uuid) },
                        new UuidObject { Uuid = Guid.Parse(actorForAfdelingToWhichTheContactrequestWillBeAssigned.Uuid) }
                    ],
                    Toelichting = "Test contactverzoek from ITA E2E test"
                });
                Console.WriteLine($"Created internetaak UUID: {createdInternetaak.Uuid}");
            }
            else
            {
                Console.WriteLine($"Internetaak already exists with UUID: {internetaken.First().Uuid}");
            }

            // === ALWAYS TRY TO CONNECT ZAAK (whether contactverzoek is new or existing) ===
            if (attachZaak)
            {
                Console.WriteLine($"Attempting to find zaak with identificatie: {testZaakIdentificatie}");
                var zaak = await ZakenApiClient.GetZaakByIdentificatieAsync(testZaakIdentificatie);

                if (zaak == null)
                {
                    Console.WriteLine($"WARNING: Zaak with identificatie '{testZaakIdentificatie}' not found in Zaken API. Skipping zaak connection.");
                    return contactmoment.Uuid;
                }

                Console.WriteLine($"Found zaak with UUID: {zaak.Uuid}");

                // Refresh contactmoment to get the latest onderwerpobjecten
                contactmoment = await OpenKlantApiClient.GetKlantcontactAsync(contactmoment.Uuid);

                // Check if zaak is already connected via an onderwerpobject
                var isZaakAlreadyConnected = false;
                if (contactmoment.GingOverOnderwerpobjecten?.Count > 0)
                {
                    Console.WriteLine($"Checking {contactmoment.GingOverOnderwerpobjecten.Count} existing onderwerpobjecten");
                    foreach (var onderwerpobjectRef in contactmoment.GingOverOnderwerpobjecten)
                    {
                        if (onderwerpobjectRef?.Uuid == null) continue;

                        var onderwerpobject = await OpenKlantApiClient.GetOnderwerpobjectAsync(onderwerpobjectRef.Uuid.Value);

                        if (onderwerpobject?.Onderwerpobjectidentificator != null &&
                            onderwerpobject.Onderwerpobjectidentificator.CodeObjecttype == "zgw-Zaak" &&
                            onderwerpobject.Onderwerpobjectidentificator.CodeRegister == "openzaak" &&
                            onderwerpobject.Onderwerpobjectidentificator.ObjectId == zaak.Uuid)
                        {
                            Console.WriteLine($"Zaak is already connected via onderwerpobject {onderwerpobject.Uuid}");
                            isZaakAlreadyConnected = true;
                            break;
                        }
                    }
                }
                else
                {
                    Console.WriteLine("No existing onderwerpobjecten found");
                }

                // Create the onderwerpobject to link the zaak to the klantcontact
                if (!isZaakAlreadyConnected)
                {
                    Console.WriteLine($"Creating onderwerpobject to link zaak {zaak.Uuid} to klantcontact {contactmoment.Uuid}");
                    try
                    {
                        var createdOnderwerpobject = await OpenKlantApiClient.CreateOnderwerpobjectAsync(new KlantcontactOnderwerpobjectRequest
                        {
                            Klantcontact = new KlantcontactReference { Uuid = contactmoment.Uuid },
                            WasKlantcontact = null,
                            Onderwerpobjectidentificator = new Onderwerpobjectidentificator
                            {
                                ObjectId = zaak.Uuid,
                                CodeObjecttype = "zgw-Zaak",
                                CodeRegister = "openzaak",
                                CodeSoortObjectId = "uuid"
                            }
                        });
                        Console.WriteLine($"✓ Successfully created onderwerpobject {createdOnderwerpobject.Uuid}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"ERROR creating onderwerpobject: {ex.Message}");
                        throw;
                    }
                }
            }
            else
            {
                Console.WriteLine("Skipping ZAAK attachment as attachZaak parameter is false");
            }

            Console.WriteLine("=== Finished CreateContactverzoek ===");
            return contactmoment.Uuid;
        }


        public async Task<(Guid contactverzoekWithZaak, Guid contactverzoekWithoutZaak)> CreateBothContactverzoekScenarios()
        {
            Console.WriteLine("=== Creating both contactverzoek scenarios ===");

            // Create contactverzoek WITH zaak (8001321008)
            var contactverzoekWithZaak = await CreateContactverzoek("Test_Contact_with_ZAAK_from_ITA_E2E_test", attachZaak: true);
            Console.WriteLine($"✓ Created contactverzoek WITH ZAAK: {contactverzoekWithZaak}");

            // Create contactverzoek WITHOUT zaak (8001321009)
            var contactverzoekWithoutZaak = await CreateContactverzoek("Test_Contact_without_ZAAK_from_ITA_E2E_test", attachZaak: false);
            Console.WriteLine($"✓ Created contactverzoek WITHOUT ZAAK: {contactverzoekWithoutZaak}");

            Console.WriteLine("=== Both contactverzoek scenarios created successfully ===");

            return (contactverzoekWithZaak, contactverzoekWithoutZaak);
        }

        public async Task DeleteTestKlantcontact(Guid uuid)
        {
            await OpenKlantApiClient.DeleteKlantcontactAsync(uuid);
        }
    }
}