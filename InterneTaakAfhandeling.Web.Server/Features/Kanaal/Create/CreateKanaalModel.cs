using System.ComponentModel.DataAnnotations;

namespace InterneTaakAfhandeling.Web.Server.Features.Kanaal.Create;

public class CreateKanaalModel
{
    [StringLength(255, ErrorMessage = "Het veld Naam moet een string zijn met een maximale lengte van 255.")]
    public required string Naam { get; set; }
}
