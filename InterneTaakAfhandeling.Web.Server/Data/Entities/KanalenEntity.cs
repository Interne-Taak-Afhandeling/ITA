using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace InterneTaakAfhandeling.Web.Server.Data.Entities;

[Index(nameof(Naam), IsUnique = true)]
public class KanalenEntity : BaseEntity
{
    [StringLength(255)]
    public required string Naam { get; set; }
}