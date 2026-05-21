using FinanceTracker.Application.Common.Exceptions;
using FinanceTracker.Application.Common.Interfaces;
using FinanceTracker.Application.Transactions.Models;
using FinanceTracker.Domain.Transactions;
using FinanceTracker.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Application.Transactions.Commands.EditTransaction;

public sealed class EditTransactionCommandHandler : IRequestHandler<EditTransactionCommand, TransactionDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IRealtimeNotifier _notifier;

    public EditTransactionCommandHandler(
        IApplicationDbContext db,
        ICurrentUser currentUser,
        IRealtimeNotifier notifier)
    {
        _db = db;
        _currentUser = currentUser;
        _notifier = notifier;
    }

    public async Task<TransactionDto> Handle(EditTransactionCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.RequireUserId();

        var transaction = await _db.Transactions
            .SingleOrDefaultAsync(t => t.Id == request.TransactionId && t.UserId == userId, cancellationToken)
            ?? throw new NotFoundException("Операция", request.TransactionId);

        var account = await _db.Accounts
            .SingleOrDefaultAsync(a => a.Id == transaction.AccountId && a.UserId == userId, cancellationToken)
            ?? throw new NotFoundException("Счёт", transaction.AccountId);

        if (request.CategoryId is { } categoryId)
        {
            var categoryExists = await _db.Categories
                .AnyAsync(c => c.Id == categoryId && c.UserId == userId && !c.IsDeleted, cancellationToken);
            if (!categoryExists)
                throw new NotFoundException("Категория", categoryId);
        }

        // Reverse old delta from balance, then apply new amount via domain Edit, then apply new delta.
        var oldDelta = transaction.Type == TransactionType.Income
            ? transaction.Amount
            : transaction.Amount.Negate();
        account.Apply(oldDelta.Negate());

        var newAmount = Money.Of(request.Amount, account.Currency);
        transaction.Edit(newAmount, request.CategoryId, request.OccurredAt, request.Note);

        var newDelta = transaction.Type == TransactionType.Income
            ? newAmount
            : newAmount.Negate();
        account.Apply(newDelta);

        await _db.SaveChangesAsync(cancellationToken);

        await _notifier.NotifyUserAsync(
            userId,
            "transaction.updated",
            new { transaction.Id, transaction.AccountId, transaction.Type, Amount = newAmount.Amount, Currency = newAmount.Currency.Code },
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
