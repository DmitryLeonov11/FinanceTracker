using FinanceTracker.Application.Common.Interfaces;
using FinanceTracker.Application.Fx.Models;
using FinanceTracker.Domain.ValueObjects;
using MediatR;

namespace FinanceTracker.Application.Fx.Queries.ConvertMoney;

public sealed class ConvertMoneyQueryHandler : IRequestHandler<ConvertMoneyQuery, FxConversionDto>
{
    private readonly IMoneyConverter _converter;

    public ConvertMoneyQueryHandler(IMoneyConverter converter) => _converter = converter;

    public async Task<FxConversionDto> Handle(ConvertMoneyQuery request, CancellationToken cancellationToken)
    {
        var from = Currency.Of(request.From);
        var to = Currency.Of(request.To);
        var source = Money.Of(request.Amount, from);

        var conversion = await _converter.ConvertAsync(source, to, asOf: null, cancellationToken);

        return new FxConversionDto(
            request.Amount,
            from.Code,
            conversion.Result.Amount,
            to.Code,
            conversion.RateApplied,
            conversion.RateDate);
    }
}
