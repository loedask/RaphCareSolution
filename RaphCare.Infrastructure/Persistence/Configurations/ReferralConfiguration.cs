using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaphCare.Domain.Clinical;

namespace RaphCare.Infrastructure.Persistence.Configurations;

public sealed class ReferralConfiguration : IEntityTypeConfiguration<Referral>
{
    public void Configure(EntityTypeBuilder<Referral> builder)
    {
        builder.ToTable("Referrals");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.ReferredTo).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Reason).HasMaxLength(500);
        builder.Property(e => e.Specialty).HasMaxLength(100);
        builder.Property(e => e.Notes).HasMaxLength(1000);
        builder.Property(e => e.Status).IsRequired().HasMaxLength(32);
        builder.Property(e => e.ReferredAt).IsRequired();
        builder.HasIndex(e => e.ClinicId);
        builder.HasIndex(e => new { e.ClinicId, e.Status });
        builder.HasIndex(e => e.PatientId);
        builder.HasIndex(e => e.VisitId);
        builder.HasOne(e => e.Visit)
            .WithMany()
            .HasForeignKey(e => e.VisitId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);
    }
}
