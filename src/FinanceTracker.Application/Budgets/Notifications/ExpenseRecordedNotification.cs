using MediatR;

namespace FinanceTracker.Application.Budgets.Notifications;

/// <summary>
/// In-memory notification fired after an Expense transaction has been committed.
/// Carries everything the budget-threshold handler needs without re-reading the transaction.
/// </summary>
public sealed record ExpenseRecordedNotification(
    Guid UserId,
    Guid? CategoryId,
    string Currency,
    decimal Amount,
    DateTimeOffset OccurredAt) : INotification;
