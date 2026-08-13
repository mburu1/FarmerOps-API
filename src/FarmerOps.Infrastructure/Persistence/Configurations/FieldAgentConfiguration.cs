using FarmerOps.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmerOps.Infrastructure.Persistence.Configurations;

public class FieldAgentConfiguration : IEntityTypeConfiguration<FieldAgent>
{
    public void Configure(EntityTypeBuilder<FieldAgent> builder)
    {
        builder.ToTable("FieldAgents");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(a => a.LastName).IsRequired().HasMaxLength(100);
        builder.Property(a => a.PhoneNumber).IsRequired().HasMaxLength(20);
        builder.Property(a => a.Email).IsRequired().HasMaxLength(150);

        builder.HasIndex(a => a.Email).IsUnique();

        builder.HasMany(a => a.Assignments)
            .WithOne(x => x.Agent)
            .HasForeignKey(x => x.AgentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(a => a.DomainEvents);
    }
}
