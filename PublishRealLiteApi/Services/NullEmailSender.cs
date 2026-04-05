using Microsoft.AspNetCore.Identity.UI.Services;

namespace PublishRealLiteApi.Services
{
    public class NullEmailSender : IEmailSender
    {
        private readonly ILogger<NullEmailSender> _logger;
        public NullEmailSender(ILogger<NullEmailSender> logger) => _logger = logger;

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            _logger.LogInformation("Mock email to {Email}: {Subject}", email, subject);
            return Task.CompletedTask;
        }
    }
}
