using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaphCare.Domain.Patients;

namespace RaphCare.Infrastructure.Persistence.Configurations;

/// <summary>EF Core Fluent API configuration for the InsuranceProfile entity (insurance bounded context).</summary>
public class InsuranceProfileConfiguration : IEntityTypeConfiguration<InsuranceProfile>
{
    public void Configure(EntityTypeBuilder<InsuranceProfile> builder)
    {
        builder.ToTable("InsuranceProfiles");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.MembershipNumber).IsRequired().HasMaxLength(100);
        builder.HasIndex(e => e.PatientId);
        builder.HasIndex(e => e.InsurancePlanId);
        builder.HasOne(e => e.Patient)
            .WithMany(p => p.InsuranceProfiles)
            .HasForeignKey(e => e.PatientId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
