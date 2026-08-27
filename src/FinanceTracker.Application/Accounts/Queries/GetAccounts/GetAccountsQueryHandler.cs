using FinanceTracker.Application.Accounts.Models;
using FinanceTracker.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Application.Accounts.Queries.GetAccounts;

public sealed class GetAccountsQueryHandler
    : IRequestHandler<GetAccountsQuery, IReadOnlyCollection<AccountDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetAccountsQueryHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyCollection<AccountDto>> Handle(GetAccountsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.RequireUserId();

        var query = _db.Accounts
            .AsNoTracking()
            .Where(a => a.UserId == userId);

        if (!request.IncludeArchived)
            query = query.Where(a => !a.IsArchived);

        return await query
            .OrderBy(a => a.IsArchived)
            .ThenBy(a => a.Name)
            .Select(a => new AccountDto(
                a.Id,
                a.Name,
                a.Type,
                a.Currency.Code,
                a.Balance.Amount,
                a.IsArchived,
                a.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}
