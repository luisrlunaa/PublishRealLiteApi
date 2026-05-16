using System.Net.Http.Headers;

namespace PublishRealLiteApi.IntegrationTests.Infrastructure;

[Collection("IntegrationTests")]
public abstract class BaseIntegrationTest : IAsyncLifetime
{
    protected readonly ApiFactory Factory;
    protected HttpClient Client { get; private set; }

    protected BaseIntegrationTest(ApiFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }

    public async Task InitializeAsync() => await Factory.ResetDatabaseAsync();

    public Task DisposeAsync()
    {
        Client.Dispose();
        return Task.CompletedTask;
    }

    protected void UseToken(string token) =>
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

    protected HttpClient CreateClientWithToken(string token)
    {
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}
