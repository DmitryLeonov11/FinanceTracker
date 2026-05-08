using FinanceTracker.Domain.Users;
using FinanceTracker.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceTracker.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id).HasColumnName("id");
        builder.Property(u => u.PasswordHash).HasColumnName("password_hash").IsRequired().HasMaxLength(200);
        builder.Property(u => u.DisplayName).HasColumnName("display_name").IsRequired().HasMaxLength(100);
        builder.Property(u => u.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(u => u.UpdatedAt).HasColumnName("updated_at");
        builder.Property(u => u.Version).HasColumnName("version").IsConcurrencyToken();

        builder.Property(u => u.Email)
            .HasConversion(e => e.Value, v => Email.Of(v))
            .HasColumnName("email")
            .IsRequired()
            .HasMaxLength(254);

        builder.Property(u => u.DisplayCurrency)
            .HasConversion(c => c.Code, v => Currency.Of(v))
            .HasColumnName("display_currency")
            .IsRequired()
            .HasMaxLength(3);

        builder.HasIndex(u => u.Email).IsUnique();

        builder.HasMany(u => u.RefreshTokens)
            .WithOne()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(u => u.DomainEvents);
    }
}
