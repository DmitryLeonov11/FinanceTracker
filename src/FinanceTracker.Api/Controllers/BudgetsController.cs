using FinanceTracker.Application.Budgets.Commands.CloseBudget;
using FinanceTracker.Application.Budgets.Commands.CreateBudget;
using FinanceTracker.Application.Budgets.Commands.UpdateBudget;
using FinanceTracker.Application.Budgets.Models;
using FinanceTracker.Application.Budgets.Queries.GetBudgets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceTracker.Api.Controllers;

[Authorize]
[Route("api/budgets")]
public sealed class BudgetsController : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<BudgetWithProgressDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<BudgetWithProgressDto>>> List(
        [FromQuery] bool includeClosed = false,
        CancellationToken cancellationToken = default)
        => Ok(await Sender.Send(new GetBudgetsQuery(includeClosed), cancellationToken));

    [HttpPost]
    [ProducesResponseType(typeof(BudgetDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BudgetDto>> Create([FromBody] CreateBudgetCommand command, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);
        return Created(string.Empty, result);
    }

    [HttpPatch("{id:guid}")]
    [ProducesResponseType(typeof(BudgetDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BudgetDto>> Update(Guid id, [FromBody] UpdateBudgetBody body, CancellationToken cancellationToken)
        => Ok(await Sender.Send(new UpdateBudgetCommand(id, body.Name, body.Limit), cancellationToken));

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Close(Guid id, CancellationToken cancellationToken)
    {
        await Sender.Send(new CloseBudgetCommand(id), cancellationToken);
        return NoContent();
    }

    public sealed record UpdateBudgetBody(string? Name, decimal? Limit);
}
