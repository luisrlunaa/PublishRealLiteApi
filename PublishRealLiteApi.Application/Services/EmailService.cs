using Microsoft.Extensions.Configuration;
using PublishRealLiteApi.Application.Services.Interfaces;
using System.Net;
using System.Net.Mail;

namespace PublishRealLiteApi.Application.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendInvitationEmailAsync(string email, string artistName, string inviteLink)
        {
            var subject = "Welcome to PublishReal - Complete Your Profile";
            var body = $"""
                <h1>Hello, {artistName}!</h1>
                <p>An administrator has created an artist profile for you on PublishReal.</p>
                <p>Please click the link below to set your password and access your account:</p>
                <p><a href='{inviteLink}'>Set Your Password & Claim Account</a></p>
                <br />
                <p>If you did not expect this invitation, please ignore this email.</p>
                """;

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // Pulling settings from appsettings.json
            var smtpHost = _config["EmailSettings:Host"];
            var smtpPort = int.Parse(_config["EmailSettings:Port"] ?? "587");
            var smtpUser = _config["EmailSettings:Username"];
            var smtpPass = _config["EmailSettings:Password"];
            var fromEmail = _config["EmailSettings:FromEmail"];

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUser, smtpPass),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail ?? "noreply@publishreal.com", "PublishReal Lite"),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };

            mailMessage.To.Add(email);

            await client.SendMailAsync(mailMessage);
        }
    }
}