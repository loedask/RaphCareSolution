using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaphCare.Domain.Telemedicine;

namespace RaphCare.Infrastructure.Persistence.Configurations;

public class TeleSessionConfiguration : IEntityTypeConfiguration<TeleSession>
{
    public void Configure(EntityTypeBuilder<TeleSession> builder)
    {
        builder.ToTable("TeleSessions");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Status).IsRequired().HasMaxLength(50);
        builder.Property(e => e.Platform).IsRequired().HasMaxLength(100);
        builder.HasIndex(e => e.ClinicId);
        builder.HasIndex(e => e.AppointmentId);
    }
}
