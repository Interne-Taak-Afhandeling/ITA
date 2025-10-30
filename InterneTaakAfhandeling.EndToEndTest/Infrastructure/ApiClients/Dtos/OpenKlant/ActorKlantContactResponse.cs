using System.Text.Json.Serialization;

namespace InterneTaakAfhandeling.EndToEndTest.Infrastructure.ApiClients.Dtos.OpenKlant
{
    public class ActorKlantContactResponse
    {
        [JsonPropertyName("uuid")]
        public required string Uuid { get; set; }

        [JsonPropertyName("url")]
        public required string Url { get; set; }

        [JsonPropertyName("actor")]
        public required ActorResponse Actor { get; set; }

        [JsonPropertyName("klantcontact")]
        public required KlantContactResponse KlantContact { get; set; }
    }
}
