using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace ITA.Poller.Services.Openklant.Models
{

    public class Internetaken
    {
        public int Count { get; set; }
        public string? Next { get; set; }
        public string? Previous { get; set; }
        public List<InternetakenItem> Results { get; set; } = new();
    }


}
