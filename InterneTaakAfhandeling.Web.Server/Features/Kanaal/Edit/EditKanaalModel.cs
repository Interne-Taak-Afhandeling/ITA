using System.ComponentModel.DataAnnotations;

namespace InterneTaakAfhandeling.Web.Server.Features.Kanaal.Edit;

public class EditKanaalModel
{
    [Required(ErrorMessage = "Het veld Naam is verplicht.")]
    [StringLength(255, ErrorMessage = "Het veld Naam moet een string zijn met een maximale lengte van 255.")]
    public required string Naam { get; set; }
}
