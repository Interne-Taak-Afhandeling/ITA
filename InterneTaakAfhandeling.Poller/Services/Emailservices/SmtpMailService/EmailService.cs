using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace InterneTaakAfhandeling.Poller.Services.Emailservices.SmtpMailService;

public interface IEmailService
{
    Task SendEmailAsync(string subject, string body);
}

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    private readonly string _toEmail;
    private readonly SmtpSettings _smtpSettings;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));


        // Load and validate configuration
        _toEmail = _configuration.GetValue<string>("Email:To")
            ?? throw new InvalidOperationException("Email:To configuration is missing");

        _smtpSettings = new SmtpSettings
        {
            Host = _configuration.GetValue<string>("Email:SmtpSettings:Host") ?? throw new InvalidOperationException("Email:SmtpSettings:Host configuration is missing"),
            Port = _configuration.GetValue<int>("Email:SmtpSettings:Port"),
            Username = _configuration.GetValue<string>("Email:SmtpSettings:Username") ?? throw new InvalidOperationException("Email:SmtpSettings:Username configuration is missing"),
            Password = _configuration.GetValue<string>("Email:SmtpSettings:Password") ?? throw new InvalidOperationException("Email:SmtpSettings:Password configuration is missing"),
            FromEmail = _configuration.GetValue<string>("Email:SmtpSettings:FromEmail") ?? throw new InvalidOperationException("Email:SmtpSettings:FromEmail configuration is missing")
        };


    }

    public async Task SendEmailAsync(string subject, string body)
    {
        try
        {
            using var client = new SmtpClient
            {
                Host = _smtpSettings.Host,
                Port = _smtpSettings.Port,
                EnableSsl = false,
                Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password)
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_smtpSettings.FromEmail),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            mailMessage.To.Add(_toEmail);

            _logger.LogInformation("Sending email to {To} via {Host}:{Port}", _toEmail, _smtpSettings.Host, _smtpSettings.Port);
            await client.SendMailAsync(mailMessage);
            _logger.LogInformation("Email sent successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email via {Host}:{Port}", _smtpSettings.Host, _smtpSettings.Port);
            throw;
        }
    }

}

