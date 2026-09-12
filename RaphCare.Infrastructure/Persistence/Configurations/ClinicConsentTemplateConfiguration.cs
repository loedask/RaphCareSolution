using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaphCare.Domain.Clinical;

namespace RaphCare.Infrastructure.Persistence.Configurations;

public sealed class ClinicConsentTemplateConfiguration : IEntityTypeConfiguration<ClinicConsentTemplate>
{
    public void Configure(EntityTypeBuilder<ClinicConsentTemplate> builder)
    {
        builder.ToTable("ClinicConsentTemplates");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Title).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Body).IsRequired().HasMaxLength(8000);
        builder.Property(e => e.IsActive).IsRequired();
        builder.HasIndex(e => e.ClinicId);
        builder.HasIndex(e => new { e.ClinicId, e.IsActive });
    }
}
