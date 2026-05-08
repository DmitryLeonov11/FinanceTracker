using FinanceTracker.Application.Common.Exceptions;
using FinanceTracker.Application.Common.Interfaces;
using FinanceTracker.Application.Dashboard.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Application.Dashboard.Queries.GetDashboardBalance;

public sealed class GetDashboardBalanceQueryHandler : IRequestHandler<GetDashboardBalanceQuery, DashboardBalanceDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetDashboardBalanceQueryHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<DashboardBalanceDto> Handle(GetDashboardBalanceQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.RequireUserId();

        var user = await _db.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new NotFoundException("Пользователь", userId);

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

        return new DashboardBalanceDto(user.DisplayCurrency.Code, grouped, accounts);
    }
}
