using FlowCare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowCare.Infrastructure.Data.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasMaxLength(50);

        builder.Property(a => a.ActorId).HasMaxLength(50).IsRequired();
        builder.Property(a => a.ActorRole).HasMaxLength(20).IsRequired();
        builder.Property(a => a.ActionType).HasMaxLength(50).IsRequired();
        builder.Property(a => a.EntityType).HasMaxLength(50).IsRequired();
        builder.Property(a => a.EntityId).HasMaxLength(100).IsRequired();
        builder.Property(a => a.Metadata).HasColumnType("jsonb");

        builder.HasIndex(a => a.Timestamp);
        builder.HasIndex(a => a.ActorId);
    }
}
