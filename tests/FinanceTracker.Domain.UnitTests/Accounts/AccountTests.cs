using FinanceTracker.Domain.Accounts;
using FinanceTracker.Domain.Exceptions;
using FinanceTracker.Domain.ValueObjects;

namespace FinanceTracker.Domain.UnitTests.Accounts;

public class AccountTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Currency Byn = Currency.Of("BYN");
    private static readonly Currency Usd = Currency.Of("USD");

    private static Account NewAccount(decimal initialBalance = 100m, Currency? currency = null) =>
        Account.Open(UserId, "Test", AccountType.Bank, Money.Of(initialBalance, currency ?? Byn));

    [Fact]
    public void Open_should_set_initial_state()
    {
        var account = NewAccount(150m);
        account.Name.Should().Be("Test");
        account.Type.Should().Be(AccountType.Bank);
        account.Balance.Amount.Should().Be(150m);
        account.Currency.Should().Be(Byn);
        account.IsArchived.Should().BeFalse();
    }

    [Fact]
    public void Open_with_negative_initial_balance_should_throw()
    {
        var act = () => Account.Open(UserId, "X", AccountType.Cash, Money.Of(-10m, Byn));
        act.Should().Throw<DomainException>().WithMessage("*отрицательным*");
    }

    [Fact]
    public void Apply_positive_delta_should_increase_balance()
    {
        var account = NewAccount(100m);
        account.Apply(Money.Of(50m, Byn));
        account.Balance.Amount.Should().Be(150m);
    }

    [Fact]
    public void Apply_negative_delta_should_decrease_balance()
    {
        var account = NewAccount(100m);
        account.Apply(Money.Of(-30m, Byn));
        account.Balance.Amount.Should().Be(70m);
    }

    [Fact]
    public void Apply_with_different_currency_should_throw()
    {
        var account = NewAccount(100m, Byn);
        var act = () => account.Apply(Money.Of(10m, Usd));
        act.Should().Throw<CurrencyMismatchException>();
    }

    [Fact]
    public void Apply_to_archived_account_should_throw()
    {
        var account = NewAccount();
        account.Archive();
        var act = () => account.Apply(Money.Of(1m, Byn));
        act.Should().Throw<DomainException>().WithMessage("*архиве*");
    }

    [Fact]
    public void Rename_to_archived_account_should_throw()
    {
        var account = NewAccount();
        account.Archive();
        var act = () => account.Rename("New");
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Archive_should_be_idempotent()
    {
        var account = NewAccount();
        account.Archive();
        account.Archive(); // not throwing
        account.IsArchived.Should().BeTrue();
    }

    [Fact]
    public void Apply_should_bump_version()
    {
        var account = NewAccount();
        var initialVersion = account.Version;
        account.Apply(Money.Of(1m, Byn));
        account.Version.Should().BeGreaterThan(initialVersion);
    }

    [Fact]
    public void Open_should_raise_AccountCreatedEvent()
    {
        var account = NewAccount();
        account.DomainEvents.Should().ContainSingle(e => e.GetType().Name == "AccountCreatedEvent");
    }
}
