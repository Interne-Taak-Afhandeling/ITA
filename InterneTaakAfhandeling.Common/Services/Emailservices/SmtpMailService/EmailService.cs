using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace InterneTaakAfhandeling.Common.Services.Emailservices.SmtpMailService;

public interface IEmailService
{
    Task<SmtpResult> SendEmailAsync(string to, string subject, string body); 
}

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly SmtpSettings _smtpSettings;

    public EmailService(IOptions<SmtpSettings> smtpOptions, ILogger<EmailService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _smtpSettings = smtpOptions.Value;
    }

    public async Task<SmtpResult> SendEmailAsync(string to, string subject, string body)
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
            
            return new SmtpResult { Success = true, Message = "Email sent successfully" };
        }
        catch (SmtpException smtpEx)
        {
            _logger.LogError(smtpEx, "SMTP error occurred while sending email via {Host}:{Port}", _smtpSettings.Host, _smtpSettings.Port);
            return new SmtpResult { Success = false, Message = $"SMTP error: {smtpEx.Message}" };
         }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email via {Host}:{Port}", _smtpSettings.Host, _smtpSettings.Port);
            return new SmtpResult { Success = false, Message = $"Email sending failed: {ex.Message}" };
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
