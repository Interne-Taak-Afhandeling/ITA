namespace InterneTaakAfhandeling.Web.Server.Features
{
    public class ResourcesConfig(IConfiguration configuration)
    {
        private readonly IConfiguration _configuration = configuration;

        public string? Theme
        {
            get
            {
                var theme = _configuration["RESOURCES:THEME_NAAM"];
                return !string.IsNullOrWhiteSpace(theme) ? theme : null;
            }
        }
        public string? LogoUrl => Uri.TryCreate(_configuration["RESOURCES:LOGO_URL"], UriKind.Absolute, out var uri) ? uri.ToString() : null;
        public string? FaviconUrl => Uri.TryCreate(_configuration["RESOURCES:FAVICON_URL"], UriKind.Absolute, out var uri) ? uri.ToString() : null;
        public string? TokensUrl => Uri.TryCreate(_configuration["RESOURCES:DESIGN_TOKENS_URL"], UriKind.Absolute, out var uri) ? uri.ToString() : null;
        public string? FontSources
        {
            get
            {
                var arr = _configuration["RESOURCES:WEB_FONT_SOURCES"]?.Split(" ", StringSplitOptions.RemoveEmptyEntries)
                    .Where(s => Uri.TryCreate(s, UriKind.Absolute, out _))
                    .ToArray();

                var sources = string.Join(" ", arr ?? []);

                return !string.IsNullOrWhiteSpace(sources) ? sources : null;
            }
        }
    }
}
