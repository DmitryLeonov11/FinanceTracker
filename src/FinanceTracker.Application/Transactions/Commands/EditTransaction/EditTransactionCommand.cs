using FinanceTracker.Application.Transactions.Models;
using MediatR;

namespace FinanceTracker.Application.Transactions.Commands.EditTransaction;

public sealed record EditTransactionCommand(
    Guid TransactionId,
    decimal Amount,
    DateTimeOffset OccurredAt,
    Guid? CategoryId,
    string? Note) : IRequest<TransactionDto>;
