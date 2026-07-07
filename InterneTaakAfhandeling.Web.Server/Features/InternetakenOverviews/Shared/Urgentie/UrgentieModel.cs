namespace InterneTaakAfhandeling.Web.Server.Features.InternetakenOverviews.Shared.Urgentie;

public enum UrgentieStatus
{
    BinnenTermijn,
    BijnaVerlopen,
    Verlopen
}

public class UrgentieInfo
{
    public required string Status { get; init; }
    public required DateTimeOffset Streefdatum { get; init; }
    public required double ResterendeUren { get; init; }
}
