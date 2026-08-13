using FarmerOps.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmerOps.Infrastructure.Persistence.Configurations;

public class LoanConfiguration : IEntityTypeConfiguration<Loan>
{
    public void Configure(EntityTypeBuilder<Loan> builder)
    {
        builder.ToTable("Loans");
        builder.HasKey(l => l.Id);

        builder.Property(l => l.PrincipalAmount).HasPrecision(18, 2);
        builder.Property(l => l.OutstandingBalance).HasPrecision(18, 2);
        builder.Property(l => l.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(l => l.RejectionReason).HasMaxLength(500);

        builder.HasMany(l => l.InputOrders)
            .WithOne(o => o.Loan)
            .HasForeignKey(o => o.LoanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(l => l.InputOrders).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Ignore(l => l.DomainEvents);

        builder.HasIndex(l => l.Status);
    }
}
