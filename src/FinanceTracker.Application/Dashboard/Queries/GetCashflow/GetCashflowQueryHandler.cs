using FinanceTracker.Application.Common.Exceptions;
using FinanceTracker.Application.Common.Interfaces;
using FinanceTracker.Application.Dashboard.Models;
using FinanceTracker.Domain.Transactions;
using FinanceTracker.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Application.Dashboard.Queries.GetCashflow;

public sealed class GetCashflowQueryHandler : IRequestHandler<GetCashflowQuery, CashflowDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IDateTime _clock;

    public GetCashflowQueryHandler(IApplicationDbContext db, ICurrentUser currentUser, IDateTime clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<CashflowDto> Handle(GetCashflowQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.RequireUserId();

        var currency = request.Currency;
        if (string.IsNullOrWhiteSpace(currency))
        {
            currency = await _db.Users
                .AsNoTracking()
                .Where(u => u.Id == userId)
                .Select(u => u.DisplayCurrency.Code)
                .SingleOrDefaultAsync(cancellationToken)
                ?? throw new NotFoundException("Пользователь", userId);
        }

        var now = _clock.UtcNow;
        var to = new DateTimeOffset(now.Date.AddDays(1), TimeSpan.Zero).AddTicks(-1);
        var from = new DateTimeOffset(now.Date.AddDays(-(request.Days - 1)), TimeSpan.Zero);

        var grouped = await _db.Transactions
            .AsNoTracking()
            .Where(t => t.UserId == userId)
            .Where(t => t.Type == TransactionType.Income || t.Type == TransactionType.Expense)
            .Where(t => t.Amount.Currency == Currency.Of(currency))
            .Where(t => t.OccurredAt >= from && t.OccurredAt <= to)
            .GroupBy(t => new { Date = t.OccurredAt.UtcDateTime.Date, t.Type })
            .Select(g => new
            {
                g.Key.Date,
                g.Key.Type,
                Amount = g.Sum(t => t.Amount.Amount)
            })
            .ToListAsync(cancellationToken);

        var byDate = new Dictionary<DateOnly, (decimal income, decimal expense)>();
        foreach (var row in grouped)
        {
            var key = DateOnly.FromDateTime(row.Date);
            var (inc, exp) = byDate.GetValueOrDefault(key);
            if (row.Type == TransactionType.Income) inc += row.Amount;
            else exp += row.Amount;
            byDate[key] = (inc, exp);
        }

        var points = new List<CashflowPoint>(request.Days);
        decimal totalIncome = 0, totalExpense = 0;
        var startDate = DateOnly.FromDateTime(from.UtcDateTime);
        for (var i = 0; i < request.Days; i++)
        {
            var date = startDate.AddDays(i);
            var (income, expense) = byDate.GetValueOrDefault(date);
            points.Add(new CashflowPoint(date, income, expense));
            totalIncome += income;
            totalExpense += expense;
        }

        return new CashflowDto(
            currency!,
            from,
            to,
            totalIncome,
            totalExpense,
            totalIncome - totalExpense,
            points);
    }
}
