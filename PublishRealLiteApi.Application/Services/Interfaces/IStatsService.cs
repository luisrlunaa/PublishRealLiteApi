using PublishRealLiteApi.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PublishRealLiteApi.Application.Services.Interfaces
{
    public interface IStatsService
    {
        Task ImportAsync(IEnumerable<StreamStatDto> items);
        Task<StatsSummaryDto> GetSummaryAsync(int rangeDays, int? artistProfileId = null);
        Task<IEnumerable<ByCountryDto>> GetByCountryAsync(int rangeDays, int? artistProfileId = null);
        Task<IEnumerable<ByDateDto>> GetByDateAsync(int rangeDays, int? artistProfileId = null);
        Task<IEnumerable<BySourceDto>> GetBySourceAsync(int rangeDays, int? artistProfileId = null);
    }
}
