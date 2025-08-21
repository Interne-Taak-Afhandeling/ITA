namespace InterneTaakAfhandeling.Common.Services.OpenKlantApi;

public class KnownGroepIdentificators : IObjectRegisterId
{
    public const string CodeObjecttypeGroep = "grp";

    public static readonly KnownGroepIdentificators ObjectRegisterId = new()
    {
        CodeObjecttype = CodeObjecttypeGroep,
        CodeRegister = "obj",
        CodeSoortObjectId = "idf"
    };
    
    public required string CodeSoortObjectId { get; init; }
    public required string CodeObjecttype { get; init; }
    public required string CodeRegister { get; init; }
}