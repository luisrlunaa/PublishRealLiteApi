using System.Net;
using System.Net.Http.Json;
using PublishRealLiteApi.Features.ArtistProfiles;
using PublishRealLiteApi.IntegrationTests.Helpers;
using PublishRealLiteApi.IntegrationTests.Infrastructure;
using Shouldly;

namespace PublishRealLiteApi.IntegrationTests.Tests.ArtistProfiles;

public class ArtistProfileTests(ApiFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task GetAll_WhenNoProfiles_ReturnsEmptyList()
    {
        var response = await Client.GetAsync("/api/artistprofiles");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var profiles = await response.Content.ReadFromJsonAsync<GetArtistProfiles.Response[]>();
        profiles.ShouldNotBeNull();
        profiles.ShouldBeEmpty();
    }

    [Fact]
    public async Task Create_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await Client.PostAsJsonAsync("/api/artistprofiles", FakeData.Artist());

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Create_WithAuth_Returns201WithProfile()
    {
        var token = await AuthHelper.RegisterAndGetTokenAsync(Client);
        UseToken(token);

        var response = await Client.PostAsJsonAsync("/api/artistprofiles", FakeData.Artist());

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var profile = await response.Content.ReadFromJsonAsync<CreateArtistProfile.Response>();
        profile.ShouldNotBeNull();
        profile.Id.ShouldBeGreaterThan(0);
        profile.IsAdminProfile.ShouldBeTrue();
    }

    [Fact]
    public async Task Create_Twice_ReturnsBadRequest()
    {
        var token = await AuthHelper.RegisterAndGetTokenAsync(Client);
        UseToken(token);
        await Client.PostAsJsonAsync("/api/artistprofiles", FakeData.Artist());

        var response = await Client.PostAsJsonAsync("/api/artistprofiles", FakeData.Artist());

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetById_AfterCreate_ReturnsProfile()
    {
        var token = await AuthHelper.RegisterAndGetTokenAsync(Client);
        UseToken(token);
        var created = await (await Client.PostAsJsonAsync("/api/artistprofiles", FakeData.Artist()))
            .Content.ReadFromJsonAsync<CreateArtistProfile.Response>();

        var response = await Client.GetAsync($"/api/artistprofiles/{created!.Id}");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var profile = await response.Content.ReadFromJsonAsync<GetArtistProfileById.Response>();
        profile!.Id.ShouldBe(created.Id);
    }

    [Fact]
    public async Task GetMyAdminProfile_AfterCreate_ReturnsAdminProfile()
    {
        var token = await AuthHelper.RegisterAndGetTokenAsync(Client);
        UseToken(token);
        var created = await (await Client.PostAsJsonAsync("/api/artistprofiles", FakeData.Artist()))
            .Content.ReadFromJsonAsync<CreateArtistProfile.Response>();

        var response = await Client.GetAsync("/api/artistprofiles/me/admin");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var profile = await response.Content.ReadFromJsonAsync<GetMyAdminProfile.Response>();
        profile.ShouldNotBeNull();
        profile.Id.ShouldBe(created!.Id);
    }

    [Fact]
    public async Task Update_MyProfile_ReturnsNoContent()
    {
        var token = await AuthHelper.RegisterAndGetTokenAsync(Client);
        UseToken(token);
        var created = await (await Client.PostAsJsonAsync("/api/artistprofiles", FakeData.Artist()))
            .Content.ReadFromJsonAsync<CreateArtistProfile.Response>();

        var response = await Client.PutAsJsonAsync($"/api/artistprofiles/{created!.Id}",
            new UpdateArtistProfile.Command(0, "Updated Artist Name", "Updated Bio", null));

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Verify the change persisted
        var profile = await (await Client.GetAsync($"/api/artistprofiles/{created.Id}"))
            .Content.ReadFromJsonAsync<GetArtistProfileById.Response>();
        profile!.ArtistName.ShouldBe("Updated Artist Name");
    }

    [Fact]
    public async Task Update_AnotherUsersProfile_ReturnsForbid()
    {
        // Owner creates profile
        var ownerToken = await AuthHelper.RegisterAndGetTokenAsync(Client);
        UseToken(ownerToken);
        var profile = await (await Client.PostAsJsonAsync("/api/artistprofiles", FakeData.Artist()))
            .Content.ReadFromJsonAsync<CreateArtistProfile.Response>();

        // Attacker tries to update
        var attackerToken = await AuthHelper.RegisterAndGetTokenAsync(
            CreateClientWithToken(""), FakeData.Email());
        using var attackerClient = CreateClientWithToken(attackerToken);

        var response = await attackerClient.PutAsJsonAsync($"/api/artistprofiles/{profile!.Id}",
            new UpdateArtistProfile.Command(0, "Hacked", null, null));

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Delete_MyProfile_ReturnsNoContent()
    {
        var token = await AuthHelper.RegisterAndGetTokenAsync(Client);
        UseToken(token);
        var created = await (await Client.PostAsJsonAsync("/api/artistprofiles", FakeData.Artist()))
            .Content.ReadFromJsonAsync<CreateArtistProfile.Response>();

        var response = await Client.DeleteAsync($"/api/artistprofiles/{created!.Id}");

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Soft-deleted — should return 404 now
        var getResponse = await Client.GetAsync($"/api/artistprofiles/{created.Id}");
        getResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
