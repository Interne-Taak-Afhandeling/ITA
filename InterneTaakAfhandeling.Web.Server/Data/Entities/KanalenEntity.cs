using System.ComponentModel.DataAnnotations;

namespace InterneTaakAfhandeling.Web.Server.Data.Entities;

public class KanalenEntity : BaseEntity
{
    [Required] [StringLength(255)] public string Naam { get; set; } = string.Empty;
}