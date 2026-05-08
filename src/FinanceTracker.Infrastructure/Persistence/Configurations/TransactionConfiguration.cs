using FinanceTracker.Domain.Transactions;
using FinanceTracker.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceTracker.Infrastructure.Persistence.Configurations;

public sealed class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("transactions");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id).HasColumnName("id");
        builder.Property(t => t.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(t => t.AccountId).HasColumnName("account_id").IsRequired();
        builder.Property(t => t.CounterAccountId).HasColumnName("counter_account_id");
        builder.Property(t => t.CategoryId).HasColumnName("category_id");
        builder.Property(t => t.Type).HasColumnName("type").HasConversion<int>().IsRequired();
        builder.Property(t => t.OccurredAt).HasColumnName("occurred_at").IsRequired();
        builder.Property(t => t.Note).HasColumnName("note").HasMaxLength(500);
        builder.Property(t => t.TransferGroupId).HasColumnName("transfer_group_id");
        builder.Property(t => t.IsDeleted).HasColumnName("is_deleted").IsRequired();
        builder.Property(t => t.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(t => t.UpdatedAt).HasColumnName("updated_at");
        builder.Property(t => t.Version).HasColumnName("version").IsConcurrencyToken();

        builder.OwnsOne(t => t.Amount, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("amount")
                .HasColumnType("numeric(19,4)")
                .IsRequired();

            money.Property(m => m.Currency)
                .HasConversion(c => c.Code, v => Currency.Of(v))
                .HasColumnName("currency")
                .IsRequired()
                .HasMaxLength(3);
        });

        builder.Navigation(t => t.Amount).IsRequired();

        builder.HasIndex(t => new { t.UserId, t.OccurredAt });
        builder.HasIndex(t => new { t.UserId, t.AccountId, t.OccurredAt });
        builder.HasIndex(t => t.TransferGroupId);

        builder.HasQueryFilter(t => !t.IsDeleted);

        builder.Ignore(t => t.DomainEvents);
    }
}
