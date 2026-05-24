using FinanceTracker.Application.Transactions.Models;
using MediatR;

namespace FinanceTracker.Application.Transactions.Commands.RecordTransfer;

public sealed record RecordTransferCommand(
    Guid SourceAccountId,
    Guid DestinationAccountId,
    decimal Amount,
    decimal? DestinationAmount,
    DateTimeOffset OccurredAt,
    string? Note) : IRequest<TransferDto>;
