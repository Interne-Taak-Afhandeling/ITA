namespace InterneTaakAfhandeling.Web.Server.Authentication
{
    public class AuthOptions
    {
        public string Authority { get; set; } = "";
        public string ClientId { get; set; } = "";
        public string ClientSecret { get; set; } = "";
        public string ITASystemAccessRole { get; set; } = "";
        public string? RoleClaimType { get; set; }
        public string? NameClaimType { get; set; }
        public string? ObjectregisterMedewerkerIdClaimType { get; set; }
        public string? EmailClaimType { get; internal set; }
    }
}
