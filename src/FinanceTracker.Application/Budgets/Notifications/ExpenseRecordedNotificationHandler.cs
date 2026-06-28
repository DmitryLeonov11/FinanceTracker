using FinanceTracker.Application.Budgets.Helpers;
using FinanceTracker.Application.Common.Interfaces;
using FinanceTracker.Domain.Transactions;
using FinanceTracker.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinanceTracker.Application.Budgets.Notifications;

/// <summary>
/// For each active budget that this expense affects, evaluates whether the
/// spent total crossed a 50/80/100% threshold upward. Pushes a SignalR event
/// for the frontend to surface a toast. Errors are swallowed so a failed
/// notification never breaks the originating mutation.
/// </summary>
public sealed class ExpenseRecordedNotificationHandler : INotificationHandler<ExpenseRecordedNotification>
{
    private readonly IApplicationDbContext _db;
    private readonly IRealtimeNotifier _notifier;
    private readonly ILogger<ExpenseRecordedNotificationHandler> _logger;

    public ExpenseRecordedNotificationHandler(
        IApplicationDbContext db,
        IRealtimeNotifier notifier,
        ILogger<ExpenseRecordedNotificationHandler> logger)
    {
        _db = db;
        _notifier = notifier;
        _logger = logger;
    }

    public async Task Handle(ExpenseRecordedNotification notification, CancellationToken cancellationToken)
    {
        try
        {
            await EvaluateAsync(notification, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Не удалось вычислить пороги бюджета после расхода {Amount} {Currency}",
                notification.Amount, notification.Currency);
        }
    }

    private async Task EvaluateAsync(ExpenseRecordedNotification notification, CancellationToken cancellationToken)
    {
        var budgets = await _db.Budgets
            .AsNoTracking()
            .Where(b => b.UserId == notification.UserId)
            .Where(b => !b.IsClosed)
            .Where(b => b.Limit.Currency == Currency.Of(notification.Currency))
            .Where(b => b.CategoryId == null || b.CategoryId == notification.CategoryId)
            .ToListAsync(cancellationToken);

        if (budgets.Count == 0) return;

        foreach (var budget in budgets)
        {
            var (from, to) = BudgetPeriodCalculator.GetCurrentWindow(budget.Period, budget.StartDate, notification.OccurredAt);
            if (notification.OccurredAt < from || notification.OccurredAt > to) continue;

            var currency = budget.Limit.Currency;
            var currencyCode = currency.Code;
            var newSpent = await _db.Transactions
                .AsNoTracking()
                .Where(t => t.UserId == notification.UserId)
                .Where(t => t.Type == TransactionType.Expense)
                .Where(t => t.Amount.Currency == currency)
                .Where(t => t.OccurredAt >= from && t.OccurredAt <= to)
                .Where(t => budget.CategoryId == null || t.CategoryId == budget.CategoryId)
                .SumAsync(t => (decimal?)t.Amount.Amount, cancellationToken) ?? 0m;

            var previousSpent = newSpent - notification.Amount;
            var limit = budget.Limit.Amount;
            var crossed = BudgetThresholdEvaluator.CrossedThresholds(previousSpent, newSpent, limit);
            if (crossed.Count == 0) continue;

            var progressPercent = limit > 0
                ? decimal.Round(newSpent / limit * 100m, 1, MidpointRounding.AwayFromZero)
                : 0m;

            foreach (var threshold in crossed)
            {
                await _notifier.NotifyUserAsync(
                    notification.UserId,
                    "budget.threshold-reached",
                    new
                    {
                        BudgetId = budget.Id,
                        budget.Name,
                        Threshold = threshold,
                        Spent = decimal.Round(newSpent, 2, MidpointRounding.AwayFromZero),
                        Limit = limit,
                        Currency = currencyCode,
                        ProgressPercent = progressPercent
                    },
                    cancellationToken);
            }
        }
    }
}
