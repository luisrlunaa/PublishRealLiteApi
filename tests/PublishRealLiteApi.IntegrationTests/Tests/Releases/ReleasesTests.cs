using System.Net;
using System.Net.Http.Json;
using PublishRealLiteApi.Features.ArtistProfiles;
using PublishRealLiteApi.Features.Releases;
using PublishRealLiteApi.IntegrationTests.Helpers;
using PublishRealLiteApi.IntegrationTests.Infrastructure;
using Shouldly;

namespace PublishRealLiteApi.IntegrationTests.Tests.Releases;

public class ReleasesTests(ApiFactory factory) : BaseIntegrationTest(factory)
{
    private async Task<(CreateArtistProfile.Response profile, string token)> CreateUserWithProfileAsync()
    {
        var token = await AuthHelper.RegisterAndGetTokenAsync(Client);
        UseToken(token);
        var profile = await (await Client.PostAsJsonAsync("/api/artistprofiles", FakeData.Artist()))
            .Content.ReadFromJsonAsync<CreateArtistProfile.Response>();
        return (profile!, token);
    }

    [Fact]
    public async Task Create_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await Client.PostAsJsonAsync("/api/releases",
            new CreateRelease.Command(1, "Title", null, null, null, null, null, null));

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Create_WithAuth_Returns201WithRelease()
    {
        var (profile, _) = await CreateUserWithProfileAsync();

        var response = await Client.PostAsJsonAsync("/api/releases", FakeData.Release(profile.Id));

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var release = await response.Content.ReadFromJsonAsync<CreateRelease.Response>();
        release.ShouldNotBeNull();
        release.Id.ShouldNotBe(Guid.Empty);
        release.ArtistProfileId.ShouldBe(profile.Id);
    }

    [Fact]
    public async Task GetAll_ByArtistProfileId_ReturnsReleasesForThatProfile()
    {
        var (profile, _) = await CreateUserWithProfileAsync();
        await Client.PostAsJsonAsync("/api/releases", FakeData.Release(profile.Id));
        await Client.PostAsJsonAsync("/api/releases", FakeData.Release(profile.Id));

        var response = await Client.GetAsync($"/api/releases?artistProfileId={profile.Id}");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var releases = await response.Content.ReadFromJsonAsync<GetReleases.Response[]>();
        releases.ShouldNotBeNull();
        releases.Length.ShouldBe(2);
        releases.ShouldAllBe(r => r.ArtistProfileId == profile.Id);
    }

    [Fact]
    public async Task GetById_ReturnsRelease()
    {
        var (profile, _) = await CreateUserWithProfileAsync();
        var created = await (await Client.PostAsJsonAsync("/api/releases", FakeData.Release(profile.Id)))
            .Content.ReadFromJsonAsync<CreateRelease.Response>();

        var response = await Client.GetAsync($"/api/releases/{created!.Id}");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var release = await response.Content.ReadFromJsonAsync<GetReleaseById.Response>();
        release!.Id.ShouldBe(created.Id);
    }

    [Fact]
    public async Task GetById_WithInvalidId_ReturnsNotFound()
    {
        var response = await Client.GetAsync($"/api/releases/{Guid.NewGuid()}");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_MyRelease_ReturnsNoContent()
    {
        var (profile, _) = await CreateUserWithProfileAsync();
        var created = await (await Client.PostAsJsonAsync("/api/releases", FakeData.Release(profile.Id)))
            .Content.ReadFromJsonAsync<CreateRelease.Response>();

        var response = await Client.PutAsJsonAsync($"/api/releases/{created!.Id}",
            new UpdateRelease.Command(Guid.Empty, "Updated Title", null, "Electronic", null, null, null, null));

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete_MyRelease_ReturnsNoContent()
    {
        var (profile, _) = await CreateUserWithProfileAsync();
        var created = await (await Client.PostAsJsonAsync("/api/releases", FakeData.Release(profile.Id)))
            .Content.ReadFromJsonAsync<CreateRelease.Response>();

        var response = await Client.DeleteAsync($"/api/releases/{created!.Id}?artistProfileId={profile.Id}");

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var getResponse = await Client.GetAsync($"/api/releases/{created.Id}");
        getResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
