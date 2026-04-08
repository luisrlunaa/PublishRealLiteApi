using System;
using System.Collections.Generic;

namespace PublishRealLiteApi.Models
{
    public class ArtistProfile
    {
        public int Id { get; set; }

        // FK to Identity user (string Id) 
        public string UserId { get; set; } = string.Empty;

        // Navigation to the Identity user is defined in the infrastructure project (AppUser).
        // The navigation property is not declared here to keep the Models project decoupled from the Identity type.
        public string ArtistName { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string? SocialLinksJson { get; set; } // JSON string for flexible links 

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        // Soft delete + audit
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public ICollection<Release> Releases { get; set; } = new List<Release>();
        public ICollection<ArtistVideo> Videos { get; set; } = new List<ArtistVideo>();
    }
}