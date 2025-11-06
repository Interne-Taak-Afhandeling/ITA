using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models
{
    public class KlantcontactQuery
    {
        public string? Onderwerp { get; set; }





        public string BuildQueryString()
        {
            var parameters = new List<string>();

            if (!string.IsNullOrEmpty(Onderwerp)) parameters.Add($"onderwerp={Uri.EscapeDataString(Onderwerp)}");

            // Add other properties as needed

            return string.Join("&", parameters);
        }


    }
}
