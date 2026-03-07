using FlowCare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowCare.Infrastructure.Data.Configurations;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasMaxLength(50);

        builder.Property(a => a.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(a => a.AttachmentPath).HasMaxLength(500);
        builder.Property(a => a.Notes).HasMaxLength(2000);

        builder.Property(a => a.CustomerId).HasMaxLength(50);
        builder.Property(a => a.BranchId).HasMaxLength(50);
        builder.Property(a => a.ServiceTypeId).HasMaxLength(50);
        builder.Property(a => a.SlotId).HasMaxLength(50);
        builder.Property(a => a.StaffId).HasMaxLength(50);

        builder.HasOne(a => a.Customer)
            .WithMany(u => u.CustomerAppointments)
            .HasForeignKey(a => a.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Branch)
            .WithMany(b => b.Appointments)
            .HasForeignKey(a => a.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.ServiceType)
            .WithMany(st => st.Appointments)
            .HasForeignKey(a => a.ServiceTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Slot)
            .WithMany(s => s.Appointments)
            .HasForeignKey(a => a.SlotId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(a => a.Staff)
            .WithMany(u => u.StaffAppointments)
            .HasForeignKey(a => a.StaffId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
