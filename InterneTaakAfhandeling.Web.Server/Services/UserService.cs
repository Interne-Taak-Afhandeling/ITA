using InterneTaakAfhandeling.Common.Services.ObjectApi;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using InterneTaakAfhandeling.Web.Server.Middleware;

namespace InterneTaakAfhandeling.Web.Server.Services
{
    public interface IUserService
    {
        Task<List<Actor>> GetActorsByUser(string? userEmail, string? userId);
        Task<List<Internetaken>> GetInterneTakenByAssignedUser(string? userEmail, string? userId);
    }
    public class UserService(IObjectApiClient objectApiClient, IOpenKlantApiClient openKlantApiClient, ILogger<UserService> logger) : IUserService
    {
        private readonly IObjectApiClient _objectApiClient = objectApiClient;
        private readonly IOpenKlantApiClient _openKlantApiClient = openKlantApiClient;
        

        public async Task<List<Actor>> GetActorsByUser(string? userEmail, string? userId)
        {
            try
            {
                var actors = new List<Actor>();
                if (!string.IsNullOrEmpty(userEmail))
                {
                    var objectRecords = await _objectApiClient.GetObject(userEmail, "email", "handmatig", "mdw");
                    var objectId = objectRecords?.FirstOrDefault()?.Data?.Identificatie;
                    if (objectId != null)
                    {
                        var actor = await _openKlantApiClient.GetActorenByObjectidAsync(objectId);
                        if (actor != null)
                        {
                            actors.Add(actor);
                        }
                    }
                      objectRecords = await _objectApiClient.GetObject(userEmail, "email", "msei", "mdw");
                      objectId = objectRecords?.FirstOrDefault()?.Data?.Identificatie;
                    if (objectId != null)
                    {
                        var actor = await _openKlantApiClient.GetActorenByObjectidAsync(objectId);
                        if (actor != null)
                        {
                            actors.Add(actor);
                        }
                    }
                }
                if (!string.IsNullOrEmpty(userId))
                {
                   var objectRecords = await _objectApiClient.GetObject(userId, "idf", "obj", "mdw");
                   var objectId = objectRecords?.FirstOrDefault()?.Data?.Identificatie;
                    if (objectId != null)
                    {
                        var actor = await _openKlantApiClient.GetActorenByObjectidAsync(objectId);
                        if (actor != null)
                        {
                            actors.Add(actor);
                        }
                    }
                }

                return actors;

            }
            catch (Exception)
            {
                throw;
            }
        }


        public async Task<List<Internetaken>> GetInterneTakenByAssignedUser(string? userEmail, string? userId)
        {
            try
            {
                var actors = await GetActorsByUser(userEmail, userId);

                if (actors.Count == 0)
                {
                    return [];
                }
                var interneTakens = new List<Internetaken>();
                foreach (var actor in actors)
                {
                    if (!string.IsNullOrEmpty(actor.Uuid))
                    {
                        interneTakens.AddRange(await _openKlantApiClient.GetInternetakenByToegewezenAanActor(actor.Uuid));
                    }
                }
                return interneTakens;
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
