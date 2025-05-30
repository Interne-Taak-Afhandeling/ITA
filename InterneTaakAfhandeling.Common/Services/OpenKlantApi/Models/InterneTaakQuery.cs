﻿namespace InterneTaakAfhandeling.Web.Server.Features.Internetaken
{
    public class InterneTaakQuery
    {
        public string? AanleidinggevendKlantcontact_Url { get; set; }
        public Guid? AanleidinggevendKlantcontact_Uuid { get; set; }

        public string? Actoren__Naam { get; set; }
        public string? Klantcontact__Nummer { get; set; }
        public Guid? Klantcontact__Uuid { get; set; }

        public string? Nummer { get; set; }

        public int? Page { get; set; }
        public int? PageSize { get; set; }

        public string? Status { get; set; }  

        public string? ToegewezenAanActor__Url { get; set; }
        public Guid? ToegewezenAanActor__Uuid { get; set; }

        public DateTime? ToegewezenOp { get; set; }
    }

}
