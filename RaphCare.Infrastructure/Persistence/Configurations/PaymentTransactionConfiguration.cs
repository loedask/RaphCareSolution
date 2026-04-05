using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaphCare.Domain.Billing;

namespace RaphCare.Infrastructure.Persistence.Configurations;

public class PaymentTransactionConfiguration : IEntityTypeConfiguration<PaymentTransaction>
{
    public void Configure(EntityTypeBuilder<PaymentTransaction> builder)
    {
        builder.ToTable("PaymentTransactions");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Currency).IsRequired().HasMaxLength(10);
        builder.Property(e => e.PaymentStatus).IsRequired().HasMaxLength(50);
        builder.Property(e => e.PaymentChannel).IsRequired().HasMaxLength(50);
        builder.Property(e => e.ExternalTransactionReference).IsRequired().HasMaxLength(256);
        builder.Property(e => e.Amount).HasPrecision(18, 2);
        builder.HasIndex(e => e.PatientId);
        builder.HasIndex(e => e.InvoiceId);
        builder.HasOne(e => e.Invoice).WithMany().HasForeignKey(e => e.InvoiceId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.PaymentMethod).WithMany(m => m.PaymentTransactions).HasForeignKey(e => e.PaymentMethodId).OnDelete(DeleteBehavior.SetNull);
        builder.Ignore(e => e.Refunds);
        builder.Ignore(e => e.MobileMoneyTransaction);
        builder.Ignore(e => e.CardTransaction);
    }
}
