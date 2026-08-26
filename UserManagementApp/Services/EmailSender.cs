using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace UserManagementApp.Services
{
    // This class sends real emails through Gmail's SMTP server.
    // Task requirement: email must be sent asynchronously, meaning the user
    // should NOT have to wait for the email to finish sending before getting
    // a response back from the registration form.
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(IConfiguration config, ILogger<EmailSender> logger)
        {
            _config = config;
            _logger = logger;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // IMPORTANT: we use Task.Run here so the actual email sending happens
            // in the background. This method returns immediately, and the caller
            // (e.g. the Register page) does not block waiting for the email to send.
            _ = Task.Run(async () =>
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
                    // If sending fails, we don't want to crash the app.
                    // We just log the error so we can debug it later.
                    _logger.LogError(ex, "Failed to send email to {Email}", email);
                }
            });

            return Task.CompletedTask;
        }
    }
}