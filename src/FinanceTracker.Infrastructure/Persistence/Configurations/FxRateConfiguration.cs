using FinanceTracker.Domain.Fx;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceTracker.Infrastructure.Persistence.Configurations;

public sealed class FxRateConfiguration : IEntityTypeConfiguration<FxRate>
{
    public void Configure(EntityTypeBuilder<FxRate> builder)
    {
        builder.ToTable("fx_rates");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id).HasColumnName("id");
        builder.Property(r => r.Date).HasColumnName("date").IsRequired();
        builder.Property(r => r.Currency).HasColumnName("currency").IsRequired().HasMaxLength(3);
        builder.Property(r => r.RateToUsd)
            .HasColumnName("rate_to_usd")
            .HasColumnType("numeric(20,10)")
            .IsRequired();
        builder.Property(r => r.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(r => r.UpdatedAt).HasColumnName("updated_at");
        builder.Property(r => r.Version).HasColumnName("version").IsConcurrencyToken();

        builder.HasIndex(r => new { r.Currency, r.Date }).IsUnique();
        builder.HasIndex(r => r.Date);

        builder.Ignore(r => r.DomainEvents);
    }
}
