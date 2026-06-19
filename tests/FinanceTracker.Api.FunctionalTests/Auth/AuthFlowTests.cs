using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FinanceTracker.Api.FunctionalTests.Infrastructure;

namespace FinanceTracker.Api.FunctionalTests.Auth;

public class AuthFlowTests : IClassFixture<FinanceTrackerWebApplicationFactory>
{
    private readonly FinanceTrackerWebApplicationFactory _factory;

    public AuthFlowTests(FinanceTrackerWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private sealed record AuthResult(
        Guid UserId,
        string Email,
        string DisplayName,
        string AccessToken,
        DateTimeOffset AccessTokenExpiresAt,
        string RefreshToken,
        DateTimeOffset RefreshTokenExpiresAt);

    private sealed record RegisterRequest(string Email, string Password, string DisplayName, string DisplayCurrency);
    private sealed record LoginRequest(string Email, string Password);
    private sealed record RefreshRequest(string RefreshToken);

    [Fact]
    public async Task Register_then_login_then_refresh_then_access_protected_endpoint_should_succeed()
    {
        var client = _factory.CreateClient();
        var email = $"smoke-{Guid.NewGuid():N}@test.local";
        const string password = "Password123";

        // --- 1. Register ---
        var registerResponse = await client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest(email, password, "Smoke Tester", "BYN"));

        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var registered = await registerResponse.Content.ReadFromJsonAsync<AuthResult>();
        registered.Should().NotBeNull();
        registered!.Email.Should().Be(email);
        registered.AccessToken.Should().NotBeNullOrWhiteSpace();
        registered.RefreshToken.Should().NotBeNullOrWhiteSpace();

        // --- 2. Login ---
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, password));
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var loggedIn = await loginResponse.Content.ReadFromJsonAsync<AuthResult>();
        loggedIn.Should().NotBeNull();
        loggedIn!.AccessToken.Should().NotBeNullOrWhiteSpace();
        loggedIn.UserId.Should().Be(registered.UserId);

        // --- 3. Refresh ---
        var refreshResponse = await client.PostAsJsonAsync("/api/auth/refresh",
            new RefreshRequest(loggedIn.RefreshToken));
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var refreshed = await refreshResponse.Content.ReadFromJsonAsync<AuthResult>();
        refreshed.Should().NotBeNull();
        refreshed!.AccessToken.Should().NotBeNullOrWhiteSpace();
        refreshed.RefreshToken.Should().NotBe(loggedIn.RefreshToken,
            "rotation should produce a new refresh token");

        // --- 4. Reuse of revoked refresh token should fail ---
        var reuseResponse = await client.PostAsJsonAsync("/api/auth/refresh",
            new RefreshRequest(loggedIn.RefreshToken));
        reuseResponse.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);

        // --- 5. Access protected endpoint with fresh access token ---
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", refreshed.AccessToken);
        var dashboardResponse = await client.GetAsync("/api/dashboard/balance");
        dashboardResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Reusing_revoked_refresh_token_should_revoke_the_entire_chain()
    {
        var client = _factory.CreateClient();
        var email = $"reuse-{Guid.NewGuid():N}@test.local";
        const string password = "Password123";

        var register = await client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest(email, password, "Reuse Tester", "BYN"));
        register.StatusCode.Should().Be(HttpStatusCode.OK);
        var registered = (await register.Content.ReadFromJsonAsync<AuthResult>())!;

        // Rotate once: original token is now revoked, rotatedToken is the live one.
        var firstRefresh = await client.PostAsJsonAsync("/api/auth/refresh",
            new RefreshRequest(registered.RefreshToken));
        firstRefresh.StatusCode.Should().Be(HttpStatusCode.OK);
        var rotated = (await firstRefresh.Content.ReadFromJsonAsync<AuthResult>())!;

        // Reuse the already-revoked original token → theft signal, whole chain gets revoked.
        var reuse = await client.PostAsJsonAsync("/api/auth/refresh",
            new RefreshRequest(registered.RefreshToken));
        reuse.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);

        // The rotated (previously valid) token must now be dead too.
        var rotatedAfterReuse = await client.PostAsJsonAsync("/api/auth/refresh",
            new RefreshRequest(rotated.RefreshToken));
        rotatedAfterReuse.StatusCode.Should().BeOneOf(
            new[] { HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden },
            "reuse of a revoked token must revoke the entire refresh-token chain");
    }

    [Fact]
    public async Task Protected_endpoint_without_token_should_return_401()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/dashboard/balance");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_with_wrong_password_should_return_401()
    {
        var client = _factory.CreateClient();
        var email = $"wrongpw-{Guid.NewGuid():N}@test.local";

        var register = await client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest(email, "Password123", "Wrong PW", "BYN"));
        register.StatusCode.Should().Be(HttpStatusCode.OK);

        var login = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest(email, "WrongPassword1"));
        login.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Register_with_duplicate_email_should_return_400()
    {
        var client = _factory.CreateClient();
        var email = $"dup-{Guid.NewGuid():N}@test.local";

        var first = await client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest(email, "Password123", "First", "BYN"));
        first.StatusCode.Should().Be(HttpStatusCode.OK);

        var second = await client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest(email, "Password123", "Second", "USD"));
        second.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
