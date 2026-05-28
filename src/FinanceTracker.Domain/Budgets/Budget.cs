using FinanceTracker.Domain.Common;
using FinanceTracker.Domain.Exceptions;
using FinanceTracker.Domain.ValueObjects;

namespace FinanceTracker.Domain.Budgets;

public sealed class Budget : AggregateRoot
{
    public Guid UserId { get; private set; }
    public string Name { get; private set; } = null!;
    public Guid? CategoryId { get; private set; }
    public BudgetPeriod Period { get; private set; }
    public Money Limit { get; private set; } = null!;
    public DateOnly StartDate { get; private set; }
    public DateOnly? EndDate { get; private set; }
    public bool Rollover { get; private set; }
    public bool IsClosed { get; private set; }

    private Budget() { }

    private Budget(
        Guid id,
        Guid userId,
        string name,
        Guid? categoryId,
        BudgetPeriod period,
        Money limit,
        DateOnly startDate,
        bool rollover) : base(id)
    {
        UserId = userId;
        Name = name;
        CategoryId = categoryId;
        Period = period;
        Limit = limit;
        StartDate = startDate;
        Rollover = rollover;
        IsClosed = false;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public static Budget Create(
        Guid userId,
        string name,
        BudgetPeriod period,
        Money limit,
        DateOnly startDate,
        Guid? categoryId = null,
        bool rollover = false)
    {
        Guard.MaxLength(Guard.NotNullOrWhiteSpace(name, nameof(name)), 100, nameof(name));
        Guard.NotNull(limit, nameof(limit));
        if (!limit.IsPositive)
            throw new DomainException("Лимит бюджета должен быть положительным.");
        if (!Enum.IsDefined(period))
            throw new DomainException("Неизвестный период бюджета.");

        return new Budget(Guid.NewGuid(), userId, name, categoryId, period, limit, startDate, rollover);
    }

    public void Rename(string name)
    {
        EnsureMutable();
        Name = Guard.MaxLength(Guard.NotNullOrWhiteSpace(name, nameof(name)), 100, nameof(name));
        UpdatedAt = DateTimeOffset.UtcNow;
        Version++;
    }

    public void UpdateLimit(Money newLimit)
    {
        EnsureMutable();
        Guard.NotNull(newLimit, nameof(newLimit));
        if (!newLimit.IsPositive)
            throw new DomainException("Лимит бюджета должен быть положительным.");
        if (!newLimit.Currency.Equals(Limit.Currency))
            throw new CurrencyMismatchException(Limit.Currency.Code, newLimit.Currency.Code);

        Limit = newLimit;
        UpdatedAt = DateTimeOffset.UtcNow;
        Version++;
    }

    public void Close(DateTimeOffset closedAt)
    {
        if (IsClosed) return;
        IsClosed = true;
        EndDate = DateOnly.FromDateTime(closedAt.UtcDateTime);
        UpdatedAt = closedAt;
        Version++;
    }

    public void Reopen()
    {
        if (!IsClosed) return;
        IsClosed = false;
        EndDate = null;
        UpdatedAt = DateTimeOffset.UtcNow;
        Version++;
    }

    private void EnsureMutable()
    {
        if (IsClosed) throw new DomainException("Бюджет закрыт и не может быть изменён.");
    }
}
