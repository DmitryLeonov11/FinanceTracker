using FinanceTracker.Application.Common.Interfaces;
using FinanceTracker.Application.Common.Models;
using FinanceTracker.Application.Transactions.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Application.Transactions.Queries.GetTransactions;

public sealed class GetTransactionsQueryHandler
    : IRequestHandler<GetTransactionsQuery, PagedResult<TransactionDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetTransactionsQueryHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<TransactionDto>> Handle(GetTransactionsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.RequireUserId();

        var query = _db.Transactions
            .AsNoTracking()
            .Where(t => t.UserId == userId);

        if (request.From is { } from)
            query = query.Where(t => t.OccurredAt >= from);

        if (request.To is { } to)
            query = query.Where(t => t.OccurredAt <= to);

        if (request.AccountIds is { Count: > 0 } accountIds)
            query = query.Where(t => accountIds.Contains(t.AccountId));

        if (request.CategoryIds is { Count: > 0 } categoryIds)
            query = query.Where(t => t.CategoryId != null && categoryIds.Contains(t.CategoryId.Value));

        if (request.Types is { Count: > 0 } types)
            query = query.Where(t => types.Contains(t.Type));

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var s = request.Search.Trim().ToLowerInvariant();
            query = query.Where(t => t.Note != null && t.Note.ToLower().Contains(s));
        }

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(t => t.OccurredAt)
            .ThenByDescending(t => t.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(t => new TransactionDto(
                t.Id,
                t.AccountId,
                t.CounterAccountId,
                t.CategoryId,
                t.Type,
                t.Amount.Amount,
                t.Amount.Currency.Code,
                t.OccurredAt,
                t.Note))
            .ToListAsync(cancellationToken);

        return new PagedResult<TransactionDto>(items, total, request.Page, request.PageSize);
    }
}
