using System;
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
        // Soft delete and audit
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
