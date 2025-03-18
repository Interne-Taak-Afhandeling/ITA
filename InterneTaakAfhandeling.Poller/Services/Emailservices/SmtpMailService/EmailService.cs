using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace InterneTaakAfhandeling.Poller.Services.Emailservices.SmtpMailService;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body); 
}

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    private readonly SmtpSettings _smtpSettings;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var smtpSettingsSection = _configuration.GetSection("Email:SmtpSettings");
        _smtpSettings = smtpSettingsSection.Exists()
            ? smtpSettingsSection.Get<SmtpSettings>() ?? throw new InvalidOperationException("SMTP settings are not configured properly.")
            : throw new InvalidOperationException("SMTP settings section is missing in the configuration.");
    }

    public async Task SendEmailAsync(string to, string subject, string body)
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

            using var smtpClient = new SmtpClient(_smtpSettings.Host, _smtpSettings.Port)
            {
                Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password),
                EnableSsl = _smtpSettings.EnableSsl
            };

             
            _logger.LogInformation("Sending email to {To} via {Host}:{Port}", to[..Math.Min(to.Length, 4)], _smtpSettings.Host, _smtpSettings.Port);
             await smtpClient.SendMailAsync(mailMessage);
           
        }
        catch (SmtpException smtpEx)
        {
            _logger.LogError(smtpEx, "SMTP error occurred while sending email via {Host}:{Port}", _smtpSettings.Host, _smtpSettings.Port);
            throw new InvalidOperationException("An error occurred while sending the email. Please try again later.", smtpEx);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email via {Host}:{Port}", _smtpSettings.Host, _smtpSettings.Port);
            throw new InvalidOperationException("An unexpected error occurred while sending the email. Please try again later.", ex);
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
