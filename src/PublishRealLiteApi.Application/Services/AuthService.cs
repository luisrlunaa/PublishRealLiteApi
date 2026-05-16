using PublishRealLiteApi.Application.Repositories.Interfaces;
using PublishRealLiteApi.Application.Services.Interfaces;

namespace PublishRealLiteApi.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly ICurrentUserService _currentUser;
        private readonly IArtistProfileRepository _artistRepo;

        public AuthService(ICurrentUserService currentUser, IArtistProfileRepository artistRepo)
        {
            _currentUser = currentUser;
            _artistRepo = artistRepo;
        }

        public string? CurrentUserId => _currentUser.UserId;
        public string? CurrentUserName => _currentUser.UserName;
        public bool IsAdmin => _currentUser.IsAdmin;

        public async Task<bool> UserHasProfileAsync()
        {
            var uid = _currentUser.UserId;
            if (uid == null) return false;
            return await _artistRepo.ExistsForUserAsync(uid);
        }

        public async Task<int> GetProfileIdAsync()
        {
            var uid = _currentUser.UserId ?? throw new System.InvalidOperationException("No user");
            var profile = await _artistRepo.GetByUserIdAsync(uid);
            if (profile == null) throw new System.InvalidOperationException("No profile");
            return profile.Id;
        }
    }
}