namespace InterneTaakAfhandeling.Common.Extensions
{

    public class ApiClientOptions
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
    }

    public class OpenKlantApiOptions : ApiClientOptions
    {
    }

    public class ObjectApiOptions : ApiClientOptions
    {
    }

    public class ZakenApiOptions : ApiClientOptions
    {
        public string JwtSecretKey { get; set; } = string.Empty;

        public string ClientId { get; set; } = string.Empty;
    }
}
