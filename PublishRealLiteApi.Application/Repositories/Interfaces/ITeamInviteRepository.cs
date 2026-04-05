using PublishRealLiteApi.Models;
using System.Threading.Tasks;

namespace PublishRealLiteApi.Application.Repositories.Interfaces
{
    public interface ITeamInviteRepository
    {
        Task<TeamInvite> AddAsync(TeamInvite invite);
        Task<TeamInvite?> GetByTokenAsync(string token);
        Task<bool> UpdateAsync(TeamInvite invite);
    }
}
