namespace FinanceTracker.Infrastructure.Settings;

public sealed class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Issuer { get; init; } = "FinanceTracker";
    public string Audience { get; init; } = "FinanceTracker.Clients";
    public string SigningKey { get; init; } = string.Empty;
    public int AccessTokenMinutes { get; init; } = 15;
    public int RefreshTokenDays { get; init; } = 14;
}
