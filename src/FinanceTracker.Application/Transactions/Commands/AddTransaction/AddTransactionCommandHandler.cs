using FinanceTracker.Application.Common.Exceptions;
using FinanceTracker.Application.Common.Interfaces;
using FinanceTracker.Application.Transactions.Models;
using FinanceTracker.Domain.Transactions;
using FinanceTracker.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Application.Transactions.Commands.AddTransaction;

public sealed class AddTransactionCommandHandler : IRequestHandler<AddTransactionCommand, TransactionDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IRealtimeNotifier _notifier;

    public AddTransactionCommandHandler(
        IApplicationDbContext db,
        ICurrentUser currentUser,
        IRealtimeNotifier notifier)
    {
        _db = db;
        _currentUser = currentUser;
        _notifier = notifier;
    }

    public async Task<TransactionDto> Handle(AddTransactionCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.RequireUserId();

        var account = await _db.Accounts
            .SingleOrDefaultAsync(a => a.Id == request.AccountId && a.UserId == userId, cancellationToken)
            ?? throw new NotFoundException("Счёт", request.AccountId);

        if (request.CategoryId is { } categoryId)
        {
            var categoryExists = await _db.Categories
                .AnyAsync(c => c.Id == categoryId && c.UserId == userId && !c.IsDeleted, cancellationToken);
            if (!categoryExists)
                throw new NotFoundException("Категория", categoryId);
        }

        var amount = Money.Of(request.Amount, account.Currency);

        var transaction = request.Type switch
        {
            TransactionType.Income => Transaction.RecordIncome(userId, account.Id, request.CategoryId, amount, request.OccurredAt, request.Note),
            TransactionType.Expense => Transaction.RecordExpense(userId, account.Id, request.CategoryId, amount, request.OccurredAt, request.Note),
            _ => throw new InvalidOperationException("Переводы не поддерживаются этой командой.")
        };

        var delta = request.Type == TransactionType.Income ? amount : amount.Negate();
        account.Apply(delta);

        _db.Transactions.Add(transaction);
        await _db.SaveChangesAsync(cancellationToken);

        await _notifier.NotifyUserAsync(
            userId,
            "transaction.created",
            new { transaction.Id, transaction.AccountId, transaction.Type, Amount = amount.Amount, Currency = amount.Currency.Code },
            cancellationToken);

        await _notifier.NotifyUserAsync(
            userId,
            "account.balance-changed",
            new { AccountId = account.Id, Balance = account.Balance.Amount, Currency = account.Currency.Code },
            cancellationToken);

        return new TransactionDto(
            transaction.Id,
            transaction.AccountId,
            transaction.CounterAccountId,
            transaction.CategoryId,
            transaction.Type,
            transaction.Amount.Amount,
            transaction.Amount.Currency.Code,
            transaction.OccurredAt,
            transaction.Note);
    }
}
