using FinanceTracker.Application.Categories.Models;
using FinanceTracker.Domain.Categories;
using MediatR;

namespace FinanceTracker.Application.Categories.Commands.CreateCategory;

public sealed record CreateCategoryCommand(
    string Name,
    CategoryKind Kind,
    Guid? ParentId = null,
    string? Icon = null,
    string? Color = null) : IRequest<CategoryDto>;
