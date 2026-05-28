using FinanceTracker.Application.Budgets.Models;
using FinanceTracker.Domain.Budgets;
using MediatR;

namespace FinanceTracker.Application.Budgets.Commands.CreateBudget;

public sealed record CreateBudgetCommand(
    string Name,
    BudgetPeriod Period,
    string Currency,
    decimal Limit,
    DateOnly StartDate,
    Guid? CategoryId = null,
    bool Rollover = false) : IRequest<BudgetDto>;
