using System.ComponentModel.DataAnnotations;

namespace InterneTaakAfhandeling.Common.Services.Emailservices.SmtpMailService;

public class SmtpSettings
{
    [Required(ErrorMessage = "SMTP Host is required")]
    public string Host { get; set; } = string.Empty;

    [Range(1, 65535, ErrorMessage = "SMTP Port must be between 1 and 65535")]
    public int Port { get; set; }

    [Required(ErrorMessage = "SMTP Username is required")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "SMTP Password is required")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "From Email is required")]
    [EmailAddress(ErrorMessage = "From Email must be a valid email address")]
    public string FromEmail { get; set; } = string.Empty;

    public bool EnableSsl { get; set; } = true;
}