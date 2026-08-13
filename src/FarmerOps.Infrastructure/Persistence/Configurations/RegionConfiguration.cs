using FarmerOps.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmerOps.Infrastructure.Persistence.Configurations;

public class RegionConfiguration : IEntityTypeConfiguration<Region>
{
    public void Configure(EntityTypeBuilder<Region> builder)
    {
        builder.ToTable("Regions");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name).IsRequired().HasMaxLength(100);
        builder.Property(r => r.Code).IsRequired().HasMaxLength(10);
        builder.HasIndex(r => r.Code).IsUnique();

        builder.HasMany(r => r.Districts)
            .WithOne(d => d.Region)
            .HasForeignKey(d => d.RegionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
