namespace InterneTaakAfhandeling.Web.Server.Features.InternetakenOverviews.Shared.Urgentie;

public class UrgentieOptions
{
    public const string SectionName = "Urgentie";

    public double AfhandeltermijnUren { get; set; } = 48;
    public double BijnaVerlopenDrempelUren { get; set; } = 6;
}
