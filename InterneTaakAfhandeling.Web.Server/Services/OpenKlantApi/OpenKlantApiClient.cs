using System.IO;
using System.Net.Http.Headers;
using System.Security.Claims;
using Duende.IdentityModel;
using InterneTaakAfhandeling.Web.Server.Middleware;
using InterneTaakAfhandeling.Web.Server.Services.ObjectApi;
using InterneTaakAfhandeling.Web.Server.Services.OpenKlantApi.Models;

namespace InterneTaakAfhandeling.Web.Server.Services.OpenKlantApi
{
    public interface IOpenKlantApiClient
    {
        Task<List<Internetaken>> GetInterneTakenByAssignedUser(string? userEmail);
        Task<Actor?> GetActorByEmailFromObjects(string userEmail);

        Task<Actor?> GetActorByEmail(string userEmail);
        Task<Actor?> CreateActorAsync(ActorRequest request);
        Task<Klantcontact?> GetKlantcontactAsync(string uuid);
        Task<Klantcontact> CreateKlantcontactAsync(KlantcontactRequest request);
        Task<ActorKlantcontact> CreateActorKlantcontactAsync(ActorKlantcontactRequest request);
        Task<Actor?> GetOrCreateActorByEmail(string email, string? naam = null);
    }
    public class OpenKlantApiClient : IOpenKlantApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration; 
        private readonly string _baseUrl;
        private readonly IObjectApiClient _objectApiClient;

        public OpenKlantApiClient(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<OpenKlantApiClient> logger,
            IHttpContextAccessor httpContextAccessor,
            IObjectApiClient objectApiClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
          
            _objectApiClient = objectApiClient ?? throw new ArgumentNullException(nameof(objectApiClient));

            _baseUrl = _configuration.GetValue<string>("OpenKlantApi:BaseUrl")
                ?? throw new InvalidOperationException("OpenKlantApi:BaseUrl configuration is missing");
            _httpClient.BaseAddress = new Uri(_baseUrl);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token",
                _configuration.GetValue<string>("OpenKlantApi:ApiKey") ?? throw new InvalidOperationException("OpenKlantApi:ApiKey configuration is missing"));
        }
        public async Task<Actor?> GetActorByEmail(string userEmail)
        {
            if (string.IsNullOrEmpty(userEmail))
            {
                throw new ConflictException("Email must not be empty",
                                            code: "EMPTY_EMAIL_ADDRESS");
            }

            try
            {
                var response = await _httpClient.GetAsync($"actoren?actoridentificatorObjectId={userEmail}");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadFromJsonAsync<ActorResponse>();
                return content?.Results?.FirstOrDefault();
            }
            catch (Exception ex)
            {
                // Specifieke exception handling en logging kan hier worden toegevoegd
                throw new ConflictException($"Error retrieving actor by email: {ex.Message}",
                                            code: "ACTOR_RETRIEVAL_ERROR");
            }
        }

        public async Task<Actor?> CreateActorAsync(ActorRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("actoren", request);
                response.EnsureSuccessStatusCode();

                var actor = await response.Content.ReadFromJsonAsync<Actor>();
                return actor;
            }
            catch (Exception ex)
            {
                throw new ConflictException($"Error creating actor: {ex.Message}",
                                           code: "ACTOR_CREATION_ERROR");
            }
        }

        public async Task<ActorKlantcontact> CreateActorKlantcontactAsync(ActorKlantcontactRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("actorklantcontacten", request);
                response.EnsureSuccessStatusCode();

                var actorKlantcontact = await response.Content.ReadFromJsonAsync<ActorKlantcontact>();

                if (actorKlantcontact == null)
                {
                    throw new ConflictException("Failed to deserialize created actor-klantcontact response",
                        code: "ACTOR_KLANTCONTACT_DESERIALIZATION_FAILED");
                }

                return actorKlantcontact;
            }
            catch (Exception ex)
            {
                throw new ConflictException($"Error linking actor to klantcontact: {ex.Message}",
                                           code: "ACTOR_KLANTCONTACT_LINKING_ERROR");
            }
        }

        public async Task<Actor?> GetOrCreateActorByEmail(string email, string? naam = null)
        {
            if (string.IsNullOrEmpty(email))
            {
                throw new ConflictException("Email must not be empty", code: "EMPTY_EMAIL_ADDRESS");
            }

            // Probeer eerst de actor op te halen
            var actor = await GetActorByEmail(email);

            // Als actor niet bestaat, maak een nieuwe aan
            if (actor == null)
            {
                // Maak een nieuwe actor request
                var actorRequest = new ActorRequest
                {
                    Naam = naam ?? email, // Als naam niet opgegeven is, gebruik email als naam
                    SoortActor = "medewerker",
                    IndicatieActief = true,
                    Actoridentificator = new ActorIdentificator
                    {
                        ObjectId = email,
                        CodeObjecttype = "mdw",
                        CodeRegister = "msei",
                        CodeSoortObjectId = "email"
                    },
                    ActorIdentificatie = new ActorIdentificatie
                    {
                        Emailadres = email
                    }
                };

                // Maak de actor aan
                actor = await CreateActorAsync(actorRequest);
            }

            return actor;
        }

        public async Task<Klantcontact> CreateKlantcontactAsync(KlantcontactRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("klantcontacten", request);
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error response: {responseContent}");
                var klantcontact = await response.Content.ReadFromJsonAsync<Klantcontact>();



                if (klantcontact == null)
                {
                    throw new ConflictException("Failed to deserialize created klantcontact response",
                        code: "KLANTCONTACT_DESERIALIZATION_FAILED");
                }

                return klantcontact;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<Actor?> GetActorByEmailFromObjects(string userEmail)
        {
            try
            {
                var objectRecords = await _objectApiClient.GetObjectsByEmail(userEmail);

                if (objectRecords == null || objectRecords.Count == 0)
                    throw new ConflictException("No object records found for the current user.",
                                                 code: "NO_OBJECT_RECORDS_FOUND");

                if (objectRecords.Count > 1)
                   throw new ConflictException("Multiple object records found for the current user.",
                                                 code: "MULTIPLE_OBJECT_RECORDS_FOUND");
                 

                var response = await _httpClient.GetAsync($"actoren?actoridentificatorObjectId={objectRecords?.FirstOrDefault()?.Data.Identificatie}");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadFromJsonAsync<ActorResponse>();

                return content?.Results?.FirstOrDefault();
            }
            catch (Exception)
            { 
                throw;
            }
        }

        public async Task<List<Internetaken>> GetInterneTakenByAssignedUser(string? userEmail)
        {
            try
            {

                if (string.IsNullOrEmpty(userEmail))
                    throw new ConflictException("No email found in the current user's claims.",
                                                 code: "MISSING_EMAIL_CLAIM");

                var actor = await GetActorByEmail(userEmail) ?? throw new ConflictException("No actor found for the current user.",
                                                 code: "NO_ACTOR_FOUND");



               List<Internetaken> content = new List<Internetaken>();
                var page = $"internetaken?toegewezenAanActor__uuid={actor?.Uuid}";
                while (!string.IsNullOrEmpty(page))
                {
                    var response = await _httpClient.GetAsync(page);
                    response.EnsureSuccessStatusCode();
                    var  currentContent = await response.Content.ReadFromJsonAsync<InternetakenResponse>();

                     await Task.WhenAll(currentContent?.Results?.Select(async x =>
                    {
                        x.AanleidinggevendKlantcontact = await GetKlantcontactAsync(x.AanleidinggevendKlantcontact?.Uuid ?? string.Empty);                      
                    }) ?? []);
                    content.AddRange(currentContent?.Results ?? []);
                    page = currentContent?.Next?.Replace(_baseUrl, string.Empty);
                }

                return content?.OrderBy(x => x.ToegewezenOp).ToList() ?? [];
            }
            catch (Exception)
            {
                 throw;
            }
        }
        public async Task<Klantcontact?> GetKlantcontactAsync(string uuid)
        {
         
            var response = await _httpClient.GetAsync($"klantcontacten/{uuid}?expand=leiddeTotInterneTaken,gingOverOnderwerpobjecten,hadBetrokkenen,hadBetrokkenen.digitaleAdressen");
            response.EnsureSuccessStatusCode();

            var klantcontact = await response.Content.ReadFromJsonAsync<Klantcontact>();           
           
            return klantcontact;
        }
    }
}
