using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using PublishRealLiteApi.DTOs;
using PublishRealLiteApi.Services.Interfaces;

namespace PublishRealLiteApi.Services
{
    public class AzureBlobStorageService : IStorageService
    {
        private readonly BlobServiceClient _client;
        private readonly ILogger<AzureBlobStorageService> _logger;
        private readonly string _containerName;

        public AzureBlobStorageService(IConfiguration config, ILogger<AzureBlobStorageService> logger)
        {
            var conn = config.GetConnectionString("AzureBlob") ?? config["AzureBlob:ConnectionString"];
            _containerName = config["AzureBlob:Container"] ?? "uploads";
            _client = new BlobServiceClient(conn);
            _logger = logger;
        }

        public async Task<UploadResultDto> SaveImageAsync(IFormFile file, string subfolder = "images")
        {
            if (file == null) throw new ArgumentNullException(nameof(file));
            var container = _client.GetBlobContainerClient(_containerName);
            await container.CreateIfNotExistsAsync(PublicAccessType.Blob);

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            if (!allowed.Contains(ext)) throw new InvalidOperationException("Invalid file type");

            var blobName = $"{subfolder}/{Guid.NewGuid()}{ext}";
            var blobClient = container.GetBlobClient(blobName);

            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });
            }

            var url = blobClient.Uri.ToString();
            return new UploadResultDto { Url = url, FileName = Path.GetFileName(blobName), Size = file.Length };
        }

        public async Task DeleteAsync(string urlOrPath)
        {
            try
            {
                var blobName = urlOrPath;
                // if full url, extract blob name
                if (urlOrPath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    var uri = new Uri(urlOrPath);
                    blobName = uri.AbsolutePath.TrimStart('/');
                }

                var container = _client.GetBlobContainerClient(_containerName);
                var blob = container.GetBlobClient(blobName);
                await blob.DeleteIfExistsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete blob {p}", urlOrPath);
            }
        }
    }
}
