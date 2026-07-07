namespace InterneTaakAfhandeling.Web.Server.Features.InternetakenOverviews.Shared.Urgentie;

public interface IUrgentieBerekenService
{
    UrgentieInfo? Bereken(DateTimeOffset? contactDatum);
}
