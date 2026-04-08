using System;

namespace PublishRealLiteApi.Models
{
    public class Track
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid ReleaseId { get; set; }
        public Release? Release { get; set; }

        public int Position { get; set; }
        public string Title { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        // Soft delete and audit
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
