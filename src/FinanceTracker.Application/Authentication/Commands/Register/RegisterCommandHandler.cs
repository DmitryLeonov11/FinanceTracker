using FinanceTracker.Application.Authentication.Models;
using FinanceTracker.Application.Common.Interfaces;
using FinanceTracker.Domain.Users;
using FinanceTracker.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Application.Authentication.Commands.Register;

public sealed class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResult>
{
    private readonly IApplicationDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwt;

    public RegisterCommandHandler(IApplicationDbContext db, IPasswordHasher passwordHasher, IJwtTokenService jwt)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _jwt = jwt;
    }

    public async Task<AuthResult> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var email = Email.Of(request.Email);
        var currency = Currency.Of(request.DisplayCurrency);

        var emailExists = await _db.Users
            .AsNoTracking()
            .AnyAsync(u => u.Email == email, cancellationToken);

        if (emailExists)
            throw new Common.Exceptions.ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.Email), "Этот email уже используется.")
            });

        var passwordHash = _passwordHasher.Hash(request.Password);
        var user = User.Register(email, passwordHash, request.DisplayName, currency);

        var access = _jwt.IssueAccessToken(user);
        var refresh = _jwt.IssueRefreshToken();
        user.IssueRefreshToken(refresh.Hash, refresh.ExpiresAt);

        _db.Users.Add(user);
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
