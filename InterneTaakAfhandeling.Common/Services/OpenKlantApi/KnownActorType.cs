namespace InterneTaakAfhandeling.Common.Services.OpenKlantApi;

public enum KnownActorType
{
    Medewerker,
    Afdeling, 
    Groep
}

public static class KnownActorTypeExtensions
{
  
    public static KnownActorType ParseActorType(string actorTypeString)
    {
        return actorTypeString?.ToLower() switch
        {
            "medewerker" => KnownActorType.Medewerker,
            "afdeling" => KnownActorType.Afdeling,
            "groep" => KnownActorType.Groep,
            _ => throw new ArgumentException($"Invalid actor type: {actorTypeString}", nameof(actorTypeString))
        };
    }
     
    public static bool TryParseActorType(string? actorTypeString, out KnownActorType actorType)
    {
        actorType = default;
        
        if (string.IsNullOrWhiteSpace(actorTypeString))
            return false;
            
        try
        {
            actorType = ParseActorType(actorTypeString);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
