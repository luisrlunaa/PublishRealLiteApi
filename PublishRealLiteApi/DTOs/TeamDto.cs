namespace PublishRealLiteApi.DTOs
{
    public class TeamDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public IEnumerable<TeamMemberDto> Members { get; set; } = Array.Empty<TeamMemberDto>();
    }
    public class TeamMemberDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public decimal SharePercent { get; set; }
        public bool Accepted { get; set; }
    }

}
