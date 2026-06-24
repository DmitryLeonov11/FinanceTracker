using FinanceTracker.Application.Common.Exceptions;
using FinanceTracker.Application.Common.Interfaces;
using FinanceTracker.Application.Dashboard.Models;
using FinanceTracker.Domain.Exceptions;
using FinanceTracker.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Application.Dashboard.Queries.GetDashboardBalance;

public sealed class GetDashboardBalanceQueryHandler : IRequestHandler<GetDashboardBalanceQuery, DashboardBalanceDto>
{
    private const int StaleRateDays = 7;

    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IMoneyConverter _converter;
    private readonly IDateTime _clock;

    public GetDashboardBalanceQueryHandler(
        IApplicationDbContext db,
        ICurrentUser currentUser,
        IMoneyConverter converter,
        IDateTime clock)
    {
        _db = db;
        _currentUser = currentUser;
        _converter = converter;
        _clock = clock;
    }

    public async Task<DashboardBalanceDto> Handle(GetDashboardBalanceQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.RequireUserId();

        var user = await _db.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new NotFoundException("Пользователь", userId);

        var displayCurrency = user.DisplayCurrency;

        var accounts = await _db.Accounts
            .AsNoTracking()
            .Where(a => a.UserId == userId && !a.IsArchived)
            .OrderBy(a => a.Name)
            .Select(a => new AccountBalance(a.Id, a.Name, a.Currency.Code, a.Balance.Amount))
            .ToListAsync(cancellationToken);

        var grouped = accounts
            .GroupBy(a => a.Currency)
            .Select(g => new CurrencyBalance(g.Key, g.Sum(x => x.Balance), g.Count()))
            .OrderBy(c => c.Currency)
            .ToList();

        decimal grandTotal = 0m;
        bool isApproximate = false;
        var today = DateOnly.FromDateTime(_clock.UtcNow.UtcDateTime);
        var staleCutoff = today.AddDays(-StaleRateDays);

        foreach (var currencyBalance in grouped)
        {
            if (currencyBalance.Currency == displayCurrency.Code)
            {
                grandTotal += currencyBalance.Total;
                continue;
            }

            try
            {
                var source = Money.Of(currencyBalance.Total, Currency.Of(currencyBalance.Currency));
                var conversion = await _converter.ConvertAsync(source, displayCurrency, asOf: null, cancellationToken);
                grandTotal += conversion.Result.Amount;
                if (conversion.RateDate < staleCutoff) isApproximate = true;
            }
            catch (DomainException)
            {
                // курса для валюты нет, поэтому пропускаем её и помечаем баланс приблизительным
                isApproximate = true;
            }
        }

        grandTotal = decimal.Round(grandTotal, 2, MidpointRounding.AwayFromZero);

        return new DashboardBalanceDto(displayCurrency.Code, grandTotal, isApproximate, grouped, accounts);
    }
}
