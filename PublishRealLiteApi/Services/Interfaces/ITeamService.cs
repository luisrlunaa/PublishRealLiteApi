using PublishRealLiteApi.DTOs;

namespace PublishRealLiteApi.Services.Interfaces
{
    public interface ITeamService
    {
        Task<TeamDto> CreateTeamAsync(int artistProfileId, string name);
        Task InviteAsync(int teamId, string email);
        Task<IEnumerable<TeamDto>> GetTeamsForArtistAsync(int artistProfileId);
        Task AcceptInviteAsync(string token, string userEmail);
    }
}
