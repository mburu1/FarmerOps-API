using FarmerOps.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmerOps.Infrastructure.Persistence.Configurations;

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Type).IsRequired().HasMaxLength(200);
        builder.Property(m => m.Content).IsRequired();
        builder.Property(m => m.Error).HasMaxLength(2000);

        builder.HasIndex(m => m.ProcessedOnUtc);
    }
}
