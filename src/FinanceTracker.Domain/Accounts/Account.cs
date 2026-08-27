using FinanceTracker.Domain.Accounts.Events;
using FinanceTracker.Domain.Common;
using FinanceTracker.Domain.Exceptions;
using FinanceTracker.Domain.ValueObjects;

namespace FinanceTracker.Domain.Accounts;

public sealed class Account : AggregateRoot
{
    public Guid UserId { get; private set; }
    public string Name { get; private set; } = null!;
    public AccountType Type { get; private set; }
    public Currency Currency { get; private set; } = null!;
    public Money Balance { get; private set; } = null!;
    public bool IsArchived { get; private set; }

    private Account() { }

    private Account(
        Guid id,
        Guid userId,
        string name,
        AccountType type,
        Money initialBalance)
        : base(id)
    {
        UserId = userId;
        Name = name;
        Type = type;
        Currency = initialBalance.Currency;
        Balance = initialBalance;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public static Account Open(
        Guid userId,
        string name,
        AccountType type,
        Money initialBalance)
    {
        Guard.MaxLength(Guard.NotNullOrWhiteSpace(name, nameof(name)), 100, nameof(name));
        Guard.NotNull(initialBalance, nameof(initialBalance));
        if (initialBalance.IsNegative)
            throw new DomainException("Начальный баланс не может быть отрицательным.");

        var account = new Account(Guid.NewGuid(), userId, name, type, initialBalance);
        account.Raise(new AccountCreatedEvent(account.Id, userId, name, type, initialBalance.Amount, initialBalance.Currency.Code));
        return account;
    }

    public void Apply(Money delta)
    {
        EnsureNotArchived();
        if (!delta.Currency.Equals(Currency))
            throw new CurrencyMismatchException(Currency.Code, delta.Currency.Code);

        var previous = Balance;
        Balance = Balance.Add(delta);
        UpdatedAt = DateTimeOffset.UtcNow;
        Version++;

        Raise(new BalanceChangedEvent(Id, UserId, previous.Amount, Balance.Amount, Currency.Code));
    }

    public void Rename(string name)
    {
        EnsureNotArchived();
        Name = Guard.MaxLength(Guard.NotNullOrWhiteSpace(name, nameof(name)), 100, nameof(name));
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Archive()
    {
        if (IsArchived) return;
        IsArchived = true;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private void EnsureNotArchived()
    {
        if (IsArchived) throw new DomainException("Счёт находится в архиве.");
    }
}
