using FinanceTracker.Application.Accounts.Models;
using FinanceTracker.Application.Common.Exceptions;
using FinanceTracker.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Application.Accounts.Queries.GetAccountById;

public sealed class GetAccountByIdQueryHandler : IRequestHandler<GetAccountByIdQuery, AccountDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetAccountByIdQueryHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<AccountDto> Handle(GetAccountByIdQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.RequireUserId();

        var dto = await _db.Accounts
            .AsNoTracking()
            .Where(a => a.Id == request.Id && a.UserId == userId)
            .Select(a => new AccountDto(
                a.Id,
                a.Name,
                a.Type,
                a.Currency.Code,
                a.Balance.Amount,
                a.IsArchived,
                a.CreatedAt))
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("Счёт", request.Id);

        return dto;
    }
}
