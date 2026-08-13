using FarmerOps.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmerOps.Infrastructure.Persistence.Configurations;

public class AgentAssignmentConfiguration : IEntityTypeConfiguration<AgentAssignment>
{
    public void Configure(EntityTypeBuilder<AgentAssignment> builder)
    {
        builder.ToTable("AgentAssignments");
        builder.HasKey(a => a.Id);

        builder.HasOne(a => a.Farmer)
            .WithMany()
            .HasForeignKey(a => a.FarmerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => new { a.FarmerId, a.IsActive });
    }
}
