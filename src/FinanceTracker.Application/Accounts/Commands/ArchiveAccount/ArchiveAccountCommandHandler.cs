using FinanceTracker.Application.Common.Exceptions;
using FinanceTracker.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Application.Accounts.Commands.ArchiveAccount;

public sealed class ArchiveAccountCommandHandler : IRequestHandler<ArchiveAccountCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IRealtimeNotifier _notifier;

    public ArchiveAccountCommandHandler(IApplicationDbContext db, ICurrentUser currentUser, IRealtimeNotifier notifier)
    {
        _db = db;
        _currentUser = currentUser;
        _notifier = notifier;
    }

    public async Task Handle(ArchiveAccountCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.RequireUserId();

        var account = await _db.Accounts
            .SingleOrDefaultAsync(a => a.Id == request.Id && a.UserId == userId, cancellationToken)
            ?? throw new NotFoundException("Счёт", request.Id);

        account.Archive();
        await _db.SaveChangesAsync(cancellationToken);

        await _notifier.NotifyUserAsync(userId, "account.archived", new { account.Id }, cancellationToken);
    }
}
