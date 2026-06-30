namespace InterneTaakAfhandeling.Poller.Features.VerlopenContactverzoekHerinnering;

public interface IVerlopenContactverzoekHerinneringsTemplateService
{
    HerinneringsMailContent GenereerMailContent(int aantalCvs, int maxWerkdagen, bool isMedewerker, string baseUrl);
}
