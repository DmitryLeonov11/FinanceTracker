using FinanceTracker.Application.Accounts.Models;
using MediatR;

namespace FinanceTracker.Application.Accounts.Queries.GetAccountBalanceHistory;

public sealed record GetAccountBalanceHistoryQuery(Guid AccountId, int Days = 30) : IRequest<AccountBalanceHistoryDto>;
