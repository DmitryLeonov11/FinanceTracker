using FinanceTracker.Application.Accounts.Models;
using FinanceTracker.Domain.Accounts;
using MediatR;

namespace FinanceTracker.Application.Accounts.Commands.CreateAccount;

public sealed record CreateAccountCommand(
    string Name,
    AccountType Type,
    string Currency,
    decimal InitialBalance) : IRequest<AccountDto>;
