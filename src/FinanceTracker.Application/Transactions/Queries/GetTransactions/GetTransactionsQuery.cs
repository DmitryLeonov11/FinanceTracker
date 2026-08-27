using FinanceTracker.Application.Common.Models;
using FinanceTracker.Application.Transactions.Models;
using FinanceTracker.Domain.Transactions;
using MediatR;

namespace FinanceTracker.Application.Transactions.Queries.GetTransactions;

public sealed record GetTransactionsQuery(
    DateTimeOffset? From = null,
    DateTimeOffset? To = null,
    IReadOnlyCollection<Guid>? AccountIds = null,
    IReadOnlyCollection<Guid>? CategoryIds = null,
    IReadOnlyCollection<TransactionType>? Types = null,
    string? Search = null,
    int Page = 1,
    int PageSize = 50) : IRequest<PagedResult<TransactionDto>>;
