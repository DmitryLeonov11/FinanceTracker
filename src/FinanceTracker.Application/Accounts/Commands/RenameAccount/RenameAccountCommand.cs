using FinanceTracker.Application.Accounts.Models;
using MediatR;

namespace FinanceTracker.Application.Accounts.Commands.RenameAccount;

public sealed record RenameAccountCommand(Guid Id, string Name) : IRequest<AccountDto>;
