using System.Text.Json.Serialization;

namespace InterneTaakAfhandeling.EndToEndTest.Infrastructure.ApiClients.Dtos.OpenKlant
{
    public class KlantContactResponse
    {
        [JsonPropertyName("uuid")]
        public required string Uuid { get; set; }
    }
}
