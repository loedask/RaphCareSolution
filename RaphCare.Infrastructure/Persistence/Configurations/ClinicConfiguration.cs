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
        builder.Property(e => e.Country).IsRequired().HasMaxLength(100);
        builder.Property(e => e.TimeZone).IsRequired().HasMaxLength(50);
        builder.Property(e => e.RegisteredByApplicationUserId);
        builder.HasIndex(e => e.RegistrationNumber).IsUnique();
        builder.HasIndex(e => e.IsDeleted);
        builder.HasQueryFilter(e => !e.IsDeleted);
        builder.HasMany(e => e.Facilities).WithOne(f => f.Clinic).HasForeignKey(f => f.ClinicId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(e => e.Departments).WithOne(d => d.Clinic).HasForeignKey(d => d.ClinicId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(e => e.Providers).WithOne(p => p.Clinic).HasForeignKey(p => p.ClinicId).OnDelete(DeleteBehavior.Restrict);
    }
}
