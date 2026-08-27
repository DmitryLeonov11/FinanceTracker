using MediatR;

namespace FinanceTracker.Application.Budgets.Notifications;

public sealed record ExpenseRecordedNotification(
    Guid UserId,
    Guid? CategoryId,
    string Currency,
    decimal Amount,
    DateTimeOffset OccurredAt) : INotification;
