using InterneTaakAfhandeling.Common.Services.ObjectApi;

namespace InterneTaakAfhandeling.Web.Server.Services
{
    public interface ILogboekService
    {        LogboekEntry AddContactmoment(Guid internetaakId);
    }

    public class LogboekService : ILogboekService
    {
        private readonly IObjectApiClient _objectenApiClient;

        public LogboekService( IObjectApiClient objectenApiClient)
        {
            _objectenApiClient = objectenApiClient;
        }

        public LogboekEntry AddContactmoment(Guid internetaakId)
        {
            //1 check if a logboek for the Intenretaak already exists

            //2 if not create it
            _objectenApiClient.CreateLogboekForInternetaak(internetaakId);

            //3 add an antry to the logboek with information about this contactmoment

            return new LogboekEntry();
        }
    }


    public class LogboekEntry
    {


    }

}
