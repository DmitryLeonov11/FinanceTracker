using FinanceTracker.Domain.Accounts;
using FinanceTracker.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceTracker.Infrastructure.Persistence.Configurations;

public sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("accounts");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id).HasColumnName("id");
        builder.Property(a => a.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(a => a.Name).HasColumnName("name").IsRequired().HasMaxLength(100);
        builder.Property(a => a.Type).HasColumnName("type").HasConversion<int>().IsRequired();
        builder.Property(a => a.IsArchived).HasColumnName("is_archived").IsRequired();
        builder.Property(a => a.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(a => a.UpdatedAt).HasColumnName("updated_at");
        builder.Property(a => a.Version).HasColumnName("version").IsConcurrencyToken();

        builder.Property(a => a.Currency)
            .HasConversion(c => c.Code, v => Currency.Of(v))
            .HasColumnName("currency")
            .IsRequired()
            .HasMaxLength(3);

        builder.OwnsOne(a => a.Balance, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("balance_amount")
                .HasColumnType("numeric(19,4)")
                .IsRequired();

            money.Property(m => m.Currency)
                .HasConversion(c => c.Code, v => Currency.Of(v))
                .HasColumnName("balance_currency")
                .IsRequired()
                .HasMaxLength(3);
        });

        builder.Navigation(a => a.Balance).IsRequired();

        builder.HasIndex(a => new { a.UserId, a.IsArchived });

        builder.Ignore(a => a.DomainEvents);
    }
}
