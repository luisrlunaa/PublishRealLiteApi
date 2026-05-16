using System.Net;
using System.Net.Http.Json;
using PublishRealLiteApi.Features.ArtistProfiles;
using PublishRealLiteApi.Features.Releases;
using PublishRealLiteApi.Features.Tracks;
using PublishRealLiteApi.IntegrationTests.Helpers;
using PublishRealLiteApi.IntegrationTests.Infrastructure;
using Shouldly;

namespace PublishRealLiteApi.IntegrationTests.Tests.Tracks;

public class TracksTests(ApiFactory factory) : BaseIntegrationTest(factory)
{
    private async Task<(CreateArtistProfile.Response profile, CreateRelease.Response release)> CreateProfileAndReleaseAsync()
    {
        var token = await AuthHelper.RegisterAndGetTokenAsync(Client);
        UseToken(token);

        var profile = await (await Client.PostAsJsonAsync("/api/artistprofiles", FakeData.Artist()))
            .Content.ReadFromJsonAsync<CreateArtistProfile.Response>();

        var release = await (await Client.PostAsJsonAsync("/api/releases", FakeData.Release(profile!.Id)))
            .Content.ReadFromJsonAsync<CreateRelease.Response>();

        return (profile, release!);
    }

    [Fact]
    public async Task GetByRelease_WhenNoTracks_ReturnsEmptyList()
    {
        var (_, release) = await CreateProfileAndReleaseAsync();

        var response = await Client.GetAsync($"/api/tracks/by-release/{release.Id}");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var tracks = await response.Content.ReadFromJsonAsync<GetTracksByRelease.Response[]>();
        tracks.ShouldNotBeNull();
        tracks.ShouldBeEmpty();
    }

    [Fact]
    public async Task Create_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await Client.PostAsJsonAsync("/api/tracks",
            new CreateTrack.Command(Guid.NewGuid(), 1, "Track"));

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Create_WithAuth_Returns201WithTrack()
    {
        var (_, release) = await CreateProfileAndReleaseAsync();

        var response = await Client.PostAsJsonAsync("/api/tracks",
            FakeData.Track(release.Id, position: 1));

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var track = await response.Content.ReadFromJsonAsync<CreateTrack.Response>();
        track.ShouldNotBeNull();
        track.ReleaseId.ShouldBe(release.Id);
        track.Position.ShouldBe(1);
    }

    [Fact]
    public async Task GetByRelease_AfterCreate_ReturnsTracksOrderedByPosition()
    {
        var (_, release) = await CreateProfileAndReleaseAsync();
        await Client.PostAsJsonAsync("/api/tracks", FakeData.Track(release.Id, position: 2));
        await Client.PostAsJsonAsync("/api/tracks", FakeData.Track(release.Id, position: 1));

        var response = await Client.GetAsync($"/api/tracks/by-release/{release.Id}");

        var tracks = await response.Content.ReadFromJsonAsync<GetTracksByRelease.Response[]>();
        tracks.ShouldNotBeNull();
        tracks.Length.ShouldBe(2);
        tracks[0].Position.ShouldBe(1);
        tracks[1].Position.ShouldBe(2);
    }

    [Fact]
    public async Task Delete_Track_ReturnsNoContent()
    {
        var (_, release) = await CreateProfileAndReleaseAsync();
        var created = await (await Client.PostAsJsonAsync("/api/tracks", FakeData.Track(release.Id)))
            .Content.ReadFromJsonAsync<CreateTrack.Response>();

        var response = await Client.DeleteAsync($"/api/tracks/{created!.Id}");

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Update_Track_PersistsNewValues()
    {
        var (_, release) = await CreateProfileAndReleaseAsync();
        var created = await (await Client.PostAsJsonAsync("/api/tracks", FakeData.Track(release.Id, 1)))
            .Content.ReadFromJsonAsync<CreateTrack.Response>();

        var response = await Client.PutAsJsonAsync($"/api/tracks/{created!.Id}",
            new UpdateTrack.Command(Guid.Empty, 5, "Renamed Track"));

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var tracks = await (await Client.GetAsync($"/api/tracks/by-release/{release.Id}"))
            .Content.ReadFromJsonAsync<GetTracksByRelease.Response[]>();

        var updated = tracks!.Single(t => t.Id == created.Id);
        updated.Title.ShouldBe("Renamed Track");
        updated.Position.ShouldBe(5);
    }
}
