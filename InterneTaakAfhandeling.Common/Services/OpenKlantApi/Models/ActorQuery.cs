namespace InterneTaakAfhandeling.Common.Services.OpenklantApi.Models
{
    public class ActorQuery
    {
        public required string ActoridentificatorCodeObjecttype { get; init; }
        public required string ActoridentificatorCodeRegister { get; init; }
        public required string ActoridentificatorCodeSoortObjectId { get; init; }
        public required string ActoridentificatorObjectId { get; init; }
        public bool? IndicatieActief { get; init; }
        public required SoortActor SoortActor { get; init; }
    }
}
