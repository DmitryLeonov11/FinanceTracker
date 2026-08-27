using FinanceTracker.Domain.Users;

namespace FinanceTracker.Application.Common.Interfaces;

public interface IJwtTokenService
{
    AccessToken IssueAccessToken(User user);
    RefreshTokenMaterial IssueRefreshToken();
    string Hash(string rawToken);
}

public sealed record AccessToken(string Value, DateTimeOffset ExpiresAt);

public sealed record RefreshTokenMaterial(string RawValue, string Hash, DateTimeOffset ExpiresAt);
