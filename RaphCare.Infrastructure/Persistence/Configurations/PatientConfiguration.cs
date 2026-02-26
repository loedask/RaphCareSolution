using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaphCare.Domain.Patients;

namespace RaphCare.Infrastructure.Persistence.Configurations;

/// <summary>EF Core Fluent API configuration for the Patient entity (clinical bounded context).</summary>
public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.ToTable("Patients");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(e => e.LastName).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Gender).IsRequired().HasMaxLength(20);
        builder.Property(e => e.NationalIdNumber).HasMaxLength(50);
        builder.Property(e => e.PhoneNumber).HasMaxLength(30);
        builder.Property(e => e.Email).HasMaxLength(256);
        builder.HasIndex(e => e.ClinicId);
        builder.HasIndex(e => new { e.ClinicId, e.IsDeleted });
        builder.HasQueryFilter(e => !e.IsDeleted);
        builder.HasMany(e => e.Addresses).WithOne(a => a.Patient).HasForeignKey(a => a.PatientId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(e => e.InsuranceProfiles).WithOne(i => i.Patient).HasForeignKey(i => i.PatientId).OnDelete(DeleteBehavior.Restrict);
    }
}
