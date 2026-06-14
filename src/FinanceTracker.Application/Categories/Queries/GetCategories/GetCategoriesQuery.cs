using FinanceTracker.Application.Categories.Models;
using FinanceTracker.Domain.Categories;
using MediatR;

namespace FinanceTracker.Application.Categories.Queries.GetCategories;

public sealed record GetCategoriesQuery(CategoryKind? Kind = null) : IRequest<IReadOnlyCollection<CategoryDto>>;
