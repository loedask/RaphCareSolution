using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaphCare.Domain.Billing;

namespace RaphCare.Infrastructure.Persistence.Configurations;

public class PriceCatalogItemConfiguration : IEntityTypeConfiguration<PriceCatalogItem>
{
    public void Configure(EntityTypeBuilder<PriceCatalogItem> builder)
    {
        builder.ToTable("PriceCatalogItems");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.SkuCode).IsRequired().HasMaxLength(PriceCatalogSku.SkuCodeMaxLength);
        builder.Property(e => e.DisplayName).IsRequired().HasMaxLength(PriceCatalogSku.DisplayNameMaxLength);
        builder.Property(e => e.Category).IsRequired().HasMaxLength(PriceCatalogSku.CategoryMaxLength);
        builder.Property(e => e.AmountZar).HasPrecision(18, 2);
        builder.Property(e => e.AmountUsd).HasPrecision(18, 2);
        builder.HasIndex(e => e.SkuCode).IsUnique();
        builder.HasIndex(e => e.Category);
        builder.HasIndex(e => e.IsActive);
    }
}
