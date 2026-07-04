using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceTracker.Infrastructure.Persistence.Configurations;

public sealed class IdempotencyRecordConfiguration : IEntityTypeConfiguration<IdempotencyRecord>
{
    public void Configure(EntityTypeBuilder<IdempotencyRecord> builder)
    {
        builder.ToTable("idempotency_keys");

        builder.HasKey(r => new { r.UserId, r.Key });

        builder.Property(r => r.UserId).HasColumnName("user_id");
        builder.Property(r => r.Key).HasColumnName("key").HasMaxLength(128);
        builder.Property(r => r.Method).HasColumnName("method").IsRequired().HasMaxLength(8);
        builder.Property(r => r.Path).HasColumnName("path").IsRequired().HasMaxLength(512);
        builder.Property(r => r.StatusCode).HasColumnName("status_code").IsRequired();
        builder.Property(r => r.ContentType).HasColumnName("content_type").HasMaxLength(128);
        builder.Property(r => r.ResponseBody).HasColumnName("response_body");
        builder.Property(r => r.CreatedAt).HasColumnName("created_at").IsRequired();

        builder.HasIndex(r => r.CreatedAt);
    }
}
