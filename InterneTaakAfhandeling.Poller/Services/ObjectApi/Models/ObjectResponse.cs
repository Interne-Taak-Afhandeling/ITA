namespace InterneTaakAfhandeling.Poller.Services.ObjectApi.Models
{
     public class ObjectResponse
    {
        public int Count { get; set; }
        public required string Next { get; set; }
        public required string Previous { get; set; }
        public required List<ObjectResult> Results { get; set; }
    }

    public class ObjectResult
    {
        public required string Url { get; set; }
        public required string Uuid { get; set; }
        public required string Type { get; set; }
        public required ObjectRecord Record { get; set; }
    }

    public class ObjectRecord
    {
        public int Index { get; set; }
        public int TypeVersion { get; set; }
        public required ObjectData Data { get; set; }
        public required object Geometry { get; set; }
        public required string StartAt { get; set; }
        public required string EndAt { get; set; }
        public required string RegistrationAt { get; set; }
        public required string CorrectionFor { get; set; }
        public required string CorrectedBy { get; set; }
    }

    public class ObjectData
    {
        public required string Naam { get; set; }
        public required string Email { get; set; }
        public required string Identificatie { get; set; }
    }
}