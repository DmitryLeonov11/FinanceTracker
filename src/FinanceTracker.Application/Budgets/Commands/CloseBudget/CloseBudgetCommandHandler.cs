using FinanceTracker.Application.Common.Exceptions;
using FinanceTracker.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Application.Budgets.Commands.CloseBudget;

public sealed class CloseBudgetCommandHandler : IRequestHandler<CloseBudgetCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IDateTime _clock;
    private readonly IRealtimeNotifier _notifier;

    public CloseBudgetCommandHandler(IApplicationDbContext db, ICurrentUser currentUser, IDateTime clock, IRealtimeNotifier notifier)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
        _notifier = notifier;
    }

    public async Task Handle(CloseBudgetCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.RequireUserId();

        var budget = await _db.Budgets
            .SingleOrDefaultAsync(b => b.Id == request.Id && b.UserId == userId, cancellationToken)
            ?? throw new NotFoundException("Бюджет", request.Id);

        budget.Close(_clock.UtcNow);
        await _db.SaveChangesAsync(cancellationToken);

        await _notifier.NotifyUserAsync(userId, "budget.closed", new { budget.Id }, cancellationToken);
    }
}
