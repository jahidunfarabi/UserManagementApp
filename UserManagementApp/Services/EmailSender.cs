using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace UserManagementApp.Services
{
    // This class sends real emails through Gmail's SMTP server.
    // Task requirement: email sending must be asynchronous (non-blocking I/O).
    // NOTE: we intentionally await the send directly here instead of using
 
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(IConfiguration config, ILogger<EmailSender> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            try
            {
                using var client = new SmtpClient("smtp.gmail.com", 587)
                {
                    Credentials = new NetworkCredential(
                        _config["Gmail:User"],
                        _config["Gmail:AppPassword"]),
                    EnableSsl = true
                };

                var mail = new MailMessage(
                    _config["Gmail:User"]!,
                    email,
                    subject,
                    htmlMessage)
                {
                    IsBodyHtml = true
                };

                await client.SendMailAsync(mail);
                _logger.LogInformation("Email sent successfully to {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", email);
            }
        }
    }
}