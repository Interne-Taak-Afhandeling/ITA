using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace InterneTaakAfhandeling.Web.Server.Services.OpenKlantApi.Models
{

    public class InternetakenResponse
    {
        public int Count { get; set; }
        public string? Next { get; set; }
        public string? Previous { get; set; }
        public List<Internetaken> Results { get; set; } = new();
    }


}
