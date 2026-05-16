namespace PublishRealLiteApi.Infrastructure.Services;

public interface IEmailService
{
    Task SendInvitationEmailAsync(string email, string artistName, string inviteLink);
    Task SendEmailAsync(string email, string subject, string htmlMessage);
}
