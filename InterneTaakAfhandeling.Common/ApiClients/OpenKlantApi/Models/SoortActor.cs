using System.Text.Json.Serialization;

namespace InterneTaakAfhandeling.Common.ApiClients.OpenKlantApi.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter<SoortActor>))]
    public enum SoortActor
    {
        geautomatiseerde_actor,
        medewerker,
        organisatorische_eenheid
    }
}
