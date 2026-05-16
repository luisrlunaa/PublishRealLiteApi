using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace PublishRealLiteApi.Common;

public class TurnstileService(HttpClient http, IConfiguration config) : ITurnstileService
{
    private readonly string _secretKey = config["Turnstile:SecretKey"]!;

    public async Task<bool> ValidateAsync(string token)
    {
        var response = await http.PostAsync(
            "https://challenges.cloudflare.com/turnstile/v0/siteverify",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["secret"] = _secretKey,
                ["response"] = token
            })
        );

        var json = await response.Content.ReadFromJsonAsync<TurnstileResponse>();
        return json?.Success ?? false;
    }

    private record TurnstileResponse([property: JsonPropertyName("success")] bool Success);
}
