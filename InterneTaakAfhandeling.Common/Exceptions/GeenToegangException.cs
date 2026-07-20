namespace InterneTaakAfhandeling.Common.Exceptions;

public class GeenToegangException : Exception
{
    public string Code { get; } = "CONTACTVERZOEK_GEEN_TOEGANG";

    public GeenToegangException(string message) : base(message) { }
}
