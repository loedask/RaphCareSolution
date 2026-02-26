using Microsoft.EntityFrameworkCore;
using RaphCare.Domain.Clinical;
using RaphCare.Domain.Organization;
using RaphCare.Domain.Patients;
using RaphCare.Infrastructure.Persistence.Configurations;

namespace RaphCare.Persistence;

/// <summary>
/// Bounded context: Clinical only. No financial entities.
/// </summary>
public class ClinicalDbContext : DbContext
{
    public ClinicalDbContext(DbContextOptions<ClinicalDbContext> options) : base(options) { }

    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Clinic> Clinics => Set<Clinic>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Visit> Visits => Set<Visit>();
    public DbSet<CarePlan> CarePlans => Set<CarePlan>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new PatientConfiguration());
        modelBuilder.ApplyConfiguration(new ClinicConfiguration());
        modelBuilder.ApplyConfiguration(new VisitConfiguration());
        modelBuilder.ApplyPersistenceConventions();
    }
}
