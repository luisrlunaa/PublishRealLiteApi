using Microsoft.Extensions.Configuration;
using Resend;

namespace PublishRealLiteApi.Infrastructure.Services
{
    public class ResendEmailService : IEmailService
    {
        private readonly IResend _resend;
        private readonly string _fromEmail;

        public ResendEmailService(IResend resend, IConfiguration config)
        {
            _resend = resend;
            _fromEmail = config["Resend:FromEmail"] ?? "onboarding@resend.dev";
        }

        public async Task SendInvitationEmailAsync(string email, string artistName, string inviteLink)
        {
            var htmlBody = $"""
                <h1>Hello, {artistName}!</h1>
                <p>An administrator has created an artist profile for you on PublishReal.</p>
                <p>Please click the link below to set your password and access your account:</p>
                <p><a href='{inviteLink}'>Set Your Password &amp; Claim Account</a></p>
                <br />
                <p>If you did not expect this invitation, please ignore this email.</p>
                """;

            await SendEmailAsync(email, "Welcome to PublishReal - Complete Your Profile", htmlBody);
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var message = new EmailMessage
            {
                From = _fromEmail,
                Subject = subject,
                HtmlBody = htmlMessage
            };
            message.To.Add(email);

            await _resend.EmailSendAsync(message);
        }
    }
}
