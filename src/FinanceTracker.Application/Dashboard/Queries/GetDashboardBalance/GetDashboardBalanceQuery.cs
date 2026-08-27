using FinanceTracker.Application.Dashboard.Models;
using MediatR;

namespace FinanceTracker.Application.Dashboard.Queries.GetDashboardBalance;

public sealed record GetDashboardBalanceQuery : IRequest<DashboardBalanceDto>;
