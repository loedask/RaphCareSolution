using Microsoft.EntityFrameworkCore;
using RaphCare.Domain.Clinical;
using RaphCare.Domain.Organization;
using RaphCare.Domain.Patients;

namespace RaphCare.Infrastructure.Persistence;

public class ClinicalDbContext(DbContextOptions<ClinicalDbContext> options) : DbContext(options)
{
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Clinic> Clinics => Set<Clinic>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Visit> Visits => Set<Visit>();
    public DbSet<CarePlan> CarePlans => Set<CarePlan>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new Configurations.PatientConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.ClinicConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.VisitConfiguration());
    }
}
