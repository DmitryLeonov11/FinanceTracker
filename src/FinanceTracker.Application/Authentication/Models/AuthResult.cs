namespace FinanceTracker.Application.Authentication.Models;

public sealed record AuthResult(
    Guid UserId,
    string Email,
    string DisplayName,
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt);
