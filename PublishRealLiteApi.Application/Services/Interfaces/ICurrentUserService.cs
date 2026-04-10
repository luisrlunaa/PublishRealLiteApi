namespace PublishRealLiteApi.Application.Services.Interfaces
{
    public interface ICurrentUserService
    {
        string? UserId { get; }
        string? UserName { get; }
        string? Email { get; }
        bool IsAuthenticated { get; }
        bool IsAdmin { get; }
    }
}