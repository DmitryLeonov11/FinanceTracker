using FinanceTracker.Application.Budgets.Models;
using MediatR;

namespace FinanceTracker.Application.Budgets.Queries.GetBudgets;

public sealed record GetBudgetsQuery(bool IncludeClosed = false) : IRequest<IReadOnlyCollection<BudgetWithProgressDto>>;
