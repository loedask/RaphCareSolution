using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
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

        void ConfigureSqlServer(SqlServerDbContextOptionsBuilder sql)
        {
            sql.MigrationsAssembly(MigrationsAssemblyName);
            // Azure SQL serverless resume after auto-pause often takes tens of seconds (error 40613).
            sql.EnableRetryOnFailure(
                maxRetryCount: 8,
                maxRetryDelay: TimeSpan.FromSeconds(15),
                errorNumbersToAdd: null);
        }

        services.AddDbContext<IdentityDbContext>((sp, options) =>
        {
            options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
            options.UseSqlServer(GetConnectionString("IdentityConnection"), ConfigureSqlServer);
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
            options.UseSqlServer(GetConnectionString("ClinicalConnection"), ConfigureSqlServer);
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
            options.UseSqlServer(GetConnectionString("DeviceConnection"), ConfigureSqlServer);
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
            options.UseSqlServer(GetConnectionString("InsuranceConnection"), ConfigureSqlServer);
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
            options.UseSqlServer(GetConnectionString("BillingConnection"), ConfigureSqlServer);
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
            options.UseSqlServer(GetConnectionString("AIConnection"), ConfigureSqlServer);
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
        services.AddScoped<IEmailPasswordAuthService, EmailPasswordAuthService>();
        services.AddScoped<IUserRoleAssignmentService, UserRoleAssignmentService>();
        services.AddScoped<IClinicStaffMembershipService, ClinicStaffMembershipService>();
        services.AddScoped<IProfessionalUserLookupService, ProfessionalUserLookupService>();
        services.AddScoped<IClinicStaffInvitationService, ClinicStaffInvitationService>();
        services.AddScoped<IClinicStaffPendingInvitationService, ClinicStaffPendingInvitationService>();
        services.AddScoped<IAdminClinicPatientQueryService, AdminClinicPatientQueryService>();
        services.AddScoped<IAdminClinicCollectionQueryService, AdminClinicCollectionQueryService>();
        services.AddScoped<IAdminClinicCasualtyQueryService, AdminClinicCasualtyQueryService>();
        services.AddScoped<IAdminClinicTheatreQueryService, AdminClinicTheatreQueryService>();
        services.AddScoped<IAdminClinicReferralQueryService, AdminClinicReferralQueryService>();
        services.AddScoped<IAdminClinicInpatientQueryService, AdminClinicInpatientQueryService>();
        services.AddScoped<IAdminClinicProviderQueryService, AdminClinicProviderQueryService>();
        services.AddScoped<IAdminClinicAppointmentQueryService, AdminClinicAppointmentQueryService>();
        services.AddScoped<IAdminClinicDashboardQueryService, AdminClinicDashboardQueryService>();
        services.AddScoped<IPatientLookupService, PatientLookupService>();
        services.AddScoped<IEmailOtpService, EmailOtpService>();
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
        services.AddScoped<IRepository<ClinicStaffMembership>>(sp => new EfRepository<ClinicStaffMembership, ClinicalDbContext>(sp.GetRequiredService<ClinicalDbContext>()));
        services.AddScoped<IRepository<ClinicStaffInvitation>>(sp => new EfRepository<ClinicStaffInvitation, ClinicalDbContext>(sp.GetRequiredService<ClinicalDbContext>()));
        services.AddScoped<IRepository<Facility>>(sp => new EfRepository<Facility, ClinicalDbContext>(sp.GetRequiredService<ClinicalDbContext>()));
        services.AddScoped<IRepository<Ward>>(sp => new EfRepository<Ward, ClinicalDbContext>(sp.GetRequiredService<ClinicalDbContext>()));
        services.AddScoped<IRepository<Room>>(sp => new EfRepository<Room, ClinicalDbContext>(sp.GetRequiredService<ClinicalDbContext>()));
        services.AddScoped<IRepository<Bed>>(sp => new EfRepository<Bed, ClinicalDbContext>(sp.GetRequiredService<ClinicalDbContext>()));
        services.AddScoped<IRepository<InpatientAdmission>>(sp => new EfRepository<InpatientAdmission, ClinicalDbContext>(sp.GetRequiredService<ClinicalDbContext>()));
        services.AddScoped<IRepository<InpatientObservation>>(sp => new EfRepository<InpatientObservation, ClinicalDbContext>(sp.GetRequiredService<ClinicalDbContext>()));
        services.AddScoped<IRepository<CasualtyTicket>>(sp => new EfRepository<CasualtyTicket, ClinicalDbContext>(sp.GetRequiredService<ClinicalDbContext>()));
        services.AddScoped<IRepository<TheatreCase>>(sp => new EfRepository<TheatreCase, ClinicalDbContext>(sp.GetRequiredService<ClinicalDbContext>()));
        services.AddScoped<IRepository<Referral>>(sp => new EfRepository<Referral, ClinicalDbContext>(sp.GetRequiredService<ClinicalDbContext>()));
        services.AddScoped<IRepository<ClinicRosterEntry>>(sp => new EfRepository<ClinicRosterEntry, ClinicalDbContext>(sp.GetRequiredService<ClinicalDbContext>()));
        services.AddScoped<IRepository<Provider>>(sp => new EfRepository<Provider, ClinicalDbContext>(sp.GetRequiredService<ClinicalDbContext>()));
        services.AddScoped<IRepository<ProviderSchedule>>(sp => new EfRepository<ProviderSchedule, ClinicalDbContext>(sp.GetRequiredService<ClinicalDbContext>()));
        services.AddScoped<IRepository<VoiceRecording>>(sp => new EfRepository<VoiceRecording, ClinicalDbContext>(sp.GetRequiredService<ClinicalDbContext>()));
        services.AddScoped<IRepository<Visit>>(sp => new EfRepository<Visit, ClinicalDbContext>(sp.GetRequiredService<ClinicalDbContext>()));
        services.AddScoped<IRepository<VitalSignRecord>>(sp => new EfRepository<VitalSignRecord, ClinicalDbContext>(sp.GetRequiredService<ClinicalDbContext>()));
        services.AddScoped<IRepository<SOAPNote>>(sp => new EfRepository<SOAPNote, ClinicalDbContext>(sp.GetRequiredService<ClinicalDbContext>()));
        services.AddScoped<IRepository<ClinicalNote>>(sp => new EfRepository<ClinicalNote, ClinicalDbContext>(sp.GetRequiredService<ClinicalDbContext>()));
        services.AddScoped<IRepository<Prescription>>(sp => new EfRepository<Prescription, ClinicalDbContext>(sp.GetRequiredService<ClinicalDbContext>()));
        services.AddScoped<IRepository<LabRequest>>(sp => new EfRepository<LabRequest, ClinicalDbContext>(sp.GetRequiredService<ClinicalDbContext>()));
        services.AddScoped<IRepository<LabResult>>(sp => new EfRepository<LabResult, ClinicalDbContext>(sp.GetRequiredService<ClinicalDbContext>()));
        services.AddScoped<IRepository<Appointment>>(sp => new EfRepository<Appointment, ClinicalDbContext>(sp.GetRequiredService<ClinicalDbContext>()));
        services.AddScoped<IRepository<TeleSession>>(sp => new EfRepository<TeleSession, ClinicalDbContext>(sp.GetRequiredService<ClinicalDbContext>()));
        services.AddScoped<IRepository<TeleSessionChat>>(sp => new EfRepository<TeleSessionChat, ClinicalDbContext>(sp.GetRequiredService<ClinicalDbContext>()));
        services.AddScoped<IRepository<Message>>(sp => new EfRepository<Message, ClinicalDbContext>(sp.GetRequiredService<ClinicalDbContext>()));
        services.AddScoped<IRepository<PatientFamilyMember>>(sp => new EfRepository<PatientFamilyMember, ClinicalDbContext>(sp.GetRequiredService<ClinicalDbContext>()));
        services.AddScoped<IRepository<EmergencyContact>>(sp => new EfRepository<EmergencyContact, ClinicalDbContext>(sp.GetRequiredService<ClinicalDbContext>()));
        services.AddScoped<IRepository<Domain.Patients.PatientProfile>>(sp => new EfRepository<Domain.Patients.PatientProfile, ClinicalDbContext>(sp.GetRequiredService<ClinicalDbContext>()));
        services.AddScoped<IRepository<PatientSupportMessage>>(sp => new EfRepository<PatientSupportMessage, ClinicalDbContext>(sp.GetRequiredService<ClinicalDbContext>()));
        services.AddScoped<IRepository<MentalHealthAssessment>>(sp => new EfRepository<MentalHealthAssessment, ClinicalDbContext>(sp.GetRequiredService<ClinicalDbContext>()));
        services.AddScoped<IRepository<TherapySession>>(sp => new EfRepository<TherapySession, ClinicalDbContext>(sp.GetRequiredService<ClinicalDbContext>()));
        services.AddScoped<IRepository<BehavioralCarePlan>>(sp => new EfRepository<BehavioralCarePlan, ClinicalDbContext>(sp.GetRequiredService<ClinicalDbContext>()));
        services.AddScoped<IRepository<MoodLog>>(sp => new EfRepository<MoodLog, ClinicalDbContext>(sp.GetRequiredService<ClinicalDbContext>()));
        services.AddScoped<IRepository<PatientInAppNotification>>(sp => new EfRepository<PatientInAppNotification, ClinicalDbContext>(sp.GetRequiredService<ClinicalDbContext>()));
        services.AddScoped<IRepository<PatientPushDevice>>(sp => new EfRepository<PatientPushDevice, ClinicalDbContext>(sp.GetRequiredService<ClinicalDbContext>()));
        services.AddScoped<IPatientPushDeviceTokenReader, PatientPushDeviceTokenReader>();
        services.AddScoped<IPatientInAppNotificationBulkWriter, PatientInAppNotificationBulkWriter>();

        services.AddScoped<IRepository<Invoice>>(sp => new EfRepository<Invoice, BillingDbContext>(sp.GetRequiredService<BillingDbContext>()));
        services.AddScoped<IRepository<InvoiceLineItem>>(sp => new EfRepository<InvoiceLineItem, BillingDbContext>(sp.GetRequiredService<BillingDbContext>()));
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
