using System;

namespace PublishRealLiteApi.Models
{
    public class TeamInvite
    {
        public int Id { get; set; }
        public int TeamId { get; set; }
        public Team? Team { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public bool Used { get; set; } = false;
        // Audit/soft-delete
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
