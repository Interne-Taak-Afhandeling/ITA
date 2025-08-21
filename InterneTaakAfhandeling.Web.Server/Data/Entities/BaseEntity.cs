using System.ComponentModel.DataAnnotations;

namespace InterneTaakAfhandeling.Web.Server.Data.Entities;

public class BaseEntity
{
    [Key]
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; } 

    public DateTimeOffset UpdatedAt { get; set; }
}