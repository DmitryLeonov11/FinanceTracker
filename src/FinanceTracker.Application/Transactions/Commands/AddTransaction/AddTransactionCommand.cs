using FinanceTracker.Application.Transactions.Models;
using FinanceTracker.Domain.Transactions;
using MediatR;

namespace FinanceTracker.Application.Transactions.Commands.AddTransaction;

public sealed record AddTransactionCommand(
    Guid AccountId,
    Guid? CategoryId,
    TransactionType Type,
    decimal Amount,
    DateTimeOffset OccurredAt,
    string? Note) : IRequest<TransactionDto>;
