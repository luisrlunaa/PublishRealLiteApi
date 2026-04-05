using Microsoft.Extensions.Caching.Memory;

namespace PublishRealLiteApi.Services
{
    public class SimpleRateLimitMiddleware : IMiddleware
    {
        private readonly ILogger<SimpleRateLimitMiddleware> _logger;
        private readonly MemoryCache _cache = new(new MemoryCacheOptions());
        private readonly int _limit = 120; // requests
        private readonly TimeSpan _period = TimeSpan.FromMinutes(1);

        public SimpleRateLimitMiddleware(ILogger<SimpleRateLimitMiddleware> logger)
        {
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                var key = $"rl:{ip}";
                var entry = _cache.GetOrCreate(key, e =>
                {
                    e.AbsoluteExpirationRelativeToNow = _period;
                    return new RateLimitEntry { Count = 0, WindowStart = DateTime.UtcNow };
                });

                entry.Count++;
                if (entry.Count > _limit)
                {
                    context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    context.Response.Headers["Retry-After"] = ((int)_period.TotalSeconds).ToString();
                    await context.Response.WriteAsJsonAsync(new { message = "Too many requests. Try again later." });
                    return;
                }

                await next(context);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Rate limiting middleware failed; allowing request.");
                await next(context);
            }
        }

        private class RateLimitEntry
        {
            public int Count { get; set; }
            public DateTime WindowStart { get; set; }
        }
    }
}
