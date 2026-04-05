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
    }
}
