using FinanceTracker.Application.Dashboard.Models;
using FinanceTracker.Application.Dashboard.Queries.GetCashflow;
using FinanceTracker.Application.Dashboard.Queries.GetDashboardBalance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceTracker.Api.Controllers;

[Authorize]
[Route("api/dashboard")]
public sealed class DashboardController : ApiControllerBase
{
    [HttpGet("balance")]
    [ProducesResponseType(typeof(DashboardBalanceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<DashboardBalanceDto>> Balance(CancellationToken cancellationToken)
        => Ok(await Sender.Send(new GetDashboardBalanceQuery(), cancellationToken));

    [HttpGet("cashflow")]
    [ProducesResponseType(typeof(CashflowDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CashflowDto>> Cashflow(
        [FromQuery] int days = 30,
        [FromQuery] string? currency = null,
        CancellationToken cancellationToken = default)
        => Ok(await Sender.Send(new GetCashflowQuery(days, currency), cancellationToken));
}
