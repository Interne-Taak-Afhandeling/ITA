using InterneTaakAfhandeling.Common.Extensions;
using InterneTaakAfhandeling.Common.Services;
using InterneTaakAfhandeling.Common.Services.ObjectApi;
using InterneTaakAfhandeling.Common.Services.ObjectApi.Models;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using InterneTaakAfhandeling.Common.Services.ZakenApi;
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

            //the contactmoment is the basis of a contactverzoek
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

            //now we have a contactmoment, let's add the rest

            //- the actor who submitted the contactmoment/internetaak
            //first check if that actor already exists
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
            }

            //we alse need actors for afdeling and or groep and or medewerker
            //they refer to items in the objectsregistry
            //we could manage those objectsregistry items from here,
            //but for now we will assume they were manually created there. it's very static stable data.
            //eventually it would be better to manage that from here, but let's take it one step at the time

            //find the objectenapi afdeling to which we will assign the internetaak
            //depending on what we're going to test we'll probably need to do the same for medewerker and groep

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

            // Find the medewerker from the objectenapi to which we will assign the internetaak
            var medewerkers = await ObjectApiClient.GetMedewerkersByIdentificatie("icatt-integratie-test@icatt.nl");
            Assert.AreEqual(1, medewerkers.Count, "Expected exactly one medewerker with identificatie 'icatt-integratie-test@icatt.nl' in objectenapi for testing.");
            var medewerker = medewerkers.First();

            // Create or query the actor representing the medewerker to which the contactrequest will be assigned
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
                    ObjectId = medewerker.Identificatie!,
                    CodeObjecttype = KnownMedewerkerIdentificators.ObjectRegisterId.CodeObjecttype,
                    CodeRegister = KnownMedewerkerIdentificators.ObjectRegisterId.CodeRegister,
                    CodeSoortObjectId = KnownMedewerkerIdentificators.ObjectRegisterId.CodeSoortObjectId
                }
            });

            // Check if internetaak already exists
            var internetaken = await OpenKlantApiClient.QueryInterneTakenAsync(new InterneTaakQuery
            {
                Nummer = testContactverzoekNummer
            });

            Assert.IsFalse(internetaken.Count > 1, "Did not expect multiple test internetaken.");

            if (internetaken.Count == 0)
            {
                await OpenKlantApiClient.CreateInterneTaak(new InternetaakPostRequest
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
            }

            // Optionally attach zaak
            if (attachZaak)
            {
                await AttachZaakToContactmomentAsync(contactmoment.Uuid, testZaakIdentificatie);
            }

            return contactmoment.Uuid;
        }

        private async Task AttachZaakToContactmomentAsync(Guid klantcontactUuid, string zaakIdentificatie)
        {
            var zaak = await ZakenApiClient.GetZaakByIdentificatieAsync(zaakIdentificatie);

            if (zaak == null)
            {
                return;
            }

            // Refresh contactmoment to get the latest onderwerpobjecten
            var contactmoment = await OpenKlantApiClient.GetKlantcontactAsync(klantcontactUuid);

            // Check if zaak is already connected via an onderwerpobject
            var isZaakAlreadyConnected = false;
            if (contactmoment.GingOverOnderwerpobjecten?.Count > 0)
            {
                foreach (var onderwerpobjectRef in contactmoment.GingOverOnderwerpobjecten)
                {
                    if (onderwerpobjectRef?.Uuid == null) continue;

                    var onderwerpobject = await OpenKlantApiClient.GetOnderwerpobjectAsync(onderwerpobjectRef.Uuid.Value);

                    if (onderwerpobject?.Onderwerpobjectidentificator != null &&
                        onderwerpobject.Onderwerpobjectidentificator.CodeObjecttype == "zgw-Zaak" &&
                        onderwerpobject.Onderwerpobjectidentificator.CodeRegister == "openzaak" &&
                        onderwerpobject.Onderwerpobjectidentificator.ObjectId == zaak.Uuid)
                    {
                        isZaakAlreadyConnected = true;
                        break;
                    }
                }
            }

            // Create the onderwerpobject to link the zaak to the klantcontact
            if (!isZaakAlreadyConnected)
            {
                await OpenKlantApiClient.CreateOnderwerpobjectAsync(new KlantcontactOnderwerpobjectRequest
                {
                    Klantcontact = new KlantcontactReference { Uuid = klantcontactUuid },
                    WasKlantcontact = null,
                    Onderwerpobjectidentificator = new Onderwerpobjectidentificator
                    {
                        ObjectId = zaak.Uuid,
                        CodeObjecttype = "zgw-Zaak",
                        CodeRegister = "openzaak",
                        CodeSoortObjectId = "uuid"
                    }
                });
            }
        }


        /// <summary>
        /// Retrieves the ZAAK identificatie connected to a contactverzoek
        /// </summary>
        public async Task<string?> GetZaakIdentificatieFromContactverzoek(Guid klantcontactUuid)
        {
            try
            {
                var contactmoment = await OpenKlantApiClient.GetKlantcontactAsync(klantcontactUuid);

                if (contactmoment?.GingOverOnderwerpobjecten == null || contactmoment.GingOverOnderwerpobjecten.Count == 0)
                {
                    return null;
                }

                foreach (var onderwerpobjectRef in contactmoment.GingOverOnderwerpobjecten)
                {
                    if (onderwerpobjectRef?.Uuid == null) continue;

                    var onderwerpobject = await OpenKlantApiClient.GetOnderwerpobjectAsync(onderwerpobjectRef.Uuid.Value);

                    if (onderwerpobject?.Onderwerpobjectidentificator != null &&
                        onderwerpobject.Onderwerpobjectidentificator.CodeObjecttype == "zgw-Zaak" &&
                        onderwerpobject.Onderwerpobjectidentificator.CodeRegister == "openzaak")
                    {
                        var zaakUuid = onderwerpobject.Onderwerpobjectidentificator.ObjectId;
                        var zaak = await ZakenApiClient.GetZaakAsync(zaakUuid);

                        return zaak?.Identificatie;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to get ZAAK identificatie for klantcontact {klantcontactUuid}: {ex.Message}");
                return null;
            }
        }
        public async Task<Guid> CreateContactverzoekWithAfdelingMedewerkerAndPartij(
        string onderwerp = "Test_Contact_with_BSN_Partij",
        string bsn = "999992223",
        bool attachZaak = false)
        {
            var testContactmomentOnderwerp = onderwerp;
            var testContactverzoekNummer = "8001321010";
            var testZaakIdentificatie = "ZAAK-2023-002";

            // 1. Clean up existing contactmomenten with the same onderwerp
            var contactmomenten = await OpenKlantApiClient.QueryKlantcontactAsync(new KlantcontactQuery
            {
                Onderwerp = testContactmomentOnderwerp,
            });

            foreach (var existing in contactmomenten)
            {
                await OpenKlantApiClient.DeleteKlantcontactAsync(existing.Uuid);
            }

            // 2. Re-query to ensure all are deleted
            contactmomenten = await OpenKlantApiClient.QueryKlantcontactAsync(new KlantcontactQuery
            {
                Onderwerp = testContactmomentOnderwerp,
            });
            Assert.IsTrue(contactmomenten.Count == 0, "Did not expect any test klantcontacten after cleanup.");

            // 3. Find the partij by BSN
            var partijen = await OpenKlantApiClient.GetPartijenByBsnAsync(bsn);

            var partij = partijen.FirstOrDefault();
            if (partij == null)
                throw new Exception($"Partij with BSN {bsn} not found");

            // 4. Create the contactmoment first
            var contactmoment = await OpenKlantApiClient.CreateKlantcontactAsync(new KlantcontactRequest
            {
                IndicatieContactGelukt = true,
                Onderwerp = testContactmomentOnderwerp,
                Inhoud = "This is a test contact request created during an end-to-end test run.",
                Kanaal = "e-mail",
                PlaatsgevondenOp = DateTime.UtcNow,
                Taal = "nl",
                Vertrouwelijk = false
            }) ?? throw new Exception("Failed to create contactmoment for testing.");

            // 5. Create the actor who submitted the contactmoment (like in original method)
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
            await OpenKlantApiClient.CreateActorKlantcontactAsync(new ActorKlantcontactRequest
            {
                Actor = new ActorReference { Uuid = actorWhoSubmittedTheContactrequest.Uuid },
                Klantcontact = new KlantcontactReference { Uuid = contactmoment.Uuid }
            });

            // 6. Attach the partij as betrokkene
            var betrokkene = await OpenKlantApiClient.CreateBetrokkeneAsync(new BetrokkeneRequest
            {
                WasPartij = new PartijReference { Uuid = Guid.Parse(partij.Uuid) },
                HadKlantcontact = new KlantcontactReference { Uuid = contactmoment.Uuid },
                Rol = "klant",
                Initiator = true,
                Contactnaam = new Contactnaam {
                    Voorletters = "V",
                    Voornaam = "Voornaam",
                    Achternaam = "Achternaam",
                    VoorvoegselAchternaam = "de"
                }
            });

            var klantnaam = partij.Naam ?? "Onbekende klant";
            // Update the contactmoment with the klantnaam
            var updatedContactmoment = await OpenKlantApiClient.PutKlantcontactAsync(contactmoment.Uuid, new KlantcontactRequest
            {
                IndicatieContactGelukt = true,
                Onderwerp = testContactmomentOnderwerp,
                Inhoud = "This is a test contact request created during an end-to-end test run.",
                Kanaal = "e-mail",
                PlaatsgevondenOp = DateTime.UtcNow,
                Taal = "nl",
                Vertrouwelijk = false,
                Klantnaam = klantnaam
            });

            // 7. Find afdeling and create/get actor for afdeling
            var afdelingen = await ObjectApiClient.FindAfdelingen("Burgerzaken_ibz");
            Assert.AreEqual(1, afdelingen.Results.Count, "Expected exactly one afdeling with name 'Burgerzaken_ibz' in objectenapi for testing.");
            var afdeling = afdelingen.Results.First();

            var actorForAfdeling = await OpenKlantApiClient.QueryActorAsync(new ActorQuery
            {
                ActoridentificatorCodeObjecttype = KnownAfdelingIdentificators.ObjectRegisterId.CodeObjecttype,
                ActoridentificatorCodeRegister = KnownAfdelingIdentificators.ObjectRegisterId.CodeRegister,
                ActoridentificatorCodeSoortObjectId = KnownMedewerkerIdentificators.ObjectRegisterId.CodeSoortObjectId,
                IndicatieActief = true,
                SoortActor = SoortActor.organisatorische_eenheid,
                ActoridentificatorObjectId = afdeling.Record.Data.Identificatie
            });

            actorForAfdeling ??= await OpenKlantApiClient.CreateActorAsync(new ActorRequest
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

            // 8. Find medewerker 
            var medewerkers = await ObjectApiClient.GetMedewerkersByIdentificatie("icatt-integratie-test@icatt.nl");
            Assert.AreEqual(1, medewerkers.Count, "Expected exactly one medewerker with identificatie 'icatt-integratie-test@icatt.nl' in objectenapi for testing.");
            var medewerker = medewerkers.First();

            var actorForMedewerker = await OpenKlantApiClient.QueryActorAsync(new ActorQuery
            {
                ActoridentificatorCodeObjecttype = KnownMedewerkerIdentificators.ObjectRegisterId.CodeObjecttype,
                ActoridentificatorCodeRegister = KnownMedewerkerIdentificators.ObjectRegisterId.CodeRegister,
                ActoridentificatorCodeSoortObjectId = KnownMedewerkerIdentificators.ObjectRegisterId.CodeSoortObjectId,
                IndicatieActief = true,
                SoortActor = SoortActor.medewerker,
                ActoridentificatorObjectId = medewerker.Identificatie
            });

            actorForMedewerker ??= await OpenKlantApiClient.CreateActorAsync(new ActorRequest
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

            // 9. Create the internetaak with both afdeling and medewerker actors
            var internetaken = await OpenKlantApiClient.QueryInterneTakenAsync(new InterneTaakQuery
            {
                Nummer = testContactverzoekNummer
            });

            Assert.IsFalse(internetaken.Count > 1, "Did not expect multiple test internetaken.");

            if (internetaken.Count == 0)
            {
                await OpenKlantApiClient.CreateInterneTaak(new InternetaakPostRequest
                {
                    AanleidinggevendKlantcontact = new UuidObject { Uuid = contactmoment.Uuid },
                    GevraagdeHandeling = "terugbellen svp",
                    Nummer = testContactverzoekNummer,
                    Status = KnownInternetaakStatussen.TeVerwerken,
                    ToegewezenAanActoren = [
                        new UuidObject { Uuid = Guid.Parse(actorForMedewerker.Uuid) },
                new UuidObject { Uuid = Guid.Parse(actorForAfdeling.Uuid) }
                    ],
                    Toelichting = "Test contactverzoek from ITA E2E test"
                });
            }

            // 10. Optionally attach zaak
            if (attachZaak)
            {
                await AttachZaakToContactmomentAsync(contactmoment.Uuid, testZaakIdentificatie);
            }

            return contactmoment.Uuid;
        }

        public async Task DeleteContactverzoekAsync(string klantcontactUuid)
        {
            var uuid = Guid.Parse(klantcontactUuid);
            try
            {
                await OpenKlantApiClient.DeleteKlantcontactAsync(uuid);
            }
            catch (Exception ex)
            {
                // Silent catch for deletion errors
            }
        }
    }
}
