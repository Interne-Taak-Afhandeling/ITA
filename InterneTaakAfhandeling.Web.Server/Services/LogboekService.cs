using InterneTaakAfhandeling.Common.Services.ObjectApi;
using InterneTaakAfhandeling.Common.Services.ObjectApi.Models;

namespace InterneTaakAfhandeling.Web.Server.Services
{
    public interface ILogboekService
    {        
        Task<LogboekData> AddContactmoment(Guid internetaakId);
    }

    public class LogboekService : ILogboekService
    {
        private readonly IObjectApiClient _objectenApiClient;

        public LogboekService( IObjectApiClient objectenApiClient)
        {
            _objectenApiClient = objectenApiClient;
        }

        public async Task<LogboekData> AddContactmoment(Guid internetaakId)
        {
            //1 check if a logboek for the Intenretaak already exists
            var exisistingLogboek = await  _objectenApiClient.GetLogboek(internetaakId);


            if (exisistingLogboek != null)
            {
                return exisistingLogboek;
            }

            //2 if not create it
            return await _objectenApiClient.CreateLogboekForInternetaak(internetaakId);
           

            //3 add an antry to the logboek with information about this contactmoment

        }
    }

    

}
