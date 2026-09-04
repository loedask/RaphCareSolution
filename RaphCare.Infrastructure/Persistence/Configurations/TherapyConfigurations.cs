using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaphCare.Domain.MentalHealth;
using RaphCare.Domain.Organization;
using RaphCare.Domain.Patients;

namespace RaphCare.Infrastructure.Persistence.Configurations;

public sealed class TherapySessionConfiguration : IEntityTypeConfiguration<TherapySession>
{
    public void Configure(EntityTypeBuilder<TherapySession> builder)
    {
        builder.ToTable("TherapySessions");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.SessionType).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Status).IsRequired().HasMaxLength(50);
        builder.Property(e => e.Summary).HasMaxLength(4000);
        builder.HasIndex(e => e.ClinicId);
        builder.HasIndex(e => e.PatientId);
        builder.HasIndex(e => new { e.ClinicId, e.SessionStart });
        builder.HasOne<Clinic>()
            .WithMany()
            .HasForeignKey(e => e.ClinicId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Patient>()
            .WithMany()
            .HasForeignKey(e => e.PatientId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(e => e.TherapyNotes)
            .WithOne(n => n.TherapySession)
            .HasForeignKey(n => n.TherapySessionId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(e => e.CrisisFlags)
            .WithOne(c => c.TherapySession)
            .HasForeignKey(c => c.TherapySessionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class TherapyNoteConfiguration : IEntityTypeConfiguration<TherapyNote>
{
    public void Configure(EntityTypeBuilder<TherapyNote> builder)
    {
        builder.ToTable("TherapyNotes");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Notes).IsRequired().HasMaxLength(8000);
        builder.Property(e => e.Category).IsRequired().HasMaxLength(100);
        builder.HasIndex(e => e.TherapySessionId);
    }
}

public sealed class CrisisFlagConfiguration : IEntityTypeConfiguration<CrisisFlag>
{
    public void Configure(EntityTypeBuilder<CrisisFlag> builder)
    {
        builder.ToTable("CrisisFlags");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.RiskLevel).IsRequired().HasMaxLength(50);
        builder.Property(e => e.Description).IsRequired().HasMaxLength(2000);
        builder.HasIndex(e => e.TherapySessionId);
        builder.HasIndex(e => e.PatientId);
    }
}

public sealed class BehavioralCarePlanConfiguration : IEntityTypeConfiguration<BehavioralCarePlan>
{
    public void Configure(EntityTypeBuilder<BehavioralCarePlan> builder)
    {
        builder.ToTable("BehavioralCarePlans");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Title).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Description).IsRequired().HasMaxLength(4000);
        builder.Property(e => e.Status).IsRequired().HasMaxLength(50);
        builder.HasIndex(e => e.ClinicId);
        builder.HasIndex(e => e.PatientId);
        builder.HasOne<Clinic>()
            .WithMany()
            .HasForeignKey(e => e.ClinicId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Patient>()
            .WithMany()
            .HasForeignKey(e => e.PatientId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(e => e.Goals)
            .WithOne(g => g.BehavioralCarePlan)
            .HasForeignKey(g => g.BehavioralCarePlanId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class TherapyGoalConfiguration : IEntityTypeConfiguration<TherapyGoal>
{
    public void Configure(EntityTypeBuilder<TherapyGoal> builder)
    {
        builder.ToTable("TherapyGoals");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.GoalDescription).IsRequired().HasMaxLength(1000);
        builder.HasIndex(e => e.BehavioralCarePlanId);
        builder.Ignore(e => e.ProgressEntries);
    }
}
