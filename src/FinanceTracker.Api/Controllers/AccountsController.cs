using FinanceTracker.Application.Accounts.Commands.ArchiveAccount;
using FinanceTracker.Application.Accounts.Commands.CreateAccount;
using FinanceTracker.Application.Accounts.Commands.RenameAccount;
using FinanceTracker.Application.Accounts.Models;
using FinanceTracker.Application.Accounts.Queries.GetAccountBalanceHistory;
using FinanceTracker.Application.Accounts.Queries.GetAccountById;
using FinanceTracker.Application.Accounts.Queries.GetAccounts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceTracker.Api.Controllers;

[Authorize]
[Route("api/accounts")]
public sealed class AccountsController : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<AccountDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<AccountDto>>> List(
        [FromQuery] bool includeArchived = false,
        CancellationToken cancellationToken = default)
        => Ok(await Sender.Send(new GetAccountsQuery(includeArchived), cancellationToken));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AccountDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AccountDto>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await Sender.Send(new GetAccountByIdQuery(id), cancellationToken));

    [HttpGet("{id:guid}/balance-history")]
    [ProducesResponseType(typeof(AccountBalanceHistoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AccountBalanceHistoryDto>> GetBalanceHistory(
        Guid id,
        [FromQuery] int days = 30,
        CancellationToken cancellationToken = default)
        => Ok(await Sender.Send(new GetAccountBalanceHistoryQuery(id, days), cancellationToken));

    [HttpPost]
    [ProducesResponseType(typeof(AccountDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<AccountDto>> Create([FromBody] CreateAccountCommand command, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);
        return Created(string.Empty, result);
    }

    [HttpPatch("{id:guid}")]
    [ProducesResponseType(typeof(AccountDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AccountDto>> Rename(Guid id, [FromBody] RenameAccountBody body, CancellationToken cancellationToken)
        => Ok(await Sender.Send(new RenameAccountCommand(id, body.Name), cancellationToken));

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Archive(Guid id, CancellationToken cancellationToken)
    {
        await Sender.Send(new ArchiveAccountCommand(id), cancellationToken);
        return NoContent();
    }

    public sealed record RenameAccountBody(string Name);
}
