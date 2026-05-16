namespace PublishRealLiteApi.Services.Interfaces
{
    public interface IStreamStatAggregatorService
    {
        Task AggregateOnceAsync(CancellationToken cancellationToken = default);
    }
}
