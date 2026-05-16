using System.Net;
using System.Net.Http.Json;
using PublishRealLiteApi.Features.Auth;
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
            new Register.Command(email, "Password123!", "test"));

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.ShouldContain(email);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsBadRequest()
    {
        var cmd = new Register.Command("dup@test.com", "Password123!", "test");
        await Client.PostAsJsonAsync("/api/auth/register", cmd);

        var response = await Client.PostAsJsonAsync("/api/auth/register", cmd);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        var email = "login@test.com";
        var password = "Password123!";
        await Client.PostAsJsonAsync("/api/auth/register", new Register.Command(email, password, "test"));

        var response = await Client.PostAsJsonAsync("/api/auth/login",
            new Login.Command(email, password, "test"));

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var auth = await response.Content.ReadFromJsonAsync<Login.Response>();
        auth.ShouldNotBeNull();
        auth.Token.ShouldNotBeNullOrWhiteSpace();
        auth.Email.ShouldBe(email);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        var email = "wrongpass@test.com";
        await Client.PostAsJsonAsync("/api/auth/register",
            new Register.Command(email, "Password123!", "test"));

        var response = await Client.PostAsJsonAsync("/api/auth/login",
            new Login.Command(email, "WrongPassword!", "test"));

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithNonExistentUser_ReturnsUnauthorized()
    {
        var response = await Client.PostAsJsonAsync("/api/auth/login",
            new Login.Command("ghost@test.com", "Password123!", "test"));

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
