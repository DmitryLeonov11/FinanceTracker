using FinanceTracker.Application.Fx.Models;
using MediatR;

namespace FinanceTracker.Application.Fx.Queries.GetFxRates;

public sealed record GetFxRatesQuery : IRequest<IReadOnlyCollection<FxRateDto>>;
