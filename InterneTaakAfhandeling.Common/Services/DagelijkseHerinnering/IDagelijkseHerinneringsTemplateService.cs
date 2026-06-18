namespace InterneTaakAfhandeling.Common.Services.DagelijkseHerinnering;

public interface IDagelijkseHerinneringsTemplateService
{
    HerinneringsMailContent GenereerMailContent(RecipientHerinneringData ontvanger, string baseUrl);
}
