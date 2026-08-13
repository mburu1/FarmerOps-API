using FarmerOps.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmerOps.Infrastructure.Persistence.Configurations;

public class DistrictConfiguration : IEntityTypeConfiguration<District>
{
    public void Configure(EntityTypeBuilder<District> builder)
    {
        builder.ToTable("Districts");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Name).IsRequired().HasMaxLength(100);
        builder.HasIndex(d => new { d.RegionId, d.Name }).IsUnique();
    }
}
