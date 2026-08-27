using FinanceTracker.Domain.Common;

namespace FinanceTracker.Domain.Transactions.Events;

public sealed record TransactionRecordedEvent(
    Guid TransactionId,
    Guid UserId,
    Guid AccountId,
    TransactionType Type,
    decimal Amount,
    string Currency,
    DateTimeOffset TransactionDate) : DomainEvent;
