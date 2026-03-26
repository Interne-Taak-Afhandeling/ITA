using System.ComponentModel.DataAnnotations;

namespace InterneTaakAfhandeling.Web.Server.Config;

public class AfhandeltermijnOptions
{
    public const string SectionName = "Afhandeltermijn";

    [Range(1, 365)]
    public int WerkdagenTermijn { get; set; } = 5;
}
