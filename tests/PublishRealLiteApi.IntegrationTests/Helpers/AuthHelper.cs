using System.Net.Http.Json;
using PublishRealLiteApi.DTOs;

namespace PublishRealLiteApi.IntegrationTests.Helpers;

public static class AuthHelper
{
    // Register does NOT return a token — Login is required after.
    public static async Task<string> RegisterAndGetTokenAsync(
        HttpClient client,
        string? email = null,
        string password = "Password123!")
    {
        email ??= $"user_{Guid.NewGuid():N}@test.com";

        var reg = await client.PostAsJsonAsync("/api/auth/register", new RegisterDto(email, password));
        reg.EnsureSuccessStatusCode();

        return await LoginAndGetTokenAsync(client, email, password);
    }

    public static async Task<string> LoginAndGetTokenAsync(
        HttpClient client,
        string email,
        string password = "Password123!")
    {
        var login = await client.PostAsJsonAsync("/api/auth/login", new LoginDto(email, password));
        login.EnsureSuccessStatusCode();

        var auth = await login.Content.ReadFromJsonAsync<AuthResponseDto>();
        return auth!.Token;
    }
}
