using System.Net;
using System.Net.Http.Json;
using FarmerOps.Application.Auth.Dtos;
using FluentAssertions;

namespace FarmerOps.IntegrationTests;

[Collection(nameof(IntegrationTestCollection))]
public class AuthApiTests(CustomWebApplicationFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    private static object RegisterPayload(string email) => new
    {
        email,
        password = "P@ssw0rd123!",
        role = 0, // Admin
        fieldAgentId = (Guid?)null
    };

    [Fact]
    public async Task Register_NewUser_ReturnsTokenPair()
    {
        var email = $"{Guid.NewGuid()}@farmerops.test";

        var response = await _client.PostAsJsonAsync("/auth/register", RegisterPayload(email));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AuthResultDto>();
        result!.AccessToken.Should().NotBeNullOrWhiteSpace();
        result.RefreshToken.Should().NotBeNullOrWhiteSpace();
        result.User.Email.Should().Be(email);
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsBadRequest()
    {
        var email = $"{Guid.NewGuid()}@farmerops.test";
        await _client.PostAsJsonAsync("/auth/register", RegisterPayload(email));

        var response = await _client.PostAsJsonAsync("/auth/register", RegisterPayload(email));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsTokenPair()
    {
        var email = $"{Guid.NewGuid()}@farmerops.test";
        await _client.PostAsJsonAsync("/auth/register", RegisterPayload(email));

        var response = await _client.PostAsJsonAsync("/auth/login", new { email, password = "P@ssw0rd123!" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AuthResultDto>();
        result!.AccessToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_WrongPassword_ReturnsUnauthorized()
    {
        var email = $"{Guid.NewGuid()}@farmerops.test";
        await _client.PostAsJsonAsync("/auth/register", RegisterPayload(email));

        var response = await _client.PostAsJsonAsync("/auth/login", new { email, password = "wrong-password" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_RotatesToANewToken_AndInvalidatesTheOldOne()
    {
        var email = $"{Guid.NewGuid()}@farmerops.test";
        var registerResponse = await _client.PostAsJsonAsync("/auth/register", RegisterPayload(email));
        var initial = await registerResponse.Content.ReadFromJsonAsync<AuthResultDto>();

        var refreshResponse = await _client.PostAsJsonAsync("/auth/refresh", new { refreshToken = initial!.RefreshToken });
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var rotated = await refreshResponse.Content.ReadFromJsonAsync<AuthResultDto>();
        rotated!.RefreshToken.Should().NotBe(initial.RefreshToken);

        var reuseOldTokenResponse = await _client.PostAsJsonAsync("/auth/refresh", new { refreshToken = initial.RefreshToken });
        reuseOldTokenResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Me_WithoutToken_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/auth/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Me_WithValidToken_ReturnsCurrentUser()
    {
        var email = $"{Guid.NewGuid()}@farmerops.test";
        var registerResponse = await _client.PostAsJsonAsync("/auth/register", RegisterPayload(email));
        var auth = await registerResponse.Content.ReadFromJsonAsync<AuthResultDto>();

        using var request = new HttpRequestMessage(HttpMethod.Get, "/auth/me");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", auth!.AccessToken);
        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var user = await response.Content.ReadFromJsonAsync<UserDto>();
        user!.Email.Should().Be(email);
    }
}
