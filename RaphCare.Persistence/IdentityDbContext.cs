using Microsoft.EntityFrameworkCore;
using RaphCare.Domain.Identity;
using RaphCare.Infrastructure.Persistence.Configurations;

namespace RaphCare.Persistence;

/// <summary>
/// Bounded context: Identity only. Manages ApplicationUser, Role, Permission and their link tables.
/// </summary>
public class IdentityDbContext : DbContext
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options) { }

    public DbSet<ApplicationUser> Users => Set<ApplicationUser>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new ApplicationUserConfiguration());
        modelBuilder.ApplyConfiguration(new RoleConfiguration());
        modelBuilder.ApplyConfiguration(new PermissionConfiguration());
        modelBuilder.ApplyConfiguration(new UserRoleConfiguration());
        modelBuilder.ApplyConfiguration(new RolePermissionConfiguration());
        modelBuilder.ApplyPersistenceConventions();
    }
}
