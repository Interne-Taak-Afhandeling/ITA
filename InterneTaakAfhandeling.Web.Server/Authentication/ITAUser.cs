namespace InterneTaakAfhandeling.Web.Server.Authentication
{
    public class ITAUser
    {
        public bool IsLoggedIn { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string[] Roles { get; set; } = [];
        public bool IsAdmin { get; set; }
    }
}
