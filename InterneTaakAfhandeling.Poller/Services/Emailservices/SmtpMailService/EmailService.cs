using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ITA.Poller.Services.Emailservices.SmtpMailService;

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

        _smtpSettings = _configuration.GetSection("Email:SmtpSettings").Get<SmtpSettings>();
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
                EnableSsl = false
            };

            _logger.LogInformation("Sending email to {To} via {Host}:{Port}", to, _smtpSettings.Host, _smtpSettings.Port);
            await smtpClient.SendMailAsync(mailMessage);
            _logger.LogInformation("Email sent successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email via {Host}:{Port}", _smtpSettings.Host, _smtpSettings.Port);
            throw;
        }
    }
}
