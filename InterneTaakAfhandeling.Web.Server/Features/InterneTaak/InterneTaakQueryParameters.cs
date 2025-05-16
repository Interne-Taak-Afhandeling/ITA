namespace InterneTaakAfhandeling.Web.Server.Features.Internetaken
{
    public class InterneTaakQueryParameters
    {
        public string? AanleidinggevendKlantcontact_Url { get; set; }
        public Guid? AanleidinggevendKlantcontact_Uuid { get; set; }

        public string? Actoren_Naam { get; set; }
        public string? Klantcontact_Nummer { get; set; }
        public Guid? Klantcontact_Uuid { get; set; }

        public string? Nummer { get; set; }

        public int? Page { get; set; }
        public int? PageSize { get; set; }

        public string? Status { get; set; }  

        public string? ToegewezenAanActor_Url { get; set; }
        public Guid? ToegewezenAanActor_Uuid { get; set; }

        public DateTime? ToegewezenOp { get; set; }
    }

}
