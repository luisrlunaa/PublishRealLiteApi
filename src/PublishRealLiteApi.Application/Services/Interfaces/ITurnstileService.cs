namespace PublishRealLiteApi.Application.Services.Interfaces
{
    public interface ITurnstileService
    {
        Task<bool> ValidateAsync(string token);
    }
}
