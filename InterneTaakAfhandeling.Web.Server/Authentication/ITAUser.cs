namespace InterneTaakAfhandeling.Web.Server.Authentication
{
    public class ITAUser
    {
        public bool IsLoggedIn { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        public string[] Roles { get; set; } = [];
        public bool IsAdmin { get; set; }
    }
}
