using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaphCare.Domain.Clinical;

namespace RaphCare.Infrastructure.Persistence.Configurations;

public class SOAPNoteConfiguration : IEntityTypeConfiguration<SOAPNote>
{
    public void Configure(EntityTypeBuilder<SOAPNote> builder)
    {
        builder.Property(e => e.Subjective).HasMaxLength(4000);
        builder.Property(e => e.Objective).HasMaxLength(4000);
        builder.Property(e => e.Assessment).HasMaxLength(4000);
        builder.Property(e => e.Plan).HasMaxLength(4000);
    }
}
