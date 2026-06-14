using FinanceTracker.Domain.Categories;

namespace FinanceTracker.Application.Categories.Models;

public sealed record CategoryDto(
    Guid Id,
    Guid? ParentId,
    string Name,
    CategoryKind Kind,
    string? Icon,
    string? Color,
    DateTimeOffset CreatedAt);
