using FinanceTracker.Domain.Budgets;
using FinanceTracker.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceTracker.Infrastructure.Persistence.Configurations;

public sealed class BudgetConfiguration : IEntityTypeConfiguration<Budget>
{
    public void Configure(EntityTypeBuilder<Budget> builder)
    {
        builder.ToTable("budgets");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Id).HasColumnName("id");
        builder.Property(b => b.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(b => b.Name).HasColumnName("name").IsRequired().HasMaxLength(100);
        builder.Property(b => b.CategoryId).HasColumnName("category_id");
        builder.Property(b => b.Period).HasColumnName("period").HasConversion<int>().IsRequired();
        builder.Property(b => b.StartDate).HasColumnName("start_date").IsRequired();
        builder.Property(b => b.EndDate).HasColumnName("end_date");
        builder.Property(b => b.Rollover).HasColumnName("rollover").IsRequired();
        builder.Property(b => b.IsClosed).HasColumnName("is_closed").IsRequired();
        builder.Property(b => b.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(b => b.UpdatedAt).HasColumnName("updated_at");
        builder.Property(b => b.Version).HasColumnName("version").IsConcurrencyToken();

        builder.OwnsOne(b => b.Limit, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("limit_amount")
                .HasColumnType("numeric(19,4)")
                .IsRequired();

            money.Property(m => m.Currency)
                .HasConversion(c => c.Code, v => Currency.Of(v))
                .HasColumnName("limit_currency")
                .IsRequired()
                .HasMaxLength(3);
        });

        builder.Navigation(b => b.Limit).IsRequired();

        builder.HasIndex(b => new { b.UserId, b.IsClosed });
        builder.HasIndex(b => new { b.UserId, b.CategoryId });

        builder.Ignore(b => b.DomainEvents);
    }
}
