using System;
using System.Collections.Generic;
using System.Text;

namespace PublishRealLiteApi.Application.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendInvitationEmailAsync(string email, string artistName, string inviteLink);

        Task SendEmailAsync(string email, string subject, string htmlMessage);
    }
}
