using FinanceTracker.Domain.Common;
using FinanceTracker.Domain.Exceptions;
using FinanceTracker.Domain.Transactions.Events;
using FinanceTracker.Domain.ValueObjects;

namespace FinanceTracker.Domain.Transactions;

public sealed class Transaction : AggregateRoot
{
    public Guid UserId { get; private set; }
    public Guid AccountId { get; private set; }
    public Guid? CounterAccountId { get; private set; }
    public Guid? CategoryId { get; private set; }
    public TransactionType Type { get; private set; }
    public Money Amount { get; private set; } = null!;
    public DateTimeOffset OccurredAt { get; private set; }
    public string? Note { get; private set; }
    public Guid? TransferGroupId { get; private set; }
    public bool IsDeleted { get; private set; }

    private Transaction() { }

    private Transaction(
        Guid id,
        Guid userId,
        Guid accountId,
        Guid? counterAccountId,
        Guid? categoryId,
        TransactionType type,
        Money amount,
        DateTimeOffset occurredAt,
        string? note,
        Guid? transferGroupId)
        : base(id)
    {
        UserId = userId;
        AccountId = accountId;
        CounterAccountId = counterAccountId;
        CategoryId = categoryId;
        Type = type;
        Amount = amount;
        OccurredAt = occurredAt;
        Note = note;
        TransferGroupId = transferGroupId;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public static Transaction RecordIncome(
        Guid userId,
        Guid accountId,
        Guid? categoryId,
        Money amount,
        DateTimeOffset occurredAt,
        string? note)
    {
        ValidatePositive(amount);
        var tx = new Transaction(Guid.NewGuid(), userId, accountId, null, categoryId, TransactionType.Income, amount, occurredAt, NormalizeNote(note), transferGroupId: null);
        tx.Raise(new TransactionRecordedEvent(tx.Id, userId, accountId, TransactionType.Income, amount.Amount, amount.Currency.Code, occurredAt));
        return tx;
    }

    public static Transaction RecordExpense(
        Guid userId,
        Guid accountId,
        Guid? categoryId,
        Money amount,
        DateTimeOffset occurredAt,
        string? note)
    {
        ValidatePositive(amount);
        var tx = new Transaction(Guid.NewGuid(), userId, accountId, null, categoryId, TransactionType.Expense, amount, occurredAt, NormalizeNote(note), transferGroupId: null);
        tx.Raise(new TransactionRecordedEvent(tx.Id, userId, accountId, TransactionType.Expense, amount.Amount, amount.Currency.Code, occurredAt));
        return tx;
    }

    public static (Transaction Outgoing, Transaction Incoming) RecordTransfer(
        Guid userId,
        Guid sourceAccountId,
        Guid destinationAccountId,
        Money sourceAmount,
        Money destinationAmount,
        DateTimeOffset occurredAt,
        string? note)
    {
        if (sourceAccountId == destinationAccountId)
            throw new DomainException("Счёт-источник и счёт-получатель должны различаться.");
        ValidatePositive(sourceAmount);
        ValidatePositive(destinationAmount);

        var groupId = Guid.NewGuid();
        var noteNormalized = NormalizeNote(note);

        var outgoing = new Transaction(
            Guid.NewGuid(), userId, sourceAccountId, destinationAccountId, null,
            TransactionType.Transfer, sourceAmount, occurredAt, noteNormalized, groupId);
        var incoming = new Transaction(
            Guid.NewGuid(), userId, destinationAccountId, sourceAccountId, null,
            TransactionType.Transfer, destinationAmount, occurredAt, noteNormalized, groupId);

        outgoing.Raise(new TransactionRecordedEvent(outgoing.Id, userId, sourceAccountId, TransactionType.Transfer, sourceAmount.Amount, sourceAmount.Currency.Code, occurredAt));
        incoming.Raise(new TransactionRecordedEvent(incoming.Id, userId, destinationAccountId, TransactionType.Transfer, destinationAmount.Amount, destinationAmount.Currency.Code, occurredAt));
        return (outgoing, incoming);
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private static void ValidatePositive(Money amount)
    {
        Guard.NotNull(amount, nameof(amount));
        if (!amount.IsPositive)
            throw new DomainException("Сумма транзакции должна быть положительной.");
    }

    private static string? NormalizeNote(string? note)
    {
        if (note is null) return null;
        var trimmed = note.Trim();
        return trimmed.Length == 0 ? null : Guard.MaxLength(trimmed, 500, nameof(note));
    }
}
