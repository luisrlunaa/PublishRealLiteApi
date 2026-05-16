namespace PublishRealLiteApi.DTOs
{
    public record RegisterDto(string Email, string Password, string TurnstileToken);
    public record LoginDto(string Email, string Password, string TurnstileToken);
    public record AuthResponseDto(string Token, string Email);
}
