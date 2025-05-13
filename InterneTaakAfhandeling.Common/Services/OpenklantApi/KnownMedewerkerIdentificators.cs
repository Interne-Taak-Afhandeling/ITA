namespace InterneTaakAfhandeling.Common.Services.OpenklantApi
{
    public class KnownMedewerkerIdentificators
    {
        public static readonly KnownMedewerkerIdentificators EmailFromEntraId = new()
        {
            CodeObjecttype = "mdw",
            CodeRegister = "msei",
            CodeSoortObjectId = "email"
        };

        public static readonly KnownMedewerkerIdentificators EmailHandmatig = new()
        {
            CodeObjecttype = "mdw",
            CodeRegister = "handmatig",
            CodeSoortObjectId = "email"
        };

        public static readonly KnownMedewerkerIdentificators ObjectregisterId = new()
        {
            CodeObjecttype = "mdw",
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
