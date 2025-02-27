using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ITA.Poller.Services.Openklant.Models
{

    public class InternetakenItem
    {
        public required string Uuid { get; set; }

        public required string Url { get; set; }

        public required string Nummer { get; set; }

        public required string GevraagdeHandeling { get; set; }

        public required string Status { get; set; }

       public required DateTime ToegewezenOp { get; set; }

        public string? Toelichting { get; set; }

        // Add constructor with default values for required properties
        public InternetakenItem()
        {
            Uuid = string.Empty;
            Url = string.Empty;
            Nummer = string.Empty;
            GevraagdeHandeling = string.Empty;
            Status = string.Empty;
        }
    }
}
