using FinanceTracker.Application.Accounts.Models;
using FinanceTracker.Application.Common.Exceptions;
using FinanceTracker.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Application.Accounts.Commands.RenameAccount;

public sealed class RenameAccountCommandHandler : IRequestHandler<RenameAccountCommand, AccountDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IRealtimeNotifier _notifier;

    public RenameAccountCommandHandler(IApplicationDbContext db, ICurrentUser currentUser, IRealtimeNotifier notifier)
    {
        _db = db;
        _currentUser = currentUser;
        _notifier = notifier;
    }

    public async Task<AccountDto> Handle(RenameAccountCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.RequireUserId();

        var account = await _db.Accounts
            .SingleOrDefaultAsync(a => a.Id == request.Id && a.UserId == userId, cancellationToken)
            ?? throw new NotFoundException("Счёт", request.Id);

        account.Rename(request.Name);
        await _db.SaveChangesAsync(cancellationToken);

        await _notifier.NotifyUserAsync(
            userId,
            "account.renamed",
            new { account.Id, account.Name },
            cancellationToken);

        return new AccountDto(
            account.Id,
            account.Name,
            account.Type,
            account.Currency.Code,
            account.Balance.Amount,
            account.IsArchived,
            account.CreatedAt);
    }
}
