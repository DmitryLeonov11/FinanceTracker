using FinanceTracker.Application.Authentication.Models;
using FinanceTracker.Application.Common.Exceptions;
using FinanceTracker.Application.Common.Interfaces;
using FinanceTracker.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Application.Authentication.Commands.Login;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResult>
{
    private readonly IApplicationDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwt;

    public LoginCommandHandler(IApplicationDbContext db, IPasswordHasher passwordHasher, IJwtTokenService jwt)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _jwt = jwt;
    }

    public async Task<AuthResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        Email email;
        try
        {
            email = Email.Of(request.Email);
        }
        catch
        {
            throw new ForbiddenAccessException("Неверный email или пароль.", true);
        }

        var user = await _db.Users
            .Include(u => u.RefreshTokens)
            .SingleOrDefaultAsync(u => u.Email == email, cancellationToken);

        var passwordHash = user?.PasswordHash ?? "dummy";
        if (!_passwordHasher.Verify(request.Password, passwordHash) || user is null)
            throw new ForbiddenAccessException("Неверный email или пароль.", true);

        var access = _jwt.IssueAccessToken(user);
        var refresh = _jwt.IssueRefreshToken();
        user.IssueRefreshToken(refresh.Hash, refresh.ExpiresAt);

        await _db.SaveChangesAsync(cancellationToken);

        return new AuthResult(
            user.Id,
            user.Email.Value,
            user.DisplayName,
            access.Value,
            access.ExpiresAt,
            refresh.RawValue,
            refresh.ExpiresAt);
    }
}
