using InterneTaakAfhandeling.Common.Services.ObjectApi;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using InterneTaakAfhandeling.Web.Server.Middleware;

namespace InterneTaakAfhandeling.Web.Server.Services
{
    public interface IUserService
    {
        Task<Actor?> GetActorByEmail(string? userEmail);
        Task<List<Internetaken>> GetInterneTakenByAssignedUser(string? userEmail);
    }
    public class UserService( IObjectApiClient objectApiClient, IOpenKlantApiClient openKlantApiClient, ILogger<UserService> logger) : IUserService
    {
        private readonly IObjectApiClient _objectApiClient = objectApiClient;
        private readonly IOpenKlantApiClient _openKlantApiClient = openKlantApiClient;
        private readonly ILogger<UserService> _logger = logger;


        public async Task<Actor?> GetActorByEmail(string? userEmail)
        {
            try
            {
                var objectRecords = await _objectApiClient.GetObjectsByIdentificatie(userEmail);                 

                if (objectRecords.Count > 1)
                    throw new ConflictException("Multiple object records found for the current user.",
                                                  code: "MULTIPLE_OBJECT_RECORDS_FOUND");

                var objectId = objectRecords?.FirstOrDefault()?.Data?.Identificatie;
                if (objectId != null)
                {
                    return await _openKlantApiClient.GetActorenByObjectidAsync(objectId);
                } 
                    return null;  
              
               
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
                var actor = await GetActorByEmail(userEmail);
                 
                if (actor == null || string.IsNullOrEmpty(actor.Uuid))
                {
                    return [];
                }

                return await _openKlantApiClient.GetInternetakenByToegewezenAanActor(actor.Uuid);
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
