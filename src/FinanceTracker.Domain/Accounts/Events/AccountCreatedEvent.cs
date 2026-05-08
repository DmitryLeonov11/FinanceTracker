using FinanceTracker.Domain.Common;

namespace FinanceTracker.Domain.Accounts.Events;

public sealed record AccountCreatedEvent(
    Guid AccountId,
    Guid UserId,
    string Name,
    AccountType Type,
    decimal InitialBalance,
    string Currency) : DomainEvent;
