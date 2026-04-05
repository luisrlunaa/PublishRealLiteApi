using PublishRealLiteApi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PublishRealLiteApi.Application.Repositories.Interfaces
{
    public interface ITeamRepository
    {
        Task<IEnumerable<Team>> GetByArtistAsync(int artistProfileId);
        Task<Team?> GetByIdAsync(int id, int artistProfileId);
        Task<Team> AddAsync(Team team);
        Task<bool> UpdateAsync(Team team);
        Task<bool> DeleteAsync(int id, int artistProfileId);

        // Optional member management
        Task AddMemberAsync(TeamMember member);
    }
}
