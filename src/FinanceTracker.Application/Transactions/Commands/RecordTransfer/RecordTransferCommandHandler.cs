using FinanceTracker.Application.Common.Exceptions;
using FinanceTracker.Application.Common.Interfaces;
using FinanceTracker.Application.Transactions.Models;
using FinanceTracker.Domain.Exceptions;
using FinanceTracker.Domain.Transactions;
using FinanceTracker.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Application.Transactions.Commands.RecordTransfer;

public sealed class RecordTransferCommandHandler : IRequestHandler<RecordTransferCommand, TransferDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IRealtimeNotifier _notifier;

    public RecordTransferCommandHandler(
        IApplicationDbContext db,
        ICurrentUser currentUser,
        IRealtimeNotifier notifier)
    {
        _db = db;
        _currentUser = currentUser;
        _notifier = notifier;
    }

    public async Task<TransferDto> Handle(RecordTransferCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.RequireUserId();

        var source = await _db.Accounts
            .SingleOrDefaultAsync(a => a.Id == request.SourceAccountId && a.UserId == userId, cancellationToken)
            ?? throw new NotFoundException("Счёт-источник", request.SourceAccountId);

        var destination = await _db.Accounts
            .SingleOrDefaultAsync(a => a.Id == request.DestinationAccountId && a.UserId == userId, cancellationToken)
            ?? throw new NotFoundException("Счёт-получатель", request.DestinationAccountId);

        if (!source.Currency.Equals(destination.Currency))
            throw new DomainException(
                "Перевод между счетами в разных валютах появится вместе с FX-модулем. Сейчас валюты счетов должны совпадать.");

        var amount = Money.Of(request.Amount, source.Currency);

        var (outgoing, incoming) = Transaction.RecordTransfer(
            userId,
            source.Id,
            destination.Id,
            amount,
            amount,
            request.OccurredAt,
            request.Note);

        source.Apply(amount.Negate());
        destination.Apply(amount);

        _db.Transactions.Add(outgoing);
        _db.Transactions.Add(incoming);

        await _db.SaveChangesAsync(cancellationToken);

        await _notifier.NotifyUserAsync(
            userId,
            "transaction.created",
            new
            {
                Transfer = true,
                outgoing.TransferGroupId,
                OutgoingId = outgoing.Id,
                IncomingId = incoming.Id,
                SourceAccountId = source.Id,
                DestinationAccountId = destination.Id,
                Amount = amount.Amount,
                Currency = amount.Currency.Code
            },
            cancellationToken);

        await _notifier.NotifyUserAsync(
            userId,
            "account.balance-changed",
            new { AccountId = source.Id, Balance = source.Balance.Amount, Currency = source.Currency.Code },
            cancellationToken);
        await _notifier.NotifyUserAsync(
            userId,
            "account.balance-changed",
            new { AccountId = destination.Id, Balance = destination.Balance.Amount, Currency = destination.Currency.Code },
            cancellationToken);

        return new TransferDto(
            outgoing.TransferGroupId!.Value,
            outgoing.Id,
            incoming.Id,
            source.Id,
            destination.Id,
            amount.Amount,
            amount.Currency.Code,
            outgoing.OccurredAt,
            outgoing.Note);
    }
}
