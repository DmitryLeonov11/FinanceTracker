using FinanceTracker.Application.Common.Exceptions;
using FinanceTracker.Application.Common.Interfaces;
using FinanceTracker.Application.Transactions.Models;
using FinanceTracker.Domain.Transactions;
using FinanceTracker.Domain.ValueObjects;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Application.Transactions.Commands.RecordTransfer;

public sealed class RecordTransferCommandHandler : IRequestHandler<RecordTransferCommand, TransferDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IRealtimeNotifier _notifier;
    private readonly IMoneyConverter _converter;

    public RecordTransferCommandHandler(
        IApplicationDbContext db,
        ICurrentUser currentUser,
        IRealtimeNotifier notifier,
        IMoneyConverter converter)
    {
        _db = db;
        _currentUser = currentUser;
        _notifier = notifier;
        _converter = converter;
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

        var sameCurrency = source.Currency.Equals(destination.Currency);

        Money sourceAmount;
        Money destinationAmount;
        decimal? appliedRate = null;

        if (sameCurrency)
        {
            sourceAmount = Money.Of(request.Amount, source.Currency);
            destinationAmount = sourceAmount;
        }
        else
        {
            if (!request.DestinationAmount.HasValue || request.DestinationAmount.Value <= 0)
            {
                throw new ValidationException(new[]
                {
                    new ValidationFailure(
                        nameof(RecordTransferCommand.DestinationAmount),
                        "Для перевода между разными валютами укажите сумму зачисления.")
                });
            }

            sourceAmount = Money.Of(request.Amount, source.Currency);
            destinationAmount = Money.Of(request.DestinationAmount.Value, destination.Currency);

            // Зафиксируем курс на момент перевода для audit-следа.
            var conversion = await _converter.ConvertAsync(
                Money.Of(1m, source.Currency),
                destination.Currency,
                asOf: null,
                cancellationToken);
            appliedRate = conversion.RateApplied;
        }

        var (outgoing, incoming) = Transaction.RecordTransfer(
            userId,
            source.Id,
            destination.Id,
            sourceAmount,
            destinationAmount,
            request.OccurredAt,
            request.Note);

        source.Apply(sourceAmount.Negate());
        destination.Apply(destinationAmount);

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
                Amount = sourceAmount.Amount,
                Currency = sourceAmount.Currency.Code,
                DestinationAmount = destinationAmount.Amount,
                DestinationCurrency = destinationAmount.Currency.Code
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
            sourceAmount.Amount,
            sourceAmount.Currency.Code,
            destinationAmount.Amount,
            destinationAmount.Currency.Code,
            appliedRate,
            outgoing.OccurredAt,
            outgoing.Note);
    }
}
