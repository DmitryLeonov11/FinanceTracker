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
            // v1 limitation: удаление переводов требует явного флага направления (IsOutgoing) на ноге,
            // которого пока в схеме нет. Чтобы откатить баланс корректно для обеих сторон, нужно знать
            // какая нога была debit, какая credit. Введение колонки откладываем до отдельной миграции.
            throw new DomainException(
                "Удаление переводов появится позже. Сейчас можно создать обратный перевод вручную.");
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
