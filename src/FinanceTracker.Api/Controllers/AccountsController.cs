using FinanceTracker.Application.Accounts.Commands.CreateAccount;
using FinanceTracker.Application.Accounts.Models;
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

    [HttpPost]
    [ProducesResponseType(typeof(AccountDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<AccountDto>> Create([FromBody] CreateAccountCommand command, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);
        return Created(string.Empty, result);
    }
}
