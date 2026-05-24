using FinanceTracker.Application.Fx.Models;
using MediatR;

namespace FinanceTracker.Application.Fx.Queries.ConvertMoney;

public sealed record ConvertMoneyQuery(decimal Amount, string From, string To) : IRequest<FxConversionDto>;
