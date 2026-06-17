using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaphCare.Domain.Patients;

namespace RaphCare.Infrastructure.Persistence.Configurations;

/// <summary>EF Core configuration for PatientIdentityEvent audit records.</summary>
public class PatientIdentityEventConfiguration : IEntityTypeConfiguration<PatientIdentityEvent>
{
    public void Configure(EntityTypeBuilder<PatientIdentityEvent> builder)
    {
        builder.ToTable("PatientIdentityEvents");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.PatientId).IsRequired();
        builder.Property(e => e.EventType).IsRequired();
        builder.Property(e => e.EventDataJson).IsRequired().HasColumnType("nvarchar(max)");
        builder.Property(e => e.OccurredAt).IsRequired();
        builder.Property(e => e.PerformedByUserId);
        builder.Property(e => e.CorrelationId).HasMaxLength(128);

        builder.HasIndex(e => e.PatientId);
        builder.HasIndex(e => e.EventType);
        builder.HasIndex(e => e.OccurredAt);

        builder.HasOne<Patient>()
            .WithMany()
            .HasForeignKey(e => e.PatientId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

