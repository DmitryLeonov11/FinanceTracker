using FinanceTracker.Application.Accounts.Models;
using MediatR;

namespace FinanceTracker.Application.Accounts.Queries.GetAccountById;

public sealed record GetAccountByIdQuery(Guid Id) : IRequest<AccountDto>;
