using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaphCare.Domain.Organization;

namespace RaphCare.Infrastructure.Persistence.Configurations;

/// <summary>EF Core Fluent API configuration for the Clinic entity (clinical bounded context).</summary>
public class ClinicConfiguration : IEntityTypeConfiguration<Clinic>
{
    public void Configure(EntityTypeBuilder<Clinic> builder)
    {
        builder.ToTable("Clinics");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
        builder.Property(e => e.RegistrationNumber).IsRequired().HasMaxLength(100);
        builder.Property(e => e.ReferenceCode).IsRequired().HasMaxLength(ClinicReferenceCode.MaxLength);
        builder.Property(e => e.Country).IsRequired().HasMaxLength(100);
        builder.Property(e => e.TimeZone).IsRequired().HasMaxLength(50);
        builder.Property(e => e.RegisteredByApplicationUserId);
        builder.Property(e => e.CollectionDisplayToken).HasMaxLength(ClinicCollectionDisplayToken.Length);
        builder.Property(e => e.CasualtyDisplayToken).HasMaxLength(ClinicCollectionDisplayToken.Length);
        builder.HasIndex(e => e.RegistrationNumber);
        builder.HasIndex(e => e.ReferenceCode).IsUnique();
        builder.HasIndex(e => e.CollectionDisplayToken)
            .IsUnique()
            .HasFilter("[CollectionDisplayToken] IS NOT NULL");
        builder.HasIndex(e => e.CasualtyDisplayToken)
            .IsUnique()
            .HasFilter("[CasualtyDisplayToken] IS NOT NULL");
        builder.HasIndex(e => e.IsDeleted);
        builder.HasQueryFilter(e => !e.IsDeleted);
        builder.HasMany(e => e.Facilities).WithOne(f => f.Clinic).HasForeignKey(f => f.ClinicId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(e => e.Departments).WithOne(d => d.Clinic).HasForeignKey(d => d.ClinicId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(e => e.Providers).WithOne(p => p.Clinic).HasForeignKey(p => p.ClinicId).OnDelete(DeleteBehavior.Restrict);
    }
}
