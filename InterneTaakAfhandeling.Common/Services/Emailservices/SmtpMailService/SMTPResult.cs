namespace InterneTaakAfhandeling.Common.Services.Emailservices.SmtpMailService;

public class SmtpResult
{
    public bool Success { get; init; }
    public required string Message { get; init; }
}