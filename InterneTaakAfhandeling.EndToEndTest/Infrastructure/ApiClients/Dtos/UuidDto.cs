using System.Text.Json.Serialization;

namespace InterneTaakAfhandeling.EndToEndTest.Infrastructure.ApiClients.Dtos
{
    public class UuidDto
    {
        [JsonPropertyName("uuid")]
        public required string Value { get; set; }
    }
}
