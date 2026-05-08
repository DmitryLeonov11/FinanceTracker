using FinanceTracker.Domain.Common;
using FinanceTracker.Domain.Exceptions;

namespace FinanceTracker.Domain.Categories;

public sealed class Category : AggregateRoot
{
    public Guid UserId { get; private set; }
    public Guid? ParentId { get; private set; }
    public string Name { get; private set; } = null!;
    public CategoryKind Kind { get; private set; }
    public string? Icon { get; private set; }
    public string? Color { get; private set; }
    public bool IsDeleted { get; private set; }

    private Category() { }

    private Category(Guid id, Guid userId, Guid? parentId, string name, CategoryKind kind, string? icon, string? color)
        : base(id)
    {
        UserId = userId;
        ParentId = parentId;
        Name = name;
        Kind = kind;
        Icon = icon;
        Color = color;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public static Category Create(Guid userId, string name, CategoryKind kind, Guid? parentId = null, string? icon = null, string? color = null)
    {
        Guard.MaxLength(Guard.NotNullOrWhiteSpace(name, nameof(name)), 100, nameof(name));
        return new Category(Guid.NewGuid(), userId, parentId, name, kind, icon, color);
    }

    public void Rename(string name)
    {
        Name = Guard.MaxLength(Guard.NotNullOrWhiteSpace(name, nameof(name)), 100, nameof(name));
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MoveTo(Guid? parentId)
    {
        if (parentId == Id)
            throw new DomainException("Категория не может быть родителем самой себя.");
        ParentId = parentId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
