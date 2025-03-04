namespace ITA.Poller.Services.ObjectApi.Models
{
     public class ObjectResponse
    {
        public int Count { get; set; }
        public string Next { get; set; }
        public string Previous { get; set; }
        public List<ObjectResult> Results { get; set; }
    }

    public class ObjectResult
    {
        public string Url { get; set; }
        public string Uuid { get; set; }
        public string Type { get; set; }
        public ObjectRecord Record { get; set; }
    }

    public class ObjectRecord
    {
        public int Index { get; set; }
        public int TypeVersion { get; set; }
        public ObjectData Data { get; set; }
        public object Geometry { get; set; }
        public string StartAt { get; set; }
        public string EndAt { get; set; }
        public string RegistrationAt { get; set; }
        public string CorrectionFor { get; set; }
        public string CorrectedBy { get; set; }
    }

    public class ObjectData
    {
        public string Naam { get; set; }
        public string Email { get; set; }
        public string Identificatie { get; set; }
    }
}