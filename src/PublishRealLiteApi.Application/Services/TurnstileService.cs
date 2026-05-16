using Microsoft.Extensions.Configuration;
using PublishRealLiteApi.Application.Services.Interfaces;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace PublishRealLiteApi.Application.Services
{
    public class TurnstileService : ITurnstileService
    {
        private readonly HttpClient _http;
        private readonly string _secretKey;

        public TurnstileService(HttpClient http, IConfiguration config)
        {
            _http = http;
            _secretKey = config["Turnstile:SecretKey"]!;
        }

        public async Task<bool> ValidateAsync(string token)
        {
            var response = await _http.PostAsync(
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
}
