using FinanceTracker.Domain.Exceptions;
using FinanceTracker.Domain.Transactions;
using FinanceTracker.Domain.ValueObjects;

namespace FinanceTracker.Domain.UnitTests.Transactions;

public class TransactionTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid AccountId = Guid.NewGuid();
    private static readonly Guid OtherAccountId = Guid.NewGuid();
    private static readonly Currency Byn = Currency.Of("BYN");
    private static readonly Currency Usd = Currency.Of("USD");
    private static readonly DateTimeOffset Now = DateTimeOffset.UtcNow;

    [Fact]
    public void RecordIncome_should_have_positive_amount_and_Income_type()
    {
        var tx = Transaction.RecordIncome(UserId, AccountId, null, Money.Of(50m, Byn), Now, null);
        tx.Type.Should().Be(TransactionType.Income);
        tx.Amount.Amount.Should().Be(50m);
        tx.AccountId.Should().Be(AccountId);
    }

    [Fact]
    public void RecordIncome_with_zero_amount_should_throw()
    {
        var act = () => Transaction.RecordIncome(UserId, AccountId, null, Money.Of(0m, Byn), Now, null);
        act.Should().Throw<DomainException>().WithMessage("*положительной*");
    }

    [Fact]
    public void RecordExpense_should_create_Expense_type()
    {
        var tx = Transaction.RecordExpense(UserId, AccountId, null, Money.Of(30m, Byn), Now, "обед");
        tx.Type.Should().Be(TransactionType.Expense);
        tx.Note.Should().Be("обед");
    }

    [Fact]
    public void RecordTransfer_should_return_linked_pair_with_same_group_id()
    {
        var (outgoing, incoming) = Transaction.RecordTransfer(
            UserId, AccountId, OtherAccountId,
            Money.Of(100m, Byn), Money.Of(100m, Byn), Now, null);

        outgoing.Type.Should().Be(TransactionType.Transfer);
        incoming.Type.Should().Be(TransactionType.Transfer);
        outgoing.TransferGroupId.Should().Be(incoming.TransferGroupId);
        outgoing.TransferGroupId.Should().NotBeNull();
        outgoing.AccountId.Should().Be(AccountId);
        incoming.AccountId.Should().Be(OtherAccountId);
    }

    [Fact]
    public void RecordTransfer_same_account_should_throw()
    {
        var act = () => Transaction.RecordTransfer(
            UserId, AccountId, AccountId,
            Money.Of(10m, Byn), Money.Of(10m, Byn), Now, null);
        act.Should().Throw<DomainException>().WithMessage("*различаться*");
    }

    [Fact]
    public void RecordTransfer_cross_currency_should_use_independent_amounts()
    {
        var (outgoing, incoming) = Transaction.RecordTransfer(
            UserId, AccountId, OtherAccountId,
            Money.Of(100m, Usd), Money.Of(327m, Byn), Now, null);

        outgoing.Amount.Currency.Code.Should().Be("USD");
        outgoing.Amount.Amount.Should().Be(100m);
        incoming.Amount.Currency.Code.Should().Be("BYN");
        incoming.Amount.Amount.Should().Be(327m);
    }

    [Fact]
    public void Edit_should_update_fields_when_currency_matches()
    {
        var tx = Transaction.RecordExpense(UserId, AccountId, null, Money.Of(25m, Byn), Now, "old");
        tx.Edit(Money.Of(40m, Byn), Guid.NewGuid(), Now.AddDays(-1), "new");
        tx.Amount.Amount.Should().Be(40m);
        tx.Note.Should().Be("new");
    }

    [Fact]
    public void Edit_with_different_currency_should_throw()
    {
        var tx = Transaction.RecordExpense(UserId, AccountId, null, Money.Of(25m, Byn), Now, null);
        var act = () => tx.Edit(Money.Of(40m, Usd), null, Now, null);
        act.Should().Throw<CurrencyMismatchException>();
    }

    [Fact]
    public void Edit_of_transfer_should_throw()
    {
        var (outgoing, _) = Transaction.RecordTransfer(
            UserId, AccountId, OtherAccountId,
            Money.Of(50m, Byn), Money.Of(50m, Byn), Now, null);

        var act = () => outgoing.Edit(Money.Of(60m, Byn), null, Now, null);
        act.Should().Throw<DomainException>().WithMessage("*Переводы*");
    }

    [Fact]
    public void Edit_of_deleted_should_throw()
    {
        var tx = Transaction.RecordExpense(UserId, AccountId, null, Money.Of(25m, Byn), Now, null);
        tx.SoftDelete();
        var act = () => tx.Edit(Money.Of(30m, Byn), null, Now, null);
        act.Should().Throw<DomainException>().WithMessage("*Удалённая*");
    }

    [Fact]
    public void SoftDelete_should_mark_deleted_without_removing_fields()
    {
        var tx = Transaction.RecordIncome(UserId, AccountId, null, Money.Of(10m, Byn), Now, null);
        tx.SoftDelete();
        tx.IsDeleted.Should().BeTrue();
        tx.Amount.Amount.Should().Be(10m); // data preserved
    }
}
