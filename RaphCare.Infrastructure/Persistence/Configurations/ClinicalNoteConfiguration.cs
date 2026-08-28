using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaphCare.Domain.Clinical;

namespace RaphCare.Infrastructure.Persistence.Configurations;

public class ClinicalNoteConfiguration : IEntityTypeConfiguration<ClinicalNote>
{
    public void Configure(EntityTypeBuilder<ClinicalNote> builder)
    {
        builder.Property(e => e.Notes).IsRequired().HasMaxLength(4000);
        builder.Property(e => e.Category).HasMaxLength(100);
    }
}
