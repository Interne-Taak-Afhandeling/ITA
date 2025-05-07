namespace InterneTaakAfhandeling.Common.Services.OpenklantApi
{
    public class KnownMedewerkerIdentificators
    {
        public static readonly KnownMedewerkerIdentificator EmailFromEntraId = new()
        {
            CodeObjecttype = "mdw",
            CodeRegister = "msei",
            CodeSoortObjectId = "email"
        };

        public static readonly KnownMedewerkerIdentificator EmailHandmatig = new()
        {
            CodeObjecttype = "mdw",
            CodeRegister = "handmatig",
            CodeSoortObjectId = "email"
        };

        public static readonly KnownMedewerkerIdentificator IdFromObjectRegistration = new()
        {
            CodeObjecttype = "mdw",
            CodeRegister = "obj",
            CodeSoortObjectId = "idf"
        };
    }

    public class KnownMedewerkerIdentificator
    {
        public required string CodeSoortObjectId { get; init; }
        public required string CodeObjecttype { get; init; }
        public required string CodeRegister { get; init; }
    }
}
