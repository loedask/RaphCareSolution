using Microsoft.EntityFrameworkCore;
using RaphCare.Domain.Identity;

namespace RaphCare.Infrastructure.Persistence;

public class IdentityDbContext(DbContextOptions<IdentityDbContext> options) : DbContext(options)
{
    public DbSet<ApplicationUser> Users => Set<ApplicationUser>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserSession> UserSessions => Set<UserSession>();
    public DbSet<RefreshTokenRecord> RefreshTokenRecords => Set<RefreshTokenRecord>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<LoginAudit> LoginAudits => Set<LoginAudit>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new Configurations.ApplicationUserConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.RoleConfiguration());
    }
}
