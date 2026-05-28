using FinanceTracker.Application.Budgets.Models;
using FinanceTracker.Application.Common.Exceptions;
using FinanceTracker.Application.Common.Interfaces;
using FinanceTracker.Domain.Budgets;
using FinanceTracker.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Application.Budgets.Commands.CreateBudget;

public sealed class CreateBudgetCommandHandler : IRequestHandler<CreateBudgetCommand, BudgetDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IRealtimeNotifier _notifier;

    public CreateBudgetCommandHandler(IApplicationDbContext db, ICurrentUser currentUser, IRealtimeNotifier notifier)
    {
        _db = db;
        _currentUser = currentUser;
        _notifier = notifier;
    }

    public async Task<BudgetDto> Handle(CreateBudgetCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.RequireUserId();

        if (request.CategoryId is { } catId)
        {
            var categoryExists = await _db.Categories
                .AsNoTracking()
                .AnyAsync(c => c.Id == catId && c.UserId == userId && !c.IsDeleted, cancellationToken);
            if (!categoryExists)
                throw new NotFoundException("Категория", catId);
        }

        var limit = Money.Of(request.Limit, Currency.Of(request.Currency));
        var budget = Budget.Create(userId, request.Name, request.Period, limit, request.StartDate, request.CategoryId, request.Rollover);

        _db.Budgets.Add(budget);
        await _db.SaveChangesAsync(cancellationToken);

        await _notifier.NotifyUserAsync(
            userId,
            "budget.created",
            new { budget.Id, budget.Name, budget.Period, Currency = budget.Limit.Currency.Code, Limit = budget.Limit.Amount },
            cancellationToken);

        return ToDto(budget);
    }

    internal static BudgetDto ToDto(Budget b) => new(
        b.Id,
        b.Name,
        b.CategoryId,
        b.Period,
        b.Limit.Currency.Code,
        b.Limit.Amount,
        b.StartDate,
        b.EndDate,
        b.Rollover,
        b.IsClosed,
        b.CreatedAt);
}
