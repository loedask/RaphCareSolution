using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Persistence.Utilities;
using RaphCare.Domain.Billing;
using RaphCare.Domain.Clinical;
using RaphCare.Domain.Communication;
using RaphCare.Domain.Devices;
using RaphCare.Domain.Insurance;
using RaphCare.Domain.MentalHealth;
using RaphCare.Domain.Organization;
using RaphCare.Domain.Patients;
using RaphCare.Domain.Telemedicine;
using RaphCare.Domain.Reporting;
using RaphCare.Infrastructure.Persistence.Interceptors;
using RaphCare.Persistence.Repositories;
using RaphCare.Persistence.FhirMappers;
using RaphCare.Persistence.Services;

namespace RaphCare.Persistence;

/// <summary>Persistence layer registration: all bounded-context DbContexts, interceptors, and connection strings.</summary>
public static class DependencyInjection
{
    private const string DefaultConnectionName = "DefaultConnection";
    private const string MigrationsAssemblyName = "RaphCare.Persistence";

    /// <summary>Registers all DbContexts (Identity, Clinical, Device, Insurance, Billing, AI) with SQL Server, interceptors, and development-only options.</summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">Application configuration (connection strings, environment).</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var env = configuration["ASPNETCORE_ENVIRONMENT"] ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "";
        var isDevelopment = string.Equals(env, "Development", StringComparison.OrdinalIgnoreCase);

        var connectionString = configuration.GetConnectionString(DefaultConnectionName)
            ?? throw new InvalidOperationException($"Connection string '{DefaultConnectionName}' not found.");

        string GetConnectionString(string name) =>
            configuration.GetConnectionString(name) ?? connectionString;

        services.AddDbContext<IdentityDbContext>((sp, options) =>
        {
            options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
            options.UseSqlServer(GetConnectionString("IdentityConnection"), sql =>
                sql.MigrationsAssembly(MigrationsAssemblyName));
            options.AddInterceptors(
                sp.GetRequiredService<AuditableEntityInterceptor>(),
                sp.GetRequiredService<SoftDeleteInterceptor>(),
                sp.GetRequiredService<DomainEventDispatcherInterceptor>());
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            if (isDevelopment)
            {
                options.EnableDetailedErrors();
                options.EnableSensitiveDataLogging();
            }
        });

        services.AddDbContext<ClinicalDbContext>((sp, options) =>
        {
            options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
            options.UseSqlServer(GetConnectionString("ClinicalConnection"), sql =>
                sql.MigrationsAssembly(MigrationsAssemblyName));
            options.AddInterceptors(
                sp.GetRequiredService<AuditableEntityInterceptor>(),
                sp.GetRequiredService<SoftDeleteInterceptor>(),
                sp.GetRequiredService<DomainEventDispatcherInterceptor>());
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            if (isDevelopment)
            {
                options.EnableDetailedErrors();
                options.EnableSensitiveDataLogging();
            }
        });

        services.AddDbContext<DeviceDbContext>((sp, options) =>
        {
            options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
            options.UseSqlServer(GetConnectionString("DeviceConnection"), sql =>
                sql.MigrationsAssembly(MigrationsAssemblyName));
            options.AddInterceptors(
                sp.GetRequiredService<AuditableEntityInterceptor>(),
                sp.GetRequiredService<SoftDeleteInterceptor>(),
                sp.GetRequiredService<DomainEventDispatcherInterceptor>());
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            if (isDevelopment)
            {
                options.EnableDetailedErrors();
                options.EnableSensitiveDataLogging();
            }
        });

        services.AddDbContext<InsuranceDbContext>((sp, options) =>
        {
            options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
            options.UseSqlServer(GetConnectionString("InsuranceConnection"), sql =>
                sql.MigrationsAssembly(MigrationsAssemblyName));
            options.AddInterceptors(
                sp.GetRequiredService<AuditableEntityInterceptor>(),
                sp.GetRequiredService<SoftDeleteInterceptor>(),
                sp.GetRequiredService<DomainEventDispatcherInterceptor>());
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            if (isDevelopment)
            {
                options.EnableDetailedErrors();
                options.EnableSensitiveDataLogging();
            }
        });

        services.AddDbContext<BillingDbContext>((sp, options) =>
        {
            options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
            options.UseSqlServer(GetConnectionString("BillingConnection"), sql =>
                sql.MigrationsAssembly(MigrationsAssemblyName));
            options.AddInterceptors(
                sp.GetRequiredService<AuditableEntityInterceptor>(),
                sp.GetRequiredService<SoftDeleteInterceptor>(),
                sp.GetRequiredService<DomainEventDispatcherInterceptor>());
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            if (isDevelopment)
            {
                options.EnableDetailedErrors();
                options.EnableSensitiveDataLogging();
            }
        });

        services.AddDbContext<AIDbContext>((sp, options) =>
        {
            options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
            options.UseSqlServer(GetConnectionString("AIConnection"), sql =>
                sql.MigrationsAssembly(MigrationsAssemblyName));
            options.AddInterceptors(
                sp.GetRequiredService<AuditableEntityInterceptor>(),
                sp.GetRequiredService<SoftDeleteInterceptor>(),
                sp.GetRequiredService<DomainEventDispatcherInterceptor>());
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            if (isDevelopment)
            {
                options.EnableDetailedErrors();
                options.EnableSensitiveDataLogging();
            }
        });

        services.AddScoped<IUnitOfWork, PersistenceUnitOfWork>();
        services.AddScoped<IApplicationUserStore, ApplicationUserStore>();
        services.AddScoped<IOtpService, OtpService>();
        services.AddScoped<IIdentityOtpProvisioningService, IdentityOtpProvisioningService>();
        services.AddScoped<IPatientIdentityTimelineService, PatientIdentityTimelineService>();
        services.AddScoped<IPatientClinicAccessService, PatientClinicAccessService>();

        services.AddScoped<IPatientFhirMapper, PatientFhirMapper>();
        services.AddScoped<IEncounterFhirMapper, EncounterFhirMapper>();
        services.AddScoped<IOrganizationFhirMapper, OrganizationFhirMapper>();
        services.AddScoped<IAppointmentFhirMapper, AppointmentFhirMapper>();
        services.AddScoped<IDeviceReadingFhirMapper, DeviceReadingFhirMapper>();
        services.AddScoped<IDeviceReadingRollupService, DeviceReadingRollupService>();

        services.AddScoped<IMasterPatientIndexService, MasterPatientIndexService>();
        services.AddScoped<IPatientMergeService, PatientMergeService>();
        services.AddScoped<IPatientUniqueConflictResolver, PatientUniqueConflictResolver>();
        services.AddScoped<IUniqueConstraintViolationDetector, SqlServerUniqueConstraintViolationDetector>();
        services.AddScoped<IRepository<Patient>>(sp => new EfRepository<Patient, ClinicalDbContext>(sp.GetRequiredService<ClinicalDbContext>()));
        services.AddScoped<IRepository<PatientExternalId>>(sp => new EfRepository<PatientExternalId, ClinicalDbContext>(sp.GetRequiredService<ClinicalDbContext>()));
        services.AddScoped<IRepository<Clinic>>(sp => new EfRepository<Clinic, ClinicalDbContext>(sp.GetRequiredService<ClinicalDbContext>()));
        services.AddScoped<IRepository<VoiceRecording>>(sp => new EfRepository<VoiceRecording, ClinicalDbContext>(sp.GetRequiredService<ClinicalDbContext>()));
        services.AddScoped<IRepository<Visit>>(sp => new EfRepository<Visit, ClinicalDbContext>(sp.GetRequiredService<ClinicalDbContext>()));
        services.AddScoped<IRepository<VitalSignRecord>>(sp => new EfRepository<VitalSignRecord, ClinicalDbContext>(sp.GetRequiredService<ClinicalDbContext>()));
        services.AddScoped<IRepository<Appointment>>(sp => new EfRepository<Appointment, ClinicalDbContext>(sp.GetRequiredService<ClinicalDbContext>()));
        services.AddScoped<IRepository<TeleSession>>(sp => new EfRepository<TeleSession, ClinicalDbContext>(sp.GetRequiredService<ClinicalDbContext>()));
        services.AddScoped<IRepository<Message>>(sp => new EfRepository<Message, ClinicalDbContext>(sp.GetRequiredService<ClinicalDbContext>()));
        services.AddScoped<IRepository<PatientFamilyMember>>(sp => new EfRepository<PatientFamilyMember, ClinicalDbContext>(sp.GetRequiredService<ClinicalDbContext>()));
        services.AddScoped<IRepository<MoodLog>>(sp => new EfRepository<MoodLog, ClinicalDbContext>(sp.GetRequiredService<ClinicalDbContext>()));
        services.AddScoped<IRepository<PatientInAppNotification>>(sp => new EfRepository<PatientInAppNotification, ClinicalDbContext>(sp.GetRequiredService<ClinicalDbContext>()));
        services.AddScoped<IRepository<PatientPushDevice>>(sp => new EfRepository<PatientPushDevice, ClinicalDbContext>(sp.GetRequiredService<ClinicalDbContext>()));
        services.AddScoped<IPatientInAppNotificationBulkWriter, PatientInAppNotificationBulkWriter>();

        services.AddScoped<IRepository<Invoice>>(sp => new EfRepository<Invoice, BillingDbContext>(sp.GetRequiredService<BillingDbContext>()));
        services.AddScoped<IRepository<PaymentMethod>>(sp => new EfRepository<PaymentMethod, BillingDbContext>(sp.GetRequiredService<BillingDbContext>()));
        services.AddScoped<IRepository<PaymentTransaction>>(sp => new EfRepository<PaymentTransaction, BillingDbContext>(sp.GetRequiredService<BillingDbContext>()));
        services.AddScoped<IRepository<PatientCarePlan>>(sp => new EfRepository<PatientCarePlan, BillingDbContext>(sp.GetRequiredService<BillingDbContext>()));
        services.AddScoped<IRepository<Device>>(sp => new EfRepository<Device, DeviceDbContext>(sp.GetRequiredService<DeviceDbContext>()));
        services.AddScoped<IRepository<DeviceAssignment>>(sp => new EfRepository<DeviceAssignment, DeviceDbContext>(sp.GetRequiredService<DeviceDbContext>()));
        services.AddScoped<IRepository<DeviceReading>>(sp => new EfRepository<DeviceReading, DeviceDbContext>(sp.GetRequiredService<DeviceDbContext>()));
        services.AddScoped<IRepository<DeviceEmergencyEvent>>(sp => new EfRepository<DeviceEmergencyEvent, DeviceDbContext>(sp.GetRequiredService<DeviceDbContext>()));
        services.AddScoped<IRepository<InsurancePlan>>(sp => new EfRepository<InsurancePlan, InsuranceDbContext>(sp.GetRequiredService<InsuranceDbContext>()));
        services.AddScoped<IRepository<InsuranceProfile>>(sp => new EfRepository<InsuranceProfile, InsuranceDbContext>(sp.GetRequiredService<InsuranceDbContext>()));
        services.AddScoped<IRepository<DashboardSnapshot>>(sp => new EfRepository<DashboardSnapshot, AIDbContext>(sp.GetRequiredService<AIDbContext>()));

        return services;
    }
}
