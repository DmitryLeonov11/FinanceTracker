using FinanceTracker.Application.Categories.Commands.CreateCategory;
using FinanceTracker.Application.Categories.Models;
using FinanceTracker.Application.Categories.Queries.GetCategories;
using FinanceTracker.Domain.Categories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceTracker.Api.Controllers;

[Authorize]
[Route("api/categories")]
public sealed class CategoriesController : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<CategoryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<CategoryDto>>> List(
        [FromQuery] CategoryKind? kind = null,
        CancellationToken cancellationToken = default)
        => Ok(await Sender.Send(new GetCategoriesQuery(kind), cancellationToken));

    [HttpPost]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CategoryDto>> Create(
        [FromBody] CreateCategoryCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);
        return Created(string.Empty, result);
    }
}
