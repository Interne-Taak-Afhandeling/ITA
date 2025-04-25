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
          Task<Actor?> GetActorByEmail(string userEmail);
          Task<Klantcontact?> GetKlantcontactAsync(string uuid);
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
