using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace UserManagementApp.Services
{
    // This class sends real emails through Gmail's SMTP server.
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
                    EnableSsl = true,
                    // IMPORTANT: cap how long we wait for the SMTP server to respond.
                    // Without this, a slow/unresponsive connection could hang the
                    // entire HTTP request (and the user's browser) indefinitely.
                    Timeout = 15000 // 15 seconds, in milliseconds
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
                // If sending fails or times out, we log it but do NOT let it
                // crash or hang the registration flow. The user should still
                // get redirected and signed in even if the email had trouble.
                _logger.LogError(ex, "Failed to send email to {Email}", email);
            }
        }
    }
}