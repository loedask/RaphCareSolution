using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaphCare.Domain.Clinical;

namespace RaphCare.Infrastructure.Persistence.Configurations;

public class LabRequestConfiguration : IEntityTypeConfiguration<LabRequest>
{
    public void Configure(EntityTypeBuilder<LabRequest> builder)
    {
        builder.Property(e => e.Status).IsRequired().HasMaxLength(32);
        builder.Property(e => e.PickupCode).IsRequired().HasMaxLength(8);
        builder.HasIndex(e => e.PickupCode).IsUnique();
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.CalledAt);
    }
}
