using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace InterneTaakAfhandeling.Common.Services.OpenklantApi.Models
{
        public class KlantcontactRequest
        {

            public string? Kanaal { get; set; }

            public string? Onderwerp { get; set; }

            public string? Inhoud { get; set; }

            public bool? IndicatieContactGelukt { get; set; }

            public string? Taal { get; set; }

            public bool? Vertrouwelijk { get; set; }

            [Required]
            public DateTimeOffset PlaatsgevondenOp { get; set; }
        }
    }