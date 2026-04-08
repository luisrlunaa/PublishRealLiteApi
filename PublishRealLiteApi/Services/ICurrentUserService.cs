namespace PublishRealLiteApi.Services
{
    public interface ICurrentUserService
    {
        string? UserId { get; }
        string? UserName { get; }
    }
}