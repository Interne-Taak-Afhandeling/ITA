using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace InterneTaakAfhandeling.Common.Services.Emailservices.SmtpMailService;

public interface IEmailService
{
    Task<EmailResult> SendEmailAsync(string to, string subject, string body);
    bool IsConfiguredCorrectly();
}

public class EmailService(IOptions<SmtpSettings> smtpOptions, ILogger<EmailService> logger)
    : IEmailService
{
    private readonly ILogger<EmailService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly SmtpSettings _smtpSettings = smtpOptions.Value;

    public bool IsConfiguredCorrectly() => !string.IsNullOrEmpty(_smtpSettings.Host)
        && _smtpSettings.Port > 0
        && !string.IsNullOrEmpty(_smtpSettings.FromEmail);

    public async Task<EmailResult> SendEmailAsync(string to, string subject, string body)
    {
        try
        {
            var mailMessage = new MailMessage
            {
                From = new MailAddress(_smtpSettings.FromEmail),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            mailMessage.To.Add(to);

            using var smtpClient = new SmtpClient(_smtpSettings.Host, _smtpSettings.Port);
            smtpClient.Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password);
            smtpClient.EnableSsl = _smtpSettings.EnableSsl;


            _logger.LogInformation("Sending email to {To} via {Host}:{Port}", to[..Math.Min(to.Length, 4)], _smtpSettings.Host, _smtpSettings.Port);
            await smtpClient.SendMailAsync(mailMessage);
            return new EmailResult { Success = true };
        }
        catch (SmtpException smtpEx)
        {
            _logger.LogError(smtpEx, "SMTP error occurred while sending email via {Host}:{Port}", _smtpSettings.Host, _smtpSettings.Port);
            return new EmailResult { Success = false, ErrorMessage = smtpEx.Message };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email via {Host}:{Port}", _smtpSettings.Host, _smtpSettings.Port);
            return new EmailResult { Success = false, ErrorMessage = ex.Message };
        }
    }
    public static bool IsValidEmail(string email)
    {
        try
        {
            return new MailAddress(email).Address == email;
        }
        catch
        {
            return false;
        }
    }
}
