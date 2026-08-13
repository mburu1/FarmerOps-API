using FarmerOps.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmerOps.Infrastructure.Persistence.Configurations;

public class FarmerConfiguration : IEntityTypeConfiguration<Farmer>
{
    public void Configure(EntityTypeBuilder<Farmer> builder)
    {
        builder.ToTable("Farmers");
        builder.HasKey(f => f.Id);

        builder.Property(f => f.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(f => f.LastName).IsRequired().HasMaxLength(100);
        builder.Property(f => f.PhoneNumber).IsRequired().HasMaxLength(20);
        builder.Property(f => f.NationalId).IsRequired().HasMaxLength(30);
        builder.Property(f => f.FarmSizeAcres).HasPrecision(10, 2);
        builder.Property(f => f.PrimaryCrop).HasConversion<string>().HasMaxLength(30);

        builder.HasIndex(f => f.NationalId).IsUnique();

        builder.HasOne(f => f.District)
            .WithMany()
            .HasForeignKey(f => f.DistrictId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(f => f.Loans)
            .WithOne(l => l.Farmer)
            .HasForeignKey(l => l.FarmerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(f => f.Loans).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Ignore(f => f.DomainEvents);
    }
}
