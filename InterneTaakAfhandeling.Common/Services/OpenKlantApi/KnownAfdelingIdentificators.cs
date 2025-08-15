namespace InterneTaakAfhandeling.Common.Services.OpenKlantApi;

public class KnownAfdelingIdentificators
{
    public static readonly KnownAfdelingIdentificators ObjectregisterId = new()
    {
        CodeObjecttype = "afd",
        CodeRegister = "obj",
        CodeSoortObjectId = "idf"
    };
    
    public required string CodeSoortObjectId { get; init; }
    public required string CodeObjecttype { get; init; }
    public required string CodeRegister { get; init; }
}