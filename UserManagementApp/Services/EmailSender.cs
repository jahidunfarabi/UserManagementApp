using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace UserManagementApp.Services
{
    // This class sends real emails through Brevo's HTTP API instead of SMTP.
    // NOTE: we switched from SMTP to an HTTP API because some cloud hosts
    // (like Render's free tier) block or heavily restrict outbound SMTP
    // ports (like 587), which caused emails to silently hang or fail.
    // HTTPS (port 443), which this API uses, is virtually never blocked.
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailSender> _logger;
        private static readonly HttpClient _httpClient = new HttpClient();

        public EmailSender(IConfiguration config, ILogger<EmailSender> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            try
            {
                var apiKey = _config["Brevo:ApiKey"];
                var senderEmail = _config["Brevo:SenderEmail"];

                var payload = new
                {
                    sender = new { email = senderEmail, name = "UserManagementApp" },
                    to = new[] { new { email = email } },
                    subject = subject,
                    htmlContent = htmlMessage
                };

                var json = JsonSerializer.Serialize(payload);
                using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.brevo.com/v3/smtp/email")
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
                request.Headers.Add("api-key", apiKey);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Email sent successfully to {Email}", email);
                }
                else
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to send email to {Email}. Status: {Status}. Body: {Body}",
                        email, response.StatusCode, errorBody);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", email);
            }
        }
    }
}