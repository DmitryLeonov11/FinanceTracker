using FinanceTracker.Application.Categories.Models;
using FinanceTracker.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Application.Categories.Queries.GetCategories;

public sealed class GetCategoriesQueryHandler
    : IRequestHandler<GetCategoriesQuery, IReadOnlyCollection<CategoryDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetCategoriesQueryHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyCollection<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.RequireUserId();

        var query = _db.Categories
            .AsNoTracking()
            .Where(c => c.UserId == userId);

        if (request.Kind is { } kind)
            query = query.Where(c => c.Kind == kind);

        return await query
            .OrderBy(c => c.ParentId == null ? 0 : 1)
            .ThenBy(c => c.Name)
            .Select(c => new CategoryDto(
                c.Id,
                c.ParentId,
                c.Name,
                c.Kind,
                c.Icon,
                c.Color,
                c.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}
