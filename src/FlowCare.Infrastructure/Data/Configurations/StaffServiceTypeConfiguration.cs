using FlowCare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowCare.Infrastructure.Data.Configurations;

public class StaffServiceTypeConfiguration : IEntityTypeConfiguration<StaffServiceType>
{
    public void Configure(EntityTypeBuilder<StaffServiceType> builder)
    {
        builder.HasKey(sst => new { sst.StaffId, sst.ServiceTypeId });

        builder.HasOne(sst => sst.Staff)
            .WithMany(u => u.StaffServiceTypes)
            .HasForeignKey(sst => sst.StaffId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sst => sst.ServiceType)
            .WithMany(st => st.StaffServiceTypes)
            .HasForeignKey(sst => sst.ServiceTypeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
