namespace InterneTaakAfhandeling.Common.Services.DagelijkseHerinnering;

public interface IVerlopenContactverzoekHerinneringsTemplateService
{
    HerinneringsMailContent GenereerMailContent(RecipientHerinneringData ontvanger, string baseUrl);
}
