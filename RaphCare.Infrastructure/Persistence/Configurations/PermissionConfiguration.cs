using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaphCare.Domain.Identity;

namespace RaphCare.Infrastructure.Persistence.Configurations;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permissions");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Code).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Description).HasMaxLength(500);
        builder.HasIndex(e => e.Code).IsUnique();
        builder.HasIndex(e => e.IsDeleted);
    }
}
