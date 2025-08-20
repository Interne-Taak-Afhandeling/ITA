namespace InterneTaakAfhandeling.Common.Services.OpenKlantApi;

public class KnownAfdelingIdentificators : IObjectRegisterId
{
    public const string CodeObjecttypeAfdeling = "afd";

    public static readonly KnownAfdelingIdentificators ObjectRegisterId = new()
    {
        CodeObjecttype = CodeObjecttypeAfdeling,
        CodeRegister = "obj",
        CodeSoortObjectId = "idf"
    };
    
    public required string CodeSoortObjectId { get; init; }
    public required string CodeObjecttype { get; init; }
    public required string CodeRegister { get; init; }
}