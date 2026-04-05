using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaphCare.Domain.Billing;

namespace RaphCare.Infrastructure.Persistence.Configurations;

public class PatientCarePlanConfiguration : IEntityTypeConfiguration<PatientCarePlan>
{
    public void Configure(EntityTypeBuilder<PatientCarePlan> builder)
    {
        builder.ToTable("PatientCarePlans");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.PlanCode).IsRequired().HasMaxLength(64);
        builder.Property(e => e.PlanDisplayName).IsRequired().HasMaxLength(128);
        builder.Property(e => e.Status).IsRequired().HasMaxLength(50);
        builder.HasIndex(e => e.PatientId).IsUnique();
    }
}
