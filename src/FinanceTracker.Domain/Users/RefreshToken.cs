using FinanceTracker.Domain.Common;

namespace FinanceTracker.Domain.Users;

public sealed class RefreshToken : Entity
{
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; } = null!;
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }
    public Guid? ReplacedByTokenId { get; private set; }

    public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt is not null;
    public bool IsActive => !IsRevoked && !IsExpired;

    private RefreshToken() { }

    private RefreshToken(Guid id, Guid userId, string tokenHash, DateTimeOffset expiresAt, Guid? replacedTokenId)
        : base(id)
    {
        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAt = expiresAt;
        ReplacedByTokenId = replacedTokenId;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    internal static RefreshToken Issue(Guid userId, string tokenHash, DateTimeOffset expiresAt, Guid? replacedTokenId = null)
        => new(Guid.NewGuid(), userId, Guard.NotNullOrWhiteSpace(tokenHash, nameof(tokenHash)), expiresAt, replacedTokenId);

    internal void Revoke()
    {
        if (IsRevoked) return;
        RevokedAt = DateTimeOffset.UtcNow;
        UpdatedAt = RevokedAt;
    }
}
