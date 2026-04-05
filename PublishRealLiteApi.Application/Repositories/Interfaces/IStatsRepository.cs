using PublishRealLiteApi.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PublishRealLiteApi.Application.Repositories.Interfaces
{
    public interface IStatsRepository
    {
        Task<IEnumerable<StreamStat>> GetStatsAsync(DateTime from, DateTime to, int? artistProfileId = null, int? releaseId = null);
        Task AddRangeAsync(IEnumerable<StreamStat> stats);
    }
}
