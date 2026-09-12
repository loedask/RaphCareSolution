using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaphCare.Domain.Clinical;

namespace RaphCare.Infrastructure.Persistence.Configurations;

public sealed class ConsultTicketConfiguration : IEntityTypeConfiguration<ConsultTicket>
{
    public void Configure(EntityTypeBuilder<ConsultTicket> builder)
    {
        builder.ToTable("ConsultTickets");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.QueueCode).IsRequired().HasMaxLength(6);
        builder.Property(e => e.Status).IsRequired().HasMaxLength(32);
        builder.Property(e => e.ArrivedAt).IsRequired();
        builder.HasIndex(e => e.ClinicId);
        builder.HasIndex(e => new { e.ClinicId, e.Status });
        builder.HasIndex(e => e.QueueCode);
        builder.HasIndex(e => e.AppointmentId);
        builder.HasIndex(e => e.CalledAt);
    }
}
