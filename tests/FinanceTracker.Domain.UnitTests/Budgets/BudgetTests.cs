using FinanceTracker.Domain.Budgets;
using FinanceTracker.Domain.Exceptions;
using FinanceTracker.Domain.ValueObjects;

namespace FinanceTracker.Domain.UnitTests.Budgets;

public class BudgetTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Currency Byn = Currency.Of("BYN");
    private static readonly Currency Usd = Currency.Of("USD");
    private static readonly DateOnly StartDate = new(2026, 5, 1);

    private static Budget NewBudget(decimal limit = 1000m, Currency? currency = null) =>
        Budget.Create(UserId, "Еда", BudgetPeriod.Month, Money.Of(limit, currency ?? Byn), StartDate);

    [Fact]
    public void Create_should_initialize_fields()
    {
        var b = NewBudget();
        b.Name.Should().Be("Еда");
        b.Limit.Amount.Should().Be(1000m);
        b.Period.Should().Be(BudgetPeriod.Month);
        b.StartDate.Should().Be(StartDate);
        b.IsClosed.Should().BeFalse();
        b.Rollover.Should().BeFalse();
    }

    [Fact]
    public void Create_with_zero_limit_should_throw()
    {
        var act = () => Budget.Create(UserId, "X", BudgetPeriod.Month, Money.Of(0m, Byn), StartDate);
        act.Should().Throw<DomainException>().WithMessage("*положительным*");
    }

    [Fact]
    public void Create_with_empty_name_should_throw()
    {
        var act = () => Budget.Create(UserId, " ", BudgetPeriod.Month, Money.Of(100m, Byn), StartDate);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void UpdateLimit_same_currency_should_succeed()
    {
        var b = NewBudget(500m);
        b.UpdateLimit(Money.Of(750m, Byn));
        b.Limit.Amount.Should().Be(750m);
    }

    [Fact]
    public void UpdateLimit_with_different_currency_should_throw()
    {
        var b = NewBudget(500m, Byn);
        var act = () => b.UpdateLimit(Money.Of(750m, Usd));
        act.Should().Throw<CurrencyMismatchException>();
    }

    [Fact]
    public void UpdateLimit_to_zero_should_throw()
    {
        var b = NewBudget();
        var act = () => b.UpdateLimit(Money.Of(0m, Byn));
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Close_should_set_IsClosed_and_EndDate()
    {
        var b = NewBudget();
        var closedAt = new DateTimeOffset(2026, 6, 15, 12, 0, 0, TimeSpan.Zero);
        b.Close(closedAt);
        b.IsClosed.Should().BeTrue();
        b.EndDate.Should().Be(DateOnly.FromDateTime(closedAt.UtcDateTime));
    }

    [Fact]
    public void Close_should_be_idempotent()
    {
        var b = NewBudget();
        b.Close(DateTimeOffset.UtcNow);
        b.Close(DateTimeOffset.UtcNow); // no-op
        b.IsClosed.Should().BeTrue();
    }

    [Fact]
    public void Reopen_should_clear_end_date()
    {
        var b = NewBudget();
        b.Close(DateTimeOffset.UtcNow);
        b.Reopen();
        b.IsClosed.Should().BeFalse();
        b.EndDate.Should().BeNull();
    }

    [Fact]
    public void Rename_closed_should_throw()
    {
        var b = NewBudget();
        b.Close(DateTimeOffset.UtcNow);
        var act = () => b.Rename("Новое имя");
        act.Should().Throw<DomainException>().WithMessage("*закрыт*");
    }

    [Fact]
    public void UpdateLimit_closed_should_throw()
    {
        var b = NewBudget();
        b.Close(DateTimeOffset.UtcNow);
        var act = () => b.UpdateLimit(Money.Of(2000m, Byn));
        act.Should().Throw<DomainException>().WithMessage("*закрыт*");
    }
}
