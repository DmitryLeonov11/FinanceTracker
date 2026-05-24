using FinanceTracker.Application.Fx.Models;
using FinanceTracker.Application.Fx.Queries.ConvertMoney;
using FinanceTracker.Application.Fx.Queries.GetFxRates;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceTracker.Api.Controllers;

[Authorize]
[Route("api/fx")]
public sealed class FxController : ApiControllerBase
{
    [HttpGet("rates")]
    [ProducesResponseType(typeof(IReadOnlyCollection<FxRateDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<FxRateDto>>> Rates(CancellationToken cancellationToken)
        => Ok(await Sender.Send(new GetFxRatesQuery(), cancellationToken));

    [HttpGet("convert")]
    [ProducesResponseType(typeof(FxConversionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<FxConversionDto>> Convert(
        [FromQuery] decimal amount,
        [FromQuery] string from,
        [FromQuery] string to,
        CancellationToken cancellationToken)
        => Ok(await Sender.Send(new ConvertMoneyQuery(amount, from, to), cancellationToken));
}
