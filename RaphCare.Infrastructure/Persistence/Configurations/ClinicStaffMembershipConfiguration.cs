using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaphCare.Domain.Organization;

namespace RaphCare.Infrastructure.Persistence.Configurations;

public sealed class ClinicStaffMembershipConfiguration : IEntityTypeConfiguration<ClinicStaffMembership>
{
    public void Configure(EntityTypeBuilder<ClinicStaffMembership> builder)
    {
        builder.ToTable("ClinicStaffMemberships");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.ApplicationUserId).IsRequired();
        builder.Property(e => e.ClinicId).IsRequired();
        builder.Property(e => e.IsActive).IsRequired();
        builder.Property(e => e.JoinedAt).IsRequired();

        builder.HasIndex(e => new { e.ApplicationUserId, e.ClinicId }).IsUnique();
        builder.HasIndex(e => e.ClinicId);
        builder.HasIndex(e => e.IsActive);

        builder.HasOne(e => e.Clinic)
            .WithMany(c => c.StaffMemberships)
            .HasForeignKey(e => e.ClinicId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
