using FlowCare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowCare.Infrastructure.Data.Configurations;

public class SlotConfiguration : IEntityTypeConfiguration<Slot>
{
    public void Configure(EntityTypeBuilder<Slot> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasMaxLength(50);

        // Soft-delete global query filter
        builder.HasQueryFilter(s => s.DeletedAt == null);

        builder.Property(s => s.BranchId).HasMaxLength(50);
        builder.Property(s => s.ServiceTypeId).HasMaxLength(50);
        builder.Property(s => s.StaffId).HasMaxLength(50);

        builder.HasOne(s => s.Branch)
            .WithMany(b => b.Slots)
            .HasForeignKey(s => s.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.ServiceType)
            .WithMany(st => st.Slots)
            .HasForeignKey(s => s.ServiceTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Staff)
            .WithMany(u => u.AssignedSlots)
            .HasForeignKey(s => s.StaffId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
