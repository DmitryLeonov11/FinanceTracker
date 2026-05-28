using MediatR;

namespace FinanceTracker.Application.Budgets.Commands.CloseBudget;

public sealed record CloseBudgetCommand(Guid Id) : IRequest;
