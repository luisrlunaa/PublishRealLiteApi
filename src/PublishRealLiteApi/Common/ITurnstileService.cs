namespace PublishRealLiteApi.Common;

public interface ITurnstileService
{
    Task<bool> ValidateAsync(string token);
}
