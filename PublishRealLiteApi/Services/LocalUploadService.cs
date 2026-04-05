using PublishRealLiteApi.DTOs;
using PublishRealLiteApi.Services.Interfaces;

namespace PublishRealLiteApi.Services
{
    public class LocalUploadService : IUploadService
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<LocalUploadService> _logger;
        private readonly long _maxBytes = 3 * 1024 * 1024; // 3MB

        public LocalUploadService(IWebHostEnvironment env, ILogger<LocalUploadService> logger)
        {
            _env = env;
            _logger = logger;
        }

        public async Task<UploadResultDto> SaveImageAsync(IFormFile file, string subfolder = "images")
        {
            if (file == null) throw new ArgumentNullException(nameof(file));
            if (file.Length == 0) throw new ArgumentException("Empty file");
            if (file.Length > _maxBytes) throw new InvalidOperationException("File too large");

            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowed.Contains(ext)) throw new InvalidOperationException("Invalid file type");

            var uploads = Path.Combine(_env.ContentRootPath, "wwwroot", "uploads", subfolder);
            Directory.CreateDirectory(uploads);

            var fileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploads, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var url = $"/uploads/{subfolder}/{fileName}";
            return new UploadResultDto { Url = url, FileName = fileName, Size = file.Length };
        }

        public Task DeleteAsync(string urlOrPath)
        {
            try
            {
                var path = urlOrPath.StartsWith("/") ? urlOrPath.TrimStart('/') : urlOrPath;
                var full = Path.Combine(_env.ContentRootPath, path.Replace('/', Path.DirectorySeparatorChar));
                if (File.Exists(full)) File.Delete(full);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete file {p}", urlOrPath);
            }
            return Task.CompletedTask;
        }
    }

}
