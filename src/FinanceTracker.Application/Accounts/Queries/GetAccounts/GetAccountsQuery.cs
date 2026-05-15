using FinanceTracker.Application.Accounts.Models;
using MediatR;

namespace FinanceTracker.Application.Accounts.Queries.GetAccounts;

public sealed record GetAccountsQuery(bool IncludeArchived = false) : IRequest<IReadOnlyCollection<AccountDto>>;
