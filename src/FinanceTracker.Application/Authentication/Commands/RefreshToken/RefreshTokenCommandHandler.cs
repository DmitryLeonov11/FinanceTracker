using FinanceTracker.Application.Authentication.Models;
using FinanceTracker.Application.Common.Exceptions;
using FinanceTracker.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Application.Authentication.Commands.RefreshToken;

public sealed class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResult>
{
    private readonly IApplicationDbContext _db;
    private readonly IJwtTokenService _jwt;

    public RefreshTokenCommandHandler(IApplicationDbContext db, IJwtTokenService jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    public async Task<AuthResult> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var hash = _jwt.Hash(request.RefreshToken);

        var token = await _db.RefreshTokens
            .SingleOrDefaultAsync(rt => rt.TokenHash == hash, cancellationToken)
            ?? throw new ForbiddenAccessException("Недействительный refresh-токен.");

        if (!token.IsActive)
            throw new ForbiddenAccessException("Refresh-токен больше не активен.");

        var user = await _db.Users
            .Include(u => u.RefreshTokens)
            .SingleOrDefaultAsync(u => u.Id == token.UserId, cancellationToken)
            ?? throw new ForbiddenAccessException("Пользователь не найден.");

        var newRefresh = _jwt.IssueRefreshToken();
        user.RotateRefreshToken(token.Id, newRefresh.Hash, newRefresh.ExpiresAt);

        var access = _jwt.IssueAccessToken(user);

        await _db.SaveChangesAsync(cancellationToken);

        return new AuthResult(
            user.Id,
            user.Email.Value,
            user.DisplayName,
            access.Value,
            access.ExpiresAt,
            newRefresh.RawValue,
            newRefresh.ExpiresAt);
    }
}
