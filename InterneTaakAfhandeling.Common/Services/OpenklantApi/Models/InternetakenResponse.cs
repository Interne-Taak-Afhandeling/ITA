 
namespace InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models
{

    public class InternetakenResponse
    {
        public int Count { get; set; }
        public string? Next { get; set; }
        public string? Previous { get; set; }
        public List<Internetaken> Results { get; set; } = new();
    }


}
