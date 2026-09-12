using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaphCare.Domain.Clinical;

namespace RaphCare.Infrastructure.Persistence.Configurations;

public sealed class AppointmentConsentConfiguration : IEntityTypeConfiguration<AppointmentConsent>
{
    public void Configure(EntityTypeBuilder<AppointmentConsent> builder)
    {
        builder.ToTable("AppointmentConsents");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.TitleSnapshot).IsRequired().HasMaxLength(200);
        builder.Property(e => e.BodySnapshot).IsRequired().HasMaxLength(8000);
        builder.Property(e => e.SignedAt).IsRequired();
        builder.Property(e => e.SignedByApplicationUserId).IsRequired();
        builder.HasIndex(e => e.AppointmentId).IsUnique();
        builder.HasIndex(e => e.PatientId);
        builder.HasIndex(e => e.ClinicId);
        builder.HasIndex(e => e.TemplateId);
    }
}
