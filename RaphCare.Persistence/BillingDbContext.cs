using Microsoft.EntityFrameworkCore;
using RaphCare.Domain.Billing;
using RaphCare.Infrastructure.Persistence.Configurations;

namespace RaphCare.Persistence;

/// <summary>
/// Bounded context: Billing only. No clinical entities.
/// </summary>
public class BillingDbContext : DbContext
{
    public BillingDbContext(DbContextOptions<BillingDbContext> options) : base(options) { }

    public DbSet<Invoice> Invoices => Set<Invoice>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new InvoiceConfiguration());
        modelBuilder.ApplyPersistenceConventions();
    }
}
