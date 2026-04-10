using System.Threading.Tasks;

namespace PublishRealLiteApi.Application.Services.Interfaces
{
    public interface IAuthService
    {
        string? CurrentUserId { get; }
        string? CurrentUserName { get; }
        bool IsAdmin { get; }

        Task<bool> UserHasProfileAsync();
        Task<int> GetProfileIdAsync();
    }
}
