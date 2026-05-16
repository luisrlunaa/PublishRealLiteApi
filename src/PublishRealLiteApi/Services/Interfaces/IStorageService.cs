using PublishRealLiteApi.DTOs;

namespace PublishRealLiteApi.Services.Interfaces
{
    public interface IStorageService
    {
        Task<UploadResultDto> SaveImageAsync(IFormFile file, string subfolder = "images");
        Task DeleteAsync(string urlOrPath);
    }
}
