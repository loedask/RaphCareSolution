using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaphCare.Domain.Clinical;

namespace RaphCare.Infrastructure.Persistence.Configurations;

public sealed class TheatreCaseConfiguration : IEntityTypeConfiguration<TheatreCase>
{
    public void Configure(EntityTypeBuilder<TheatreCase> builder)
    {
        builder.ToTable("TheatreCases");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.ProcedureName).IsRequired().HasMaxLength(200);
        builder.Property(e => e.TheatreName).HasMaxLength(100);
        builder.Property(e => e.SurgeonName).HasMaxLength(200);
        builder.Property(e => e.Status).IsRequired().HasMaxLength(32);
        builder.Property(e => e.Notes).HasMaxLength(1000);
        builder.Property(e => e.ScheduledStart).IsRequired();
        builder.HasIndex(e => e.ClinicId);
        builder.HasIndex(e => new { e.ClinicId, e.ScheduledStart });
        builder.HasIndex(e => e.PatientId);
        builder.HasIndex(e => e.Status);
    }
}
