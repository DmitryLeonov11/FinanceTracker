using FinanceTracker.Application.Budgets.Models;
using MediatR;

namespace FinanceTracker.Application.Budgets.Commands.UpdateBudget;

public sealed record UpdateBudgetCommand(Guid Id, string? Name, decimal? Limit) : IRequest<BudgetDto>;
