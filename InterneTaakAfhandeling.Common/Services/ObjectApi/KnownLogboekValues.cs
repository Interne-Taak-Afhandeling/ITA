using InterneTaakAfhandeling.Common.Services.ObjectApi.Models;

namespace InterneTaakAfhandeling.Common.Services.ObjectApi.KnownLogboekValues
{
    public static class ActiviteitTypes
    {
        public static readonly string Klantcontact = "klantcontact"; 
        public static readonly string Afsluiting = "afsluiting";
    }

    public static class InternetaakObjectIdentificator
    {
        public static readonly string CodeRegister = "openklant";
        public static readonly string CodeObjectType = "internetaak";
        public static readonly string CodeSoortObjectId = "uuid";
    }

    public static class ActiviteitContactmomentObjectIdentificator
    {
        public static readonly string CodeRegister = "openklant";
        public static readonly string CodeObjectType = "klantcontact";
        public static readonly string CodeSoortObjectId = "uuid";
    }

}
 