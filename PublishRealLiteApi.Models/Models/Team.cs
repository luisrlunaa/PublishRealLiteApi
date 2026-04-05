using System.Collections.Generic;

namespace PublishRealLiteApi.Models
{
    public class Team
    {
        public int Id { get; set; }
        public int ArtistProfileId { get; set; }
        public ArtistProfile? ArtistProfile { get; set; }
        public string Name { get; set; } = "Default Team";
        public ICollection<TeamMember> Members { get; set; } = new List<TeamMember>();
    }
}
