using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaphCare.Domain.Organization;

namespace RaphCare.Infrastructure.Persistence.Configurations;

public sealed class ClinicStaffInvitationConfiguration : IEntityTypeConfiguration<ClinicStaffInvitation>
{
    public void Configure(EntityTypeBuilder<ClinicStaffInvitation> builder)
    {
        builder.ToTable("ClinicStaffInvitations");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.ClinicId).IsRequired();
        builder.Property(e => e.Email).IsRequired().HasMaxLength(256);
        builder.Property(e => e.InvitedByApplicationUserId).IsRequired();
        builder.Property(e => e.InvitedAt).IsRequired();
        builder.Property(e => e.IsCancelled).IsRequired();

        builder.HasIndex(e => new { e.ClinicId, e.Email });
        builder.HasIndex(e => e.ClinicId);
        builder.HasIndex(e => e.IsCancelled);

        builder.HasOne(e => e.Clinic)
            .WithMany()
            .HasForeignKey(e => e.ClinicId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
