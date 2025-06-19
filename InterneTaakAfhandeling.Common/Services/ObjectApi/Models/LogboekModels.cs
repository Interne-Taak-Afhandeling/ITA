namespace InterneTaakAfhandeling.Common.Services.ObjectApi.Models
{
    public class LogboekModels
    {
        public required string Type { get; set; }
        public required LogboekRecord Record { get; set; }
    }

    public class LogboekRecord
    {
        public required string TypeVersion { get; set; }
        public required string StartAt { get; set; }
        public required LogboekData Data { get; set; }
    }

    public class LogboekData
    {
        public required ObjectIdentificator HeeftBetrekkingOp { get; set; }
        public required List<Activiteit> Activiteiten { get; set; }
    }

    public class Activiteit
    {
        public required string Datum { get; set; }
        public required string Type { get; set; }
        public required string Omschrijving { get; set; }
        public ObjectIdentificator? HeeftBetrekkingOp { get; set; }
        
    }

    public class ObjectIdentificator
    {
        public required string CodeRegister { get; set; }
        public required string CodeObjecttype { get; set; }
        public required string CodeSoortObjectId { get; set; }
        public required string ObjectId { get; set; }
    }
    public class LogboekOptions
    {  
        public required string Type { get; init; }
        public required string TypeVersion { get; init; }
        public required string CodeObjectType { get; init; }
        public required string CodeRegister { get; init; }
        public required string CodeSoortObjectId { get; init; }
    }
}
