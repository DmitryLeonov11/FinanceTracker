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

        var rows = await _db.Transactions
            .AsNoTracking()
            .Where(t => t.AccountId == account.Id
                     && !t.IsDeleted
                     && t.OccurredAt >= fromUtc
                     && t.OccurredAt <= toUtc)
            .Select(t => new { t.OccurredAt, t.Type, t.IsOutgoing, Amount = t.Amount.Amount })
            .ToListAsync(cancellationToken);

        var deltasByDay = rows
            .GroupBy(t => t.OccurredAt.UtcDateTime.Date)
            .ToDictionary(
                g => DateOnly.FromDateTime(g.Key),
                g => g.Sum(t =>
                    t.Type == TransactionType.Income ? t.Amount :
                    t.Type == TransactionType.Expense ? -t.Amount :
                    t.IsOutgoing ? -t.Amount : t.Amount));

        var points = BalanceHistoryCalculator.Compute(account.Balance, today, request.Days, deltasByDay);

        return new AccountBalanceHistoryDto(account.Id, account.Currency, points);
    }
}
