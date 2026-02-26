using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaphCare.Domain.Identity;

namespace RaphCare.Infrastructure.Persistence.Configurations;

/// <summary>EF Core Fluent API configuration for the RolePermission join entity (identity bounded context).</summary>
public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("RolePermissions");
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => e.RoleId);
        builder.HasIndex(e => e.PermissionId);
        builder.HasIndex(e => new { e.RoleId, e.PermissionId }).IsUnique();
        builder.HasOne(e => e.Role).WithMany(r => r.RolePermissions).HasForeignKey(e => e.RoleId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Permission).WithMany(p => p.RolePermissions).HasForeignKey(e => e.PermissionId).OnDelete(DeleteBehavior.Restrict);
    }
}
