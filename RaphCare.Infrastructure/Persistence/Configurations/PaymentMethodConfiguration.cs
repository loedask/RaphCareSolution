using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaphCare.Domain.Billing;

namespace RaphCare.Infrastructure.Persistence.Configurations;

public class PaymentMethodConfiguration : IEntityTypeConfiguration<PaymentMethod>
{
    public void Configure(EntityTypeBuilder<PaymentMethod> builder)
    {
        builder.ToTable("PaymentMethods");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.MethodType).IsRequired().HasMaxLength(50);
        builder.Property(e => e.ProviderName).IsRequired().HasMaxLength(100);
        builder.Property(e => e.MaskedDetails).IsRequired().HasMaxLength(256);
        builder.HasIndex(e => e.PatientId);
        builder.HasIndex(e => new { e.PatientId, e.IsDefault });
    }
}
