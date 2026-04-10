namespace PublishRealLiteApi.Services.Interfaces
{
    public interface ICurrentUserService
    {
        string? UserId { get; }
        string? UserName { get; }
        string? Email { get; }
        bool IsAdmin { get; }
    }
}
