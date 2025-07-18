namespace InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models
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



   

    public static class InterneTaakExtensions
    {
    
        public static string BuildQueryString(this InterneTaakQuery query)
        {
            var parameters = new List<string>();

            if (query.Status != null) parameters.Add($"status={Uri.EscapeDataString(query.Status)}");
            
            if (query.Page.HasValue)
                parameters.Add($"page={query.Page.Value}");

            if (query.PageSize.HasValue)
                parameters.Add($"pageSize={query.PageSize.Value}");

            if (!string.IsNullOrEmpty(query.Nummer))
                parameters.Add($"nummer={Uri.EscapeDataString(query.Nummer)}");

            if (!string.IsNullOrEmpty(query.AanleidinggevendKlantcontact_Url))
                parameters.Add($"aanleidinggevendKlantcontact__url={Uri.EscapeDataString(query.AanleidinggevendKlantcontact_Url)}");

            if (query.AanleidinggevendKlantcontact_Uuid.HasValue)
                parameters.Add($"aanleidinggevendKlantcontact__uuid={query.AanleidinggevendKlantcontact_Uuid.Value}");

            if (!string.IsNullOrEmpty(query.Actoren__Naam))
                parameters.Add($"actoren__naam={Uri.EscapeDataString(query.Actoren__Naam)}");

            if (!string.IsNullOrEmpty(query.Klantcontact__Nummer))
                parameters.Add($"klantcontact__nummer={Uri.EscapeDataString(query.Klantcontact__Nummer)}");

            if (query.Klantcontact__Uuid.HasValue)
                parameters.Add($"klantcontact__uuid={query.Klantcontact__Uuid.Value}");

            if (!string.IsNullOrEmpty(query.ToegewezenAanActor__Url))
                parameters.Add($"toegewezenAanActor__url={Uri.EscapeDataString(query.ToegewezenAanActor__Url)}");

            if (query.ToegewezenAanActor__Uuid.HasValue)
                parameters.Add($"toegewezenAanActor__uuid={query.ToegewezenAanActor__Uuid.Value}");

            if (query.ToegewezenOp.HasValue)
                parameters.Add($"toegewezenOp={query.ToegewezenOp.Value:yyyy-MM-ddTHH:mm:ss.fffZ}");

            return string.Join("&", parameters);
        }

    }
}