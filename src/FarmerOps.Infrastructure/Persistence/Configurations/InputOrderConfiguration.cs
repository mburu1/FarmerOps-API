using FarmerOps.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmerOps.Infrastructure.Persistence.Configurations;

public class InputOrderConfiguration : IEntityTypeConfiguration<InputOrder>
{
    public void Configure(EntityTypeBuilder<InputOrder> builder)
    {
        builder.ToTable("InputOrders");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Quantity).HasPrecision(18, 2);
        builder.Property(o => o.UnitCost).HasPrecision(18, 2);
        builder.Property(o => o.InputType).HasConversion<string>().HasMaxLength(20);
        builder.Property(o => o.Status).HasConversion<string>().HasMaxLength(20);
        builder.Ignore(o => o.TotalCost);
        builder.Ignore(o => o.DomainEvents);

        builder.HasOne(o => o.Farmer)
            .WithMany()
            .HasForeignKey(o => o.FarmerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
