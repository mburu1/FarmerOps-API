using FarmerOps.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmerOps.Infrastructure.Persistence.Configurations;

public class AlertConfiguration : IEntityTypeConfiguration<Alert>
{
    public void Configure(EntityTypeBuilder<Alert> builder)
    {
        builder.ToTable("Alerts");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Message).IsRequired().HasMaxLength(1000);
        builder.Property(a => a.Type).HasConversion<string>().HasMaxLength(30);
        builder.Property(a => a.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(a => a.FailureReason).HasMaxLength(500);

        builder.HasOne<Farmer>()
            .WithMany()
            .HasForeignKey(a => a.FarmerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<FieldAgent>()
            .WithMany()
            .HasForeignKey(a => a.AgentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => a.Status);
    }
}
