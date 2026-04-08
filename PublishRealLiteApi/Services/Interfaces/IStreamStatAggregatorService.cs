using System.Threading;
using System.Threading.Tasks;

namespace PublishRealLiteApi.Services.Interfaces
{
    public interface IStreamStatAggregatorService
    {
        Task AggregateOnceAsync(CancellationToken cancellationToken = default);
    }
}
