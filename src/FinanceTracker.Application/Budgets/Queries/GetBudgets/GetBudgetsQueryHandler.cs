using FinanceTracker.Application.Budgets.Helpers;
using FinanceTracker.Application.Budgets.Models;
using FinanceTracker.Application.Common.Interfaces;
using FinanceTracker.Domain.Transactions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Application.Budgets.Queries.GetBudgets;

public sealed class GetBudgetsQueryHandler : IRequestHandler<GetBudgetsQuery, IReadOnlyCollection<BudgetWithProgressDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IDateTime _clock;

    public GetBudgetsQueryHandler(IApplicationDbContext db, ICurrentUser currentUser, IDateTime clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<IReadOnlyCollection<BudgetWithProgressDto>> Handle(
        GetBudgetsQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.RequireUserId();
        var now = _clock.UtcNow;

        var budgetsQuery = _db.Budgets
            .AsNoTracking()
            .Where(b => b.UserId == userId);
        if (!request.IncludeClosed)
            budgetsQuery = budgetsQuery.Where(b => !b.IsClosed);

        var budgets = await budgetsQuery
            .OrderBy(b => b.IsClosed)
            .ThenByDescending(b => b.CreatedAt)
            .ToListAsync(cancellationToken);

        if (budgets.Count == 0)
            return Array.Empty<BudgetWithProgressDto>();

        // Precompute each budget's active window, then pull all candidate expenses in a single
        // query over their union range and aggregate per budget in memory — avoids the previous
        // N+1 (one SUM round-trip per budget).
        var windows = budgets
            .Select(b => BudgetPeriodCalculator.GetCurrentWindow(b.Period, b.StartDate, now))
            .ToArray();

        var rangeFrom = windows.Min(w => w.From);
        var rangeTo = windows.Max(w => w.To);

        var expenses = await _db.Transactions
            .AsNoTracking()
            .Where(t => t.UserId == userId)
            .Where(t => t.Type == TransactionType.Expense)
            .Where(t => t.OccurredAt >= rangeFrom && t.OccurredAt <= rangeTo)
            .Select(t => new ExpenseRow(
                t.OccurredAt,
                t.Amount.Currency.Code,
                t.Amount.Amount,
                t.CategoryId))
            .ToListAsync(cancellationToken);

        var result = new List<BudgetWithProgressDto>(budgets.Count);

        for (var i = 0; i < budgets.Count; i++)
        {
            var budget = budgets[i];
            var (from, to) = windows[i];
            var currencyCode = budget.Limit.Currency.Code;

            var spent = expenses
                .Where(e => e.CurrencyCode == currencyCode
                    && e.OccurredAt >= from && e.OccurredAt <= to
                    && (budget.CategoryId == null || e.CategoryId == budget.CategoryId))
                .Sum(e => e.Amount);

            var limit = budget.Limit.Amount;
            var remaining = decimal.Round(limit - spent, 2, MidpointRounding.AwayFromZero);
            var progressPercent = limit > 0
                ? decimal.Round(spent / limit * 100m, 1, MidpointRounding.AwayFromZero)
                : 0m;
            var isOverLimit = spent > limit;
            int? thresholdReached = progressPercent switch
            {
                >= 100m => 100,
                >= 80m => 80,
                >= 50m => 50,
                _ => null
            };

            result.Add(new BudgetWithProgressDto(
                budget.Id,
                budget.Name,
                budget.CategoryId,
                budget.Period,
                currencyCode,
                limit,
                budget.StartDate,
                budget.EndDate,
                budget.Rollover,
                budget.IsClosed,
                budget.CreatedAt,
                decimal.Round(spent, 2, MidpointRounding.AwayFromZero),
                remaining,
                progressPercent,
                isOverLimit,
                thresholdReached,
                from,
                to));
        }

        return result;
    }

    private sealed record ExpenseRow(
        DateTimeOffset OccurredAt,
        string CurrencyCode,
        decimal Amount,
        Guid? CategoryId);
}
