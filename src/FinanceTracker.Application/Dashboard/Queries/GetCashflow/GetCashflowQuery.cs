using FinanceTracker.Application.Dashboard.Models;
using MediatR;

namespace FinanceTracker.Application.Dashboard.Queries.GetCashflow;

public sealed record GetCashflowQuery(int Days = 30, string? Currency = null) : IRequest<CashflowDto>;
