using System.Net;
using System.Net.Http.Json;
using PublishRealLiteApi.DTOs;
using PublishRealLiteApi.IntegrationTests.Infrastructure;
using Shouldly;

namespace PublishRealLiteApi.IntegrationTests.Tests.Auth;

public class AuthTests(ApiFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Register_WithValidData_Returns200WithUserEmail()
    {
        var email = "newartist@test.com";

        var response = await Client.PostAsJsonAsync("/api/auth/register",
            new RegisterDto(email, "Password123!"));

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.ShouldContain(email);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsBadRequest()
    {
        var dto = new RegisterDto("dup@test.com", "Password123!");
        await Client.PostAsJsonAsync("/api/auth/register", dto);

        var response = await Client.PostAsJsonAsync("/api/auth/register", dto);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        var email = "login@test.com";
        var password = "Password123!";
        await Client.PostAsJsonAsync("/api/auth/register", new RegisterDto(email, password));

        var response = await Client.PostAsJsonAsync("/api/auth/login",
            new LoginDto(email, password));

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var auth = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        auth.ShouldNotBeNull();
        auth.Token.ShouldNotBeNullOrWhiteSpace();
        auth.Email.ShouldBe(email);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        var email = "wrongpass@test.com";
        await Client.PostAsJsonAsync("/api/auth/register",
            new RegisterDto(email, "Password123!"));

        var response = await Client.PostAsJsonAsync("/api/auth/login",
            new LoginDto(email, "WrongPassword!"));

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithNonExistentUser_ReturnsUnauthorized()
    {
        var response = await Client.PostAsJsonAsync("/api/auth/login",
            new LoginDto("ghost@test.com", "Password123!"));

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
