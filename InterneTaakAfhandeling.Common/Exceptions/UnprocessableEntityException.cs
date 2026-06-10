namespace InterneTaakAfhandeling.Common.Exceptions;

public class UnprocessableEntityException : Exception
{
    public string? Code { get; set; }

    public UnprocessableEntityException(string message) : base(message) { }

    public UnprocessableEntityException(string message, string code) : base(message)
    {
        Code = code;
    }
}
