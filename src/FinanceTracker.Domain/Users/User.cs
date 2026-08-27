using FinanceTracker.Domain.Common;
using FinanceTracker.Domain.Exceptions;
using FinanceTracker.Domain.ValueObjects;

namespace FinanceTracker.Domain.Users;

public sealed class User : AggregateRoot
{
    private readonly List<RefreshToken> _refreshTokens = new();

    public Email Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public string DisplayName { get; private set; } = null!;
    public Currency DisplayCurrency { get; private set; } = null!;

    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    private User() { }

    private User(Guid id, Email email, string passwordHash, string displayName, Currency displayCurrency)
        : base(id)
    {
        Email = email;
        PasswordHash = passwordHash;
        DisplayName = displayName;
        DisplayCurrency = displayCurrency;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public static User Register(Email email, string passwordHash, string displayName, Currency displayCurrency)
    {
        Guard.NotNullOrWhiteSpace(passwordHash, nameof(passwordHash));
        Guard.MaxLength(Guard.NotNullOrWhiteSpace(displayName, nameof(displayName)), 100, nameof(displayName));
        return new User(Guid.NewGuid(), email, passwordHash, displayName, displayCurrency);
    }

    public RefreshToken IssueRefreshToken(string tokenHash, DateTimeOffset expiresAt)
    {
        var token = RefreshToken.Issue(Id, tokenHash, expiresAt);
        _refreshTokens.Add(token);
        return token;
    }

    public RefreshToken RotateRefreshToken(Guid currentTokenId, string newTokenHash, DateTimeOffset newExpiresAt)
    {
        var current = _refreshTokens.SingleOrDefault(rt => rt.Id == currentTokenId)
            ?? throw new DomainException("Refresh-токен не найден.");

        if (!current.IsActive)
            throw new DomainException("Refresh-токен не активен.");

        current.Revoke();
        var next = RefreshToken.Issue(Id, newTokenHash, newExpiresAt, replacedTokenId: current.Id);
        _refreshTokens.Add(next);
        return next;
    }

    public void RevokeAllRefreshTokens()
    {
        foreach (var token in _refreshTokens.Where(t => t.IsActive))
            token.Revoke();
    }

    public void ChangeDisplayCurrency(Currency currency)
    {
        DisplayCurrency = Guard.NotNull(currency, nameof(currency));
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
