using System.Web;
using InterneTaakAfhandeling.Common.ApiClients.OpenKlantApi;
using InterneTaakAfhandeling.Common.ApiClients.OpenKlantApi.Models;
using Microsoft.Extensions.Logging;

namespace InterneTaakAfhandeling.Common.Services.Actors
{
    
    public interface IActorService
    {
        Task<Actor> GetActorAsync(string? uuid);
        Task<Actor?> QueryActorAsync(ActorQuery query);
    }
    
    public class ActorService : IActorService
    {
        
        private readonly IOpenKlantApiClient _openKlantApiClient;
        private readonly ILogger<ActorService> _logger;
        
        public ActorService(
            IOpenKlantApiClient openKlantApiClient,
            ILogger<ActorService> logger)
        {
            _openKlantApiClient = openKlantApiClient ?? throw new ArgumentNullException(nameof(openKlantApiClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        //todo: refactor this method to return null instead of throwing an exception when the actor is not found. It's up to the features how to deal with an actor that can't be found. The apiclient should not be opinionated on this.
        public async Task<Actor> GetActorAsync(string? uuid)
        {
            _logger.LogInformation("Fetching actor {Uuid}", uuid);
        
            var actor = await _openKlantApiClient.GetAsync<Actor>($"actoren/{uuid}");

            if (actor == null)
            {
                _logger.LogInformation("Actor not found {Uuid}", uuid);
                throw new Exception("Actor not found");
            }

            _logger.LogInformation("Successfully retrieved actor {Uuid}", uuid);

            return actor;
        }

        public async Task<Actor?> QueryActorAsync(ActorQuery query)
        {
            var queryDictionary = HttpUtility.ParseQueryString(string.Empty);
            queryDictionary["actoridentificatorCodeObjecttype"] = query.ActoridentificatorCodeObjecttype;
            queryDictionary["actoridentificatorCodeRegister"] = query.ActoridentificatorCodeRegister;
            queryDictionary["actoridentificatorCodeSoortObjectId"] = query.ActoridentificatorCodeSoortObjectId;
            queryDictionary["actoridentificatorObjectId"] = query.ActoridentificatorObjectId;
            queryDictionary["soortActor"] = query.SoortActor.ToString();

            if (query.IndicatieActief.HasValue)
            {
                queryDictionary["indicatieActief"] = query.IndicatieActief.Value ? "true" : "false";
            }

            var content = await _openKlantApiClient.GetAsync<ActorResponse>($"actoren?{queryDictionary}");
            return content?.Results.FirstOrDefault();
        }
        
        private class ActorResponse
        {
            public int Count { get; set; }
            public string? Next { get; set; }
            public string? Previous { get; set; }
            public List<Actor> Results { get; set; } = [];
        }
    }
}
