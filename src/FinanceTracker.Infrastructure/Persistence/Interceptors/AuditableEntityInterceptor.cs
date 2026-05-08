using FinanceTracker.Application.Common.Interfaces;
using FinanceTracker.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace FinanceTracker.Infrastructure.Persistence.Interceptors;

public sealed class AuditableEntityInterceptor : SaveChangesInterceptor
{
    private readonly IDateTime _clock;

    public AuditableEntityInterceptor(IDateTime clock) => _clock = clock;

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateTimestamps(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        UpdateTimestamps(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void UpdateTimestamps(DbContext? context)
    {
        if (context is null) return;

        foreach (EntityEntry<Entity> entry in context.ChangeTracker.Entries<Entity>())
        {
            if (entry.State == EntityState.Added && entry.Entity.CreatedAt == default)
                entry.Property(nameof(Entity.CreatedAt)).CurrentValue = _clock.UtcNow;

            if (entry.State == EntityState.Modified)
                entry.Property(nameof(Entity.UpdatedAt)).CurrentValue = _clock.UtcNow;
        }
    }
}
