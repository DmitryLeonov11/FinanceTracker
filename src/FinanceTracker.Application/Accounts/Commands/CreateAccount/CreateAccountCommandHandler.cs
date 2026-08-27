using FinanceTracker.Application.Accounts.Models;
using FinanceTracker.Application.Common.Interfaces;
using FinanceTracker.Domain.Accounts;
using FinanceTracker.Domain.ValueObjects;
using MediatR;

namespace FinanceTracker.Application.Accounts.Commands.CreateAccount;

public sealed class CreateAccountCommandHandler : IRequestHandler<CreateAccountCommand, AccountDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IRealtimeNotifier _notifier;

    public CreateAccountCommandHandler(
        IApplicationDbContext db,
        ICurrentUser currentUser,
        IRealtimeNotifier notifier)
    {
        _db = db;
        _currentUser = currentUser;
        _notifier = notifier;
    }

    public async Task<AccountDto> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.RequireUserId();

        var initial = Money.Of(request.InitialBalance, Currency.Of(request.Currency));
        var account = Account.Open(userId, request.Name, request.Type, initial);

        _db.Accounts.Add(account);
        await _db.SaveChangesAsync(cancellationToken);

        try
        {
            await _notifier.NotifyUserAsync(
                userId,
                "account.created",
                new { account.Id, account.Name, Currency = account.Currency.Code, Balance = account.Balance.Amount },
                cancellationToken);
        }
        catch (Exception)
        {
            // realtime delivery is best-effort; the command itself has succeeded
        }

        return new AccountDto(
            account.Id,
            account.Name,
            account.Type,
            account.Currency.Code,
            account.Balance.Amount,
            account.IsArchived,
            account.CreatedAt);
    }
}
