using FinanceTracker.Domain.Common;

namespace FinanceTracker.Domain.Accounts.Events;

public sealed record BalanceChangedEvent(
    Guid AccountId,
    Guid UserId,
    decimal PreviousBalance,
    decimal NewBalance,
    string Currency) : DomainEvent;
