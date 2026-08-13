using FarmerOps.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmerOps.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email).IsRequired().HasMaxLength(200);
        builder.Property(u => u.PasswordHash).IsRequired();
        builder.Property(u => u.Role).HasConversion<string>().HasMaxLength(30);

        builder.HasIndex(u => u.Email).IsUnique();

        builder.HasMany(u => u.RefreshTokens)
            .WithOne(rt => rt.User)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(u => u.RefreshTokens).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Ignore(u => u.DomainEvents);
    }
}
