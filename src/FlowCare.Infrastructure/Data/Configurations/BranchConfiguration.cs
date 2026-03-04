using FlowCare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowCare.Infrastructure.Data.Configurations;

public class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.SeedId).HasMaxLength(50);
        builder.HasIndex(b => b.SeedId).IsUnique().HasFilter("\"SeedId\" != ''");

        builder.Property(b => b.Name).HasMaxLength(200).IsRequired();
        builder.Property(b => b.City).HasMaxLength(100).IsRequired();
        builder.Property(b => b.Address).HasMaxLength(500).IsRequired();
        builder.Property(b => b.Timezone).HasMaxLength(50).IsRequired();
    }
}
