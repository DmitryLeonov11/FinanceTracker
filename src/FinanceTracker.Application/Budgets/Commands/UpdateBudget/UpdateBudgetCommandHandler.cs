using FinanceTracker.Application.Budgets.Commands.CreateBudget;
using FinanceTracker.Application.Budgets.Models;
using FinanceTracker.Application.Common.Exceptions;
using FinanceTracker.Application.Common.Interfaces;
using FinanceTracker.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Application.Budgets.Commands.UpdateBudget;

public sealed class UpdateBudgetCommandHandler : IRequestHandler<UpdateBudgetCommand, BudgetDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IRealtimeNotifier _notifier;

    public UpdateBudgetCommandHandler(IApplicationDbContext db, ICurrentUser currentUser, IRealtimeNotifier notifier)
    {
        _db = db;
        _currentUser = currentUser;
        _notifier = notifier;
    }

    public async Task<BudgetDto> Handle(UpdateBudgetCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.RequireUserId();

        var budget = await _db.Budgets
            .SingleOrDefaultAsync(b => b.Id == request.Id && b.UserId == userId, cancellationToken)
            ?? throw new NotFoundException("Бюджет", request.Id);

        if (request.Name is not null)
            budget.Rename(request.Name);

        if (request.Limit.HasValue)
        {
            var newLimit = Money.Of(request.Limit.Value, budget.Limit.Currency);
            budget.UpdateLimit(newLimit);
        }

        await _db.SaveChangesAsync(cancellationToken);

        await _notifier.NotifyUserAsync(
            userId,
            "budget.updated",
            new { budget.Id, budget.Name, Limit = budget.Limit.Amount },
            cancellationToken);

        return CreateBudgetCommandHandler.ToDto(budget);
    }
}
