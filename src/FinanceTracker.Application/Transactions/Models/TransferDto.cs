namespace FinanceTracker.Application.Transactions.Models;

public sealed record TransferDto(
    Guid TransferGroupId,
    Guid OutgoingId,
    Guid IncomingId,
    Guid SourceAccountId,
    Guid DestinationAccountId,
    decimal Amount,
    string Currency,
    DateTimeOffset OccurredAt,
    string? Note);
