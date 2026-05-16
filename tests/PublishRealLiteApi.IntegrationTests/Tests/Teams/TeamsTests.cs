using System.Net;
using System.Net.Http.Json;
using PublishRealLiteApi.DTOs;
using PublishRealLiteApi.IntegrationTests.Helpers;
using PublishRealLiteApi.IntegrationTests.Infrastructure;
using Shouldly;

namespace PublishRealLiteApi.IntegrationTests.Tests.Teams;

public class TeamsTests(ApiFactory factory) : BaseIntegrationTest(factory)
{
    private async Task<int> CreateUserWithProfileAsync(HttpClient? client = null)
    {
        client ??= Client;
        var token = await AuthHelper.RegisterAndGetTokenAsync(client);
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var profile = await (await client.PostAsJsonAsync("/api/artistprofiles", FakeData.Artist()))
            .Content.ReadFromJsonAsync<ArtistProfileDto>();
        return profile!.Id;
    }

    [Fact]
    public async Task GetMine_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await Client.GetAsync("/api/teams/mine");

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Create_WithAuth_Returns201WithTeam()
    {
        await CreateUserWithProfileAsync();

        var response = await Client.PostAsJsonAsync("/api/teams", new { Name = "My Band" });

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var team = await response.Content.ReadFromJsonAsync<TeamDto>();
        team.ShouldNotBeNull();
        team.Id.ShouldBeGreaterThan(0);
        team.Name.ShouldBe("My Band");
    }

    [Fact]
    public async Task GetMine_AfterCreate_ReturnsTeams()
    {
        await CreateUserWithProfileAsync();
        await Client.PostAsJsonAsync("/api/teams", new { Name = "Team A" });
        await Client.PostAsJsonAsync("/api/teams", new { Name = "Team B" });

        var response = await Client.GetAsync("/api/teams/mine");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var teams = await response.Content.ReadFromJsonAsync<TeamDto[]>();
        teams.ShouldNotBeNull();
        teams.Length.ShouldBe(2);
    }

    [Fact]
    public async Task Invite_WithValidTeamAndEmail_ReturnsOk()
    {
        await CreateUserWithProfileAsync();
        var team = await (await Client.PostAsJsonAsync("/api/teams", new { Name = "Collab Team" }))
            .Content.ReadFromJsonAsync<TeamDto>();

        var response = await Client.PostAsJsonAsync("/api/teams/invite",
            new { TeamId = team!.Id, Email = "invited@test.com" });

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Accept_WithValidToken_ReturnsOk()
    {
        // Owner creates team and sends invite
        await CreateUserWithProfileAsync();
        var team = await (await Client.PostAsJsonAsync("/api/teams", new { Name = "Joint Team" }))
            .Content.ReadFromJsonAsync<TeamDto>();

        var inviteeEmail = FakeData.Email();
        await Client.PostAsJsonAsync("/api/teams/invite",
            new { TeamId = team!.Id, Email = inviteeEmail });

        // Retrieve the invite token directly from the database
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PublishRealLiteApi.Infrastructure.Data.AppDbContext>();
        var invite = db.TeamInvites.Single(i => i.Email == inviteeEmail);

        // Invitee registers and accepts
        using var inviteeClient = Factory.CreateClient();
        await AuthHelper.RegisterAndGetTokenAsync(inviteeClient, inviteeEmail);
        await CreateUserWithProfileAsync(inviteeClient);
        var inviteeToken = await AuthHelper.LoginAndGetTokenAsync(inviteeClient, inviteeEmail);
        inviteeClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", inviteeToken);

        var response = await inviteeClient.PostAsync(
            $"/api/teams/accept?token={invite.Token}", null);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}
