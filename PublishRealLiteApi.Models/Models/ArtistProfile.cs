using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

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
        public ICollection<Release> Releases { get; set; } = new List<Release>();
        public ICollection<ArtistVideo> Videos { get; set; } = new List<ArtistVideo>();
    }
}