using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models
{
    public class PartijQuery
    {
        public string CodeObjecttype { get; set; }
        public string CodeSoortObjectId { get; set; }
        public string ObjectId { get; set; }
        public string CodeRegister { get; set; }






        // public string BuildQueryString()
        // {
        //     var parameters = new List<string>();

        //     if (!string.IsNullOrEmpty(Onderwerp)) parameters.Add($"onderwerp={Uri.EscapeDataString(Onderwerp)}");

        //     // Add other properties as needed

        //     return string.Join("&", parameters);
        // }


    }
}
