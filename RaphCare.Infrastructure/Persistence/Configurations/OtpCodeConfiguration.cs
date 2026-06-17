using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaphCare.Domain.Identity;

namespace RaphCare.Infrastructure.Persistence.Configurations;

/// <summary>EF Core Fluent API configuration for phone OTP codes.</summary>
public class OtpCodeConfiguration : IEntityTypeConfiguration<OtpCode>
{
    public void Configure(EntityTypeBuilder<OtpCode> builder)
    {
        builder.ToTable("OtpCodes");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.PhoneNumber)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(e => e.CodeHash)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(e => e.ExpiresAt)
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.HasIndex(e => e.PhoneNumber);
        builder.HasIndex(e => e.ExpiresAt);
        builder.HasIndex(e => e.IsUsed);
    }
}

