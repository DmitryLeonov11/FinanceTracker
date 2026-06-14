using FinanceTracker.Application.Categories.Models;
using FinanceTracker.Application.Common.Exceptions;
using FinanceTracker.Application.Common.Interfaces;
using FinanceTracker.Domain.Categories;
using FinanceTracker.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Application.Categories.Commands.CreateCategory;

public sealed class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, CategoryDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IRealtimeNotifier _notifier;

    public CreateCategoryCommandHandler(IApplicationDbContext db, ICurrentUser currentUser, IRealtimeNotifier notifier)
    {
        _db = db;
        _currentUser = currentUser;
        _notifier = notifier;
    }

    public async Task<CategoryDto> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.RequireUserId();

        if (request.ParentId is { } parentId)
        {
            var parent = await _db.Categories
                .AsNoTracking()
                .SingleOrDefaultAsync(c => c.Id == parentId && c.UserId == userId, cancellationToken)
                ?? throw new NotFoundException("Категория", parentId);

            if (parent.Kind != request.Kind)
                throw new DomainException("Родительская категория должна иметь тот же тип (Income/Expense).");
        }

        var category = Category.Create(userId, request.Name, request.Kind, request.ParentId, request.Icon, request.Color);

        _db.Categories.Add(category);
        await _db.SaveChangesAsync(cancellationToken);

        try
        {
            await _notifier.NotifyUserAsync(
                userId,
                "category.created",
                new { category.Id, category.Name, category.Kind, category.ParentId },
                cancellationToken);
        }
        catch (Exception)
        {
            // realtime delivery is best-effort
        }

        return new CategoryDto(
            category.Id,
            category.ParentId,
            category.Name,
            category.Kind,
            category.Icon,
            category.Color,
            category.CreatedAt);
    }
}
