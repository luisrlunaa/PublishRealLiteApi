using PublishRealLiteApi.Models;

namespace PublishRealLiteApi.Application.Repositories.Interfaces
{
    public interface IArtistApplicationRepository
    {
        Task AddAsync(ArtistApplication application);
    }
}
