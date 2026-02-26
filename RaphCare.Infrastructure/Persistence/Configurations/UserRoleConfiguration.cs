using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaphCare.Domain.Identity;

namespace RaphCare.Infrastructure.Persistence.Configurations;

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("UserRoles");
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.RoleId);
        builder.HasIndex(e => new { e.UserId, e.RoleId }).IsUnique();
        builder.HasOne(e => e.User).WithMany(u => u.UserRoles).HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Role).WithMany(r => r.UserRoles).HasForeignKey(e => e.RoleId).OnDelete(DeleteBehavior.Restrict);
    }
}
