using System.Net;
using System.Net.Http.Json;
using PublishRealLiteApi.DTOs;
using PublishRealLiteApi.IntegrationTests.Helpers;
using PublishRealLiteApi.IntegrationTests.Infrastructure;
using Shouldly;

namespace PublishRealLiteApi.IntegrationTests.Tests.Tracks;

public class TracksTests(ApiFactory factory) : BaseIntegrationTest(factory)
{
    private async Task<(ArtistProfileDto profile, ReleaseDto release)> CreateProfileAndReleaseAsync()
    {
        var token = await AuthHelper.RegisterAndGetTokenAsync(Client);
        UseToken(token);

        var profile = await (await Client.PostAsJsonAsync("/api/artistprofiles", FakeData.Artist()))
            .Content.ReadFromJsonAsync<ArtistProfileDto>();

        var release = await (await Client.PostAsJsonAsync("/api/releases", FakeData.Release(profile!.Id)))
            .Content.ReadFromJsonAsync<ReleaseDto>();

        return (profile, release!);
    }

    [Fact]
    public async Task GetByRelease_WhenNoTracks_ReturnsEmptyList()
    {
        var (_, release) = await CreateProfileAndReleaseAsync();

        var response = await Client.GetAsync($"/api/tracks/by-release/{release.Id}");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var tracks = await response.Content.ReadFromJsonAsync<TrackDto[]>();
        tracks.ShouldNotBeNull();
        tracks.ShouldBeEmpty();
    }

    [Fact]
    public async Task Create_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await Client.PostAsJsonAsync("/api/tracks",
            new CreateTrackDto(Guid.NewGuid(), 1, "Track"));

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Create_WithAuth_Returns201WithTrack()
    {
        var (_, release) = await CreateProfileAndReleaseAsync();

        var response = await Client.PostAsJsonAsync("/api/tracks",
            FakeData.Track(release.Id, position: 1));

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var track = await response.Content.ReadFromJsonAsync<TrackDto>();
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

        var tracks = await response.Content.ReadFromJsonAsync<TrackDto[]>();
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
            .Content.ReadFromJsonAsync<TrackDto>();

        var response = await Client.DeleteAsync($"/api/tracks/{created!.Id}");

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    // NOTE: TracksController.Update has a bug — it assigns track.Position = track.Position
    // and track.Title = track.Title instead of dto.Position / dto.Title. This test documents
    // the expected behavior and will fail until the bug is fixed.
    [Fact]
    public async Task Update_Track_PersistsNewValues()
    {
        var (_, release) = await CreateProfileAndReleaseAsync();
        var created = await (await Client.PostAsJsonAsync("/api/tracks", FakeData.Track(release.Id, 1)))
            .Content.ReadFromJsonAsync<TrackDto>();

        var response = await Client.PutAsJsonAsync($"/api/tracks/{created!.Id}",
            new UpdateTrackDto(Position: 5, Title: "Renamed Track"));

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var tracks = await (await Client.GetAsync($"/api/tracks/by-release/{release.Id}"))
            .Content.ReadFromJsonAsync<TrackDto[]>();

        var updated = tracks!.Single(t => t.Id == created.Id);
        updated.Title.ShouldBe("Renamed Track");
        updated.Position.ShouldBe(5);
    }
}
