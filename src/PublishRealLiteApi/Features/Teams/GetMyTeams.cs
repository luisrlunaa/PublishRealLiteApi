using Microsoft.EntityFrameworkCore;
using PublishRealLiteApi.Common;
using PublishRealLiteApi.Infrastructure.Data;

namespace PublishRealLiteApi.Features.Teams;

public static class GetMyTeams
{
    public record Query();

    public record TeamMemberDto(int Id, string Email, string Role, decimal SharePercent, bool Accepted);

    public record Response(int Id, string Name, List<TeamMemberDto> Members);

    public class Handler(AppDbContext db, ICurrentUserService currentUser)
    {
        public async Task<List<Response>?> HandleAsync(Query query, CancellationToken ct = default)
        {
            var userId = currentUser.UserId!;

            var profile = await db.ArtistProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId && !p.IsDeleted, ct);

            if (profile == null) return null;

            var teams = await db.Teams
                .Include(t => t.Members)
                .Where(t => t.ArtistProfileId == profile.Id && !t.IsDeleted)
                .ToListAsync(ct);

            return teams.Select(t => new Response(
                t.Id,
                t.Name,
                t.Members
                    .Where(m => !m.IsDeleted)
                    .Select(m => new TeamMemberDto(m.Id, m.Email, m.Role, m.SharePercent, m.Accepted))
                    .ToList()
            )).ToList();
        }
    }
}
