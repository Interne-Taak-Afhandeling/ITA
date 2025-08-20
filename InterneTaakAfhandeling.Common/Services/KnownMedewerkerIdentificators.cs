namespace InterneTaakAfhandeling.Common.Services
{
    public class KnownMedewerkerIdentificators : IObjectRegisterId
    {

        public const string CodeObjecttypeMedewerker = "mdw";

        public static readonly KnownMedewerkerIdentificators EmailFromEntraId = new()
        {
            CodeObjecttype = CodeObjecttypeMedewerker,
            CodeRegister = "msei",
            CodeSoortObjectId = "email"
        };

        public static readonly KnownMedewerkerIdentificators EmailHandmatig = new()
        {
            CodeObjecttype = CodeObjecttypeMedewerker,
            CodeRegister = "handmatig",
            CodeSoortObjectId = "email"
        };

        public static readonly KnownMedewerkerIdentificators ObjectRegisterId = new()
        {
            CodeObjecttype = CodeObjecttypeMedewerker,
            CodeRegister = "obj",
            CodeSoortObjectId = "idf"
        };

        private KnownMedewerkerIdentificators()
        {
        }

        public required string CodeSoortObjectId { get; init; }
        public required string CodeObjecttype { get; init; }
        public required string CodeRegister { get; init; }
    }
}
