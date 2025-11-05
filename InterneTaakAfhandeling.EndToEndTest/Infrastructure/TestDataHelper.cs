using InterneTaakAfhandeling.Common.Services;
using InterneTaakAfhandeling.Common.Services.ObjectApi;
using InterneTaakAfhandeling.Common.Services.ObjectApi.Models;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
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
        private string Username { get; }
        private string OpenKlantBaseUrl { get; }
        private string OpenKlantApiKey { get; }

        public TestDataHelper(string openKlantBaseUrl, string openKlantApiKey, string objectenApiBaseUrl, string objectenApiKey,
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
        }

        public async Task<string> CreateContactverzoek(string onderwerp = "Test_Contact_from_ITA_E2E_test")
        {
            //we need to add multiplethings to create a contactverzoek.
            //for each we will check if it already exists to avoid creating duplicates on multiple test runs

            var testContactmomentOnderwerp = onderwerp;
            var testContactverzoekNummer = "8001321008";

            // OPTION: Uncomment the next line to always create fresh data with current timestamp
            // await DeleteTestKlantcontact();

            //the contactmoment is the basis of a contactverzoek
            //check if it already exists
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

            //- connect that actor to the contactmoment, if they are not connected allready
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

            //- assigning the internetaak to the actor representing the afdeling (do the same for a medewerker?)
            //first check if that actor already exists
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
                    ObjectId = medewerker.Identificatie ?? throw new InvalidOperationException("Medewerker identificatie cannot be null"),
                    CodeObjecttype = KnownMedewerkerIdentificators.ObjectRegisterId.CodeObjecttype,
                    CodeRegister = KnownMedewerkerIdentificators.ObjectRegisterId.CodeRegister,
                    CodeSoortObjectId = KnownMedewerkerIdentificators.ObjectRegisterId.CodeSoortObjectId
                }
            });

            //- the internetaak/contactverzoek
            //first check if it already exists
            var internetaken = await OpenKlantApiClient.QueryInterneTakenAsync(new InterneTaakQuery
            {
                Nummer = testContactverzoekNummer
            });

            Assert.IsFalse(internetaken.Count > 1, "Did not expect multiple test internetaken.");

            if (internetaken.Count == 0)
            {
                var createdInternetaak = await OpenKlantApiClient.CreateInterneTaak(new InternetaakPostRequest
                {
                    AanleidinggevendKlantcontact = new UuidObject { Uuid = Guid.Parse(contactmoment.Uuid) },
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

            return contactmoment.Uuid;
        }


        public async Task DeleteTestKlantcontact(string uuid)
        {
            await OpenKlantApiClient.DeleteKlantcontactAsync(uuid);
        }
    }
}
