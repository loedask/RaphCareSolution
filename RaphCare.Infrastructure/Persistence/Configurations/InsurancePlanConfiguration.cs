using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaphCare.Domain.Insurance;

namespace RaphCare.Infrastructure.Persistence.Configurations;

public class InsurancePlanConfiguration : IEntityTypeConfiguration<InsurancePlan>
{
    public void Configure(EntityTypeBuilder<InsurancePlan> builder)
    {
        builder.ToTable("InsurancePlans");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);
        builder.Property(e => e.Code)
            .IsRequired()
            .HasMaxLength(50);
        builder.Property(e => e.Description)
            .HasMaxLength(500);

        builder.HasIndex(e => e.Code).IsUnique();
    }
}
