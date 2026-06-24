namespace InterneTaakAfhandeling.Web.Server.Features.Urgentie;

public interface IUrgentieBerekenService
{
    UrgentieInfo? Bereken(DateTimeOffset? contactDatum);
}
