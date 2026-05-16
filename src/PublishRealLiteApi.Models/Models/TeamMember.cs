using System;

namespace PublishRealLiteApi.Models
{
    public class TeamMember
    {
        public int Id { get; set; }
        public int TeamId { get; set; }
        public Team? Team { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = "Viewer"; // Viewer, Editor, Manager
        public decimal SharePercent { get; set; } = 0m;
        public bool Accepted { get; set; } = false;
        // Audit/soft-delete
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
