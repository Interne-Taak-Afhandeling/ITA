namespace InterneTaakAfhandeling.Web.Server.Features.InterneTaak;

public class PartijIdentificatorConfig(IConfiguration configuration)
{
    // Off by default
    public bool Tonen => configuration.GetValue<bool>("PARTIJ_IDENTIFICATOR_TONEN");
}
