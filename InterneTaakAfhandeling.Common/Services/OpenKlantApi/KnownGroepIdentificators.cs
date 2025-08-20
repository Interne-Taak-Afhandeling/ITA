namespace InterneTaakAfhandeling.Common.Services.OpenKlantApi;

public class KnownGroepIdentificators : IObjectRegisterId
{
    public static readonly KnownGroepIdentificators ObjectRegisterId = new()
    {
        CodeObjecttype = "grp",
        CodeRegister = "obj",
        CodeSoortObjectId = "idf"
    };
    
    public required string CodeSoortObjectId { get; init; }
    public required string CodeObjecttype { get; init; }
    public required string CodeRegister { get; init; }
}