using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaphCare.Domain.Clinical;

namespace RaphCare.Infrastructure.Persistence.Configurations;

public sealed class InpatientObservationConfiguration : IEntityTypeConfiguration<InpatientObservation>
{
    public void Configure(EntityTypeBuilder<InpatientObservation> builder)
    {
        builder.ToTable("InpatientObservations");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Note).IsRequired().HasMaxLength(2000);
        builder.Property(e => e.HeartRate).HasPrecision(8, 2);
        builder.Property(e => e.TemperatureCelsius).HasPrecision(5, 2);
        builder.Property(e => e.OxygenSaturation).HasPrecision(5, 2);
        builder.Property(e => e.SystolicBp).HasPrecision(8, 2);
        builder.Property(e => e.DiastolicBp).HasPrecision(8, 2);
        builder.Property(e => e.RecordedAt).IsRequired();
        builder.HasIndex(e => e.ClinicId);
        builder.HasIndex(e => e.AdmissionId);
        builder.HasOne(e => e.Admission)
            .WithMany()
            .HasForeignKey(e => e.AdmissionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
