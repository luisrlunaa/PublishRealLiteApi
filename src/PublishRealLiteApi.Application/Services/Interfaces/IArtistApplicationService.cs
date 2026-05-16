using PublishRealLiteApi.Application.DTOs;

namespace PublishRealLiteApi.Application.Services.Interfaces
{
    public interface IArtistApplicationService
    {
        Task<bool> SubmitAsync(SubmitApplicationDto dto);
    }
}
