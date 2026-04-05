using PublishRealLiteApi.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PublishRealLiteApi.Application.Services.Interfaces
{
    public interface IReleaseService
    {
        Task<IEnumerable<Release>> GetReleasesAsync(int artistProfileId);
        Task<Release?> GetReleaseByIdAsync(Guid id, int artistProfileId);
        Task<Release> CreateReleaseAsync(Release release);
        Task<bool> UpdateReleaseAsync(Release release);
        Task<bool> DeleteReleaseAsync(Guid id, int artistProfileId);
    }
}
