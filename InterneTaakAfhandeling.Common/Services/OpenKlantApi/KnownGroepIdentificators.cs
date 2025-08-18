namespace InterneTaakAfhandeling.Common.Services.OpenKlantApi;

public class KnownGroepIdentificators
{
    public static readonly KnownGroepIdentificators ObjectregisterId = new()
    {
        CodeObjecttype = "grp",
        CodeRegister = "obj",
        CodeSoortObjectId = "idf"
    };
    
    public required string CodeSoortObjectId { get; init; }
    public required string CodeObjecttype { get; init; }
    public required string CodeRegister { get; init; }
}