using Microsoft.EntityFrameworkCore;
using RaphCare.Domain.Billing;
using RaphCare.Infrastructure.Persistence.Configurations;

namespace RaphCare.Persistence;

/// <summary>
/// Bounded context: Billing only. No clinical entities.
/// </summary>
public class BillingDbContext(DbContextOptions<BillingDbContext> options) : DbContext(options)
{
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceLineItem> InvoiceLineItems => Set<InvoiceLineItem>();
    public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();
    public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();
    public DbSet<PatientCarePlan> PatientCarePlans => Set<PatientCarePlan>();
    public DbSet<PriceCatalogItem> PriceCatalogItems => Set<PriceCatalogItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new InvoiceConfiguration());
        modelBuilder.ApplyConfiguration(new InvoiceLineItemConfiguration());
        modelBuilder.ApplyConfiguration(new PaymentMethodConfiguration());
        modelBuilder.ApplyConfiguration(new PaymentTransactionConfiguration());
        modelBuilder.ApplyConfiguration(new PatientCarePlanConfiguration());
        modelBuilder.ApplyConfiguration(new PriceCatalogItemConfiguration());
        modelBuilder.ApplyPersistenceConventions();
    }
}
