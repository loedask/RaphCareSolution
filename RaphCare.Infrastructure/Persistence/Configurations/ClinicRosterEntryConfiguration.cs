using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaphCare.Domain.Organization;

namespace RaphCare.Infrastructure.Persistence.Configurations;

public sealed class ClinicRosterEntryConfiguration : IEntityTypeConfiguration<ClinicRosterEntry>
{
    public void Configure(EntityTypeBuilder<ClinicRosterEntry> builder)
    {
        builder.ToTable("ClinicRosterEntries");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.ShiftLabel).IsRequired().HasMaxLength(32);
        builder.Property(e => e.Note).HasMaxLength(200);
        builder.Property(e => e.DutyDate).IsRequired();
        builder.HasIndex(e => new { e.ClinicId, e.DutyDate });
        builder.HasIndex(e => new { e.ClinicId, e.DutyDate, e.ApplicationUserId, e.ShiftLabel })
            .IsUnique();
    }
}
