using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaphCare.Domain.Patients;

namespace RaphCare.Infrastructure.Persistence.Configurations;

public class PatientFamilyMemberConfiguration : IEntityTypeConfiguration<PatientFamilyMember>
{
    public void Configure(EntityTypeBuilder<PatientFamilyMember> builder)
    {
        builder.ToTable("PatientFamilyMembers");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(e => e.LastName).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Relationship).IsRequired().HasMaxLength(50);
        builder.Property(e => e.PhoneNumber).HasMaxLength(30);
        builder.Property(e => e.Email).HasMaxLength(256);
        builder.HasIndex(e => e.OwnerPatientId);
        builder.HasIndex(e => new { e.OwnerPatientId, e.IsActive });
        builder.HasOne<Patient>()
            .WithMany()
            .HasForeignKey(e => e.OwnerPatientId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Patient>()
            .WithMany()
            .HasForeignKey(e => e.LinkedPatientId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
