using FarmerOps.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmerOps.Infrastructure.Persistence.Configurations;

public class VisitConfiguration : IEntityTypeConfiguration<Visit>
{
    public void Configure(EntityTypeBuilder<Visit> builder)
    {
        builder.ToTable("Visits");
        builder.HasKey(v => v.Id);

        builder.Property(v => v.Notes).HasMaxLength(1000);
        builder.Property(v => v.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasOne(v => v.Agent)
            .WithMany()
            .HasForeignKey(v => v.AgentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(v => v.Farmer)
            .WithMany()
            .HasForeignKey(v => v.FarmerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(v => v.DomainEvents);
    }
}
