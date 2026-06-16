using FinanceTracker.Application.Accounts.Helpers;
using FinanceTracker.Application.Accounts.Models;
using FinanceTracker.Application.Common.Exceptions;
using FinanceTracker.Application.Common.Interfaces;
using FinanceTracker.Domain.Transactions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Application.Accounts.Queries.GetAccountBalanceHistory;

public sealed class GetAccountBalanceHistoryQueryHandler
    : IRequestHandler<GetAccountBalanceHistoryQuery, AccountBalanceHistoryDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IDateTime _clock;

    public GetAccountBalanceHistoryQueryHandler(IApplicationDbContext db, ICurrentUser currentUser, IDateTime clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<AccountBalanceHistoryDto> Handle(GetAccountBalanceHistoryQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.RequireUserId();

        var account = await _db.Accounts
            .AsNoTracking()
            .Where(a => a.Id == request.AccountId && a.UserId == userId)
            .Select(a => new { a.Id, Currency = a.Currency.Code, Balance = a.Balance.Amount })
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("Счёт", request.AccountId);

        var today = DateOnly.FromDateTime(_clock.UtcNow.UtcDateTime);
        var from = today.AddDays(-(request.Days - 1));
        var fromUtc = new DateTimeOffset(from.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        var toUtc = new DateTimeOffset(today.ToDateTime(TimeOnly.MaxValue), TimeSpan.Zero);

        var deltas = await _db.Transactions
            .AsNoTracking()
            .Where(t => t.AccountId == account.Id
                     && !t.IsDeleted
                     && t.OccurredAt >= fromUtc
                     && t.OccurredAt <= toUtc)
            .GroupBy(t => t.OccurredAt.UtcDateTime.Date)
            .Select(g => new
            {
                Date = g.Key,
                Delta = g.Sum(t =>
                    t.Type == TransactionType.Income ? t.Amount.Amount :
                    t.Type == TransactionType.Expense ? -t.Amount.Amount :
                    t.IsOutgoing ? -t.Amount.Amount : t.Amount.Amount)
            })
            .ToListAsync(cancellationToken);

        var deltasByDay = deltas.ToDictionary(x => DateOnly.FromDateTime(x.Date), x => x.Delta);

        var points = BalanceHistoryCalculator.Compute(account.Balance, today, request.Days, deltasByDay);

        return new AccountBalanceHistoryDto(account.Id, account.Currency, points);
    }
}
