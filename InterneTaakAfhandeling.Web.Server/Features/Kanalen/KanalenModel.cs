using System.ComponentModel.DataAnnotations;

namespace InterneTaakAfhandeling.Web.Server.Features.Kanalen;

public class CreateKanalenModel
{
    [StringLength(255, MinimumLength = 3,ErrorMessage ="Het veld Naam moet een string zijn met een minimale lengte van 3 en een maximale lengte van 255.\"")]
    public required string Name { get; set; }
}