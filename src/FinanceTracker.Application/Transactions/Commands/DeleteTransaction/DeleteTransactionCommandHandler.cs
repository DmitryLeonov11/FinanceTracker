using FinanceTracker.Application.Common.Exceptions;
using FinanceTracker.Application.Common.Interfaces;
using FinanceTracker.Domain.Exceptions;
using FinanceTracker.Domain.Transactions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Application.Transactions.Commands.DeleteTransaction;

public sealed class DeleteTransactionCommandHandler : IRequestHandler<DeleteTransactionCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IRealtimeNotifier _notifier;

    public DeleteTransactionCommandHandler(
        IApplicationDbContext db,
        ICurrentUser currentUser,
        IRealtimeNotifier notifier)
    {
        _db = db;
        _currentUser = currentUser;
        _notifier = notifier;
    }

    public async Task Handle(DeleteTransactionCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.RequireUserId();

        var transaction = await _db.Transactions
            .SingleOrDefaultAsync(t => t.Id == request.TransactionId && t.UserId == userId, cancellationToken)
            ?? throw new NotFoundException("Операция", request.TransactionId);

        if (transaction.Type == TransactionType.Transfer)
        {
            if (transaction.TransferGroupId == null)
                throw new DomainException("Для перевода отсутствует TransferGroupId.");

            var groupId = transaction.TransferGroupId.Value;
            var transfers = await _db.Transactions
                .Where(t => t.TransferGroupId == groupId && t.UserId == userId)
                .ToListAsync(cancellationToken);

            var outgoing = transfers.SingleOrDefault(t => t.IsOutgoing);
            var incoming = transfers.SingleOrDefault(t => !t.IsOutgoing);

            if (outgoing != null)
            {
                var sourceAccount = await _db.Accounts
                    .SingleOrDefaultAsync(a => a.Id == outgoing.AccountId && a.UserId == userId, cancellationToken);
                if (sourceAccount != null)
                {
                    sourceAccount.Apply(outgoing.Amount);
                    await _notifier.NotifyUserAsync(
                        userId,
                        "account.balance-changed",
                        new { AccountId = sourceAccount.Id, Balance = sourceAccount.Balance.Amount, Currency = sourceAccount.Currency.Code },
                        cancellationToken);
                }
                outgoing.SoftDelete();
                await _notifier.NotifyUserAsync(userId, "transaction.deleted", new { Id = outgoing.Id }, cancellationToken);
            }

            if (incoming != null)
            {
                var destAccount = await _db.Accounts
                    .SingleOrDefaultAsync(a => a.Id == incoming.AccountId && a.UserId == userId, cancellationToken);
                if (destAccount != null)
                {
                    destAccount.Apply(incoming.Amount.Negate());
                    await _notifier.NotifyUserAsync(
                        userId,
                        "account.balance-changed",
                        new { AccountId = destAccount.Id, Balance = destAccount.Balance.Amount, Currency = destAccount.Currency.Code },
                        cancellationToken);
                }
                incoming.SoftDelete();
                await _notifier.NotifyUserAsync(userId, "transaction.deleted", new { Id = incoming.Id }, cancellationToken);
            }

            await _db.SaveChangesAsync(cancellationToken);
            return;
        }

        var account = await _db.Accounts
            .SingleOrDefaultAsync(a => a.Id == transaction.AccountId && a.UserId == userId, cancellationToken)
            ?? throw new NotFoundException("Счёт", transaction.AccountId);

        // Reverse the original delta:
        //   Income — original was +amount, reverse = -amount
        //   Expense — original was -amount, reverse = +amount
        var reverse = transaction.Type == TransactionType.Income
            ? transaction.Amount.Negate()
            : transaction.Amount;
        account.Apply(reverse);
        transaction.SoftDelete();

        await _db.SaveChangesAsync(cancellationToken);

        await _notifier.NotifyUserAsync(
            userId,
            "transaction.deleted",
            new { transaction.Id },
            cancellationToken);
        await _notifier.NotifyUserAsync(
            userId,
            "account.balance-changed",
            new { AccountId = account.Id, Balance = account.Balance.Amount, Currency = account.Currency.Code },
            cancellationToken);
    }
}
