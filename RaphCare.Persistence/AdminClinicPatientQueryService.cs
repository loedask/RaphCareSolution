using Microsoft.EntityFrameworkCore;
using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Clinical;
using RaphCare.Domain.Devices;
using RaphCare.Domain.Patients;

namespace RaphCare.Persistence;

public sealed class AdminClinicPatientQueryService(
    ClinicalDbContext clinicalDbContext,
    DeviceDbContext deviceDbContext,
    BillingDbContext billingDbContext,
    InsuranceDbContext insuranceDbContext,
    IProfessionalUserLookupService professionalUserLookupService,
    IDeviceReadingRollupService deviceReadingRollupService,
    IDateTimeProvider clock)
    : IAdminClinicPatientQueryService
{
    public async Task<PagedResult<AdminClinicPatientListItemDto>> GetPatientsAsync(
        Guid clinicId,
        int pageNumber,
        int pageSize,
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        var query = clinicalDbContext.PatientClinicAccesses
            .AsNoTracking()
            .Where(a => a.ClinicId == clinicId && a.IsActive)
            .Join(
                clinicalDbContext.Patients.AsNoTracking().Where(p => !p.IsDeleted),
                access => access.PatientId,
                patient => patient.Id,
                (access, patient) => new { access, patient });

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x =>
                x.patient.FirstName.Contains(term)
                || x.patient.LastName.Contains(term)
                || (x.patient.Email != null && x.patient.Email.Contains(term))
                || (x.patient.PhoneNumber != null && x.patient.PhoneNumber.Contains(term)));
        }

        query = query
            .OrderBy(x => x.patient.LastName)
            .ThenBy(x => x.patient.FirstName);

        var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);
        var skip = (pageNumber - 1) * pageSize;

        var items = await query
            .Skip(skip)
            .Take(pageSize)
            .Select(x => new AdminClinicPatientListItemDto
            {
                PatientId = x.patient.Id,
                FirstName = x.patient.FirstName,
                LastName = x.patient.LastName,
                DateOfBirth = x.patient.DateOfBirth,
                AccessType = x.access.AccessType.ToString(),
                GrantedAt = x.access.GrantedAt
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new PagedResult<AdminClinicPatientListItemDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<AdminClinicPatientDetailDto?> GetPatientDetailAsync(
        Guid clinicId,
        Guid patientId,
        CancellationToken cancellationToken = default)
    {
        var row = await clinicalDbContext.PatientClinicAccesses
            .AsNoTracking()
            .Where(a => a.ClinicId == clinicId && a.PatientId == patientId && a.IsActive)
            .Join(
                clinicalDbContext.Patients.AsNoTracking().Where(p => !p.IsDeleted),
                access => access.PatientId,
                patient => patient.Id,
                (access, patient) => new { access, patient })
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (row is null)
            return null;

        var recentVisits = await clinicalDbContext.Visits
            .AsNoTracking()
            .Where(v => v.ClinicId == clinicId && v.PatientId == patientId)
            .OrderByDescending(v => v.VisitStart)
            .Take(10)
            .Select(v => new AdminClinicPatientVisitDto
            {
                Id = v.Id,
                VisitStart = v.VisitStart,
                VisitEnd = v.VisitEnd,
                VisitType = v.VisitType,
                Status = v.Status,
                Summary = v.Summary
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var appointments = await clinicalDbContext.Appointments
            .AsNoTracking()
            .Where(a => a.ClinicId == clinicId && a.PatientId == patientId && !a.IsCancelled)
            .OrderByDescending(a => a.ScheduledStart)
            .Take(10)
            .Select(a => new
            {
                a.Id,
                a.ProviderId,
                a.ScheduledStart,
                a.ScheduledEnd,
                a.Type,
                a.Status,
                a.Reason
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var providerIds = appointments.Select(a => a.ProviderId).Distinct().ToList();
        var providers = providerIds.Count == 0
            ? []
            : await clinicalDbContext.Set<Domain.Organization.Provider>()
                .AsNoTracking()
                .Where(p => providerIds.Contains(p.Id))
                .Select(p => new { p.Id, p.ApplicationUserId })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

        var userIds = providers.Select(p => p.ApplicationUserId).Distinct().ToList();
        var users = userIds.Count == 0
            ? []
            : await professionalUserLookupService.GetUsersByIdsAsync(userIds, cancellationToken).ConfigureAwait(false);
        var usersById = users.ToDictionary(u => u.Id);
        var providerNames = providers.ToDictionary(
            p => p.Id,
            p => usersById.TryGetValue(p.ApplicationUserId, out var u)
                ? (string.IsNullOrWhiteSpace(u.DisplayName) ? u.Email : u.DisplayName) ?? "Provider"
                : "Provider");

        var appointmentDtos = appointments.Select(a =>
        {
            providerNames.TryGetValue(a.ProviderId, out var name);
            return new AdminClinicPatientAppointmentDto
            {
                Id = a.Id,
                ScheduledStart = a.ScheduledStart,
                ScheduledEnd = a.ScheduledEnd,
                Type = a.Type,
                Status = a.Status,
                Reason = a.Reason,
                ProviderName = name ?? "Provider"
            };
        }).ToList();

        var recentVitals = await clinicalDbContext.Set<VitalSignRecord>()
            .AsNoTracking()
            .Join(
                clinicalDbContext.Visits.AsNoTracking()
                    .Where(v => v.ClinicId == clinicId && v.PatientId == patientId),
                vital => vital.VisitId,
                visit => visit.Id,
                (vital, visit) => new { vital, visit })
            .OrderByDescending(x => x.vital.RecordedAt)
            .Take(15)
            .Select(x => new AdminClinicPatientVitalDto
            {
                Type = x.vital.Type,
                Value = x.vital.Value,
                Unit = x.vital.Unit,
                RecordedAt = x.vital.RecordedAt,
                VisitType = x.visit.VisitType
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var recentDeviceRows = await deviceDbContext.Set<DeviceReading>()
            .AsNoTracking()
            .Join(
                deviceDbContext.Set<Device>().AsNoTracking(),
                r => r.DeviceId,
                d => d.Id,
                (r, d) => new { r, d })
            .Where(x => x.r.PatientId == patientId && x.d.ClinicId == clinicId)
            .OrderByDescending(x => x.r.RecordedAt)
            .Take(10)
            .Select(x => x.r)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var recentDeviceReadings = recentDeviceRows.Select(MapDeviceReading).ToList();

        var rollupTo = clock.UtcNow;
        var rollupFrom = rollupTo.AddDays(-7);
        var rollups = await deviceReadingRollupService
            .GetDailyRollupsAsync(patientId, clinicId, rollupFrom, rollupTo, cancellationToken)
            .ConfigureAwait(false);
        var rollupDtos = rollups.Select(r => new AdminClinicPatientDeviceRollupDto
        {
            Date = r.Date,
            HeartRateSampleCount = r.HeartRateSampleCount,
            AvgHeartRateBpm = r.AvgHeartRateBpm,
            MinHeartRateBpm = r.MinHeartRateBpm,
            MaxHeartRateBpm = r.MaxHeartRateBpm,
            SpO2SampleCount = r.SpO2SampleCount,
            AvgSpO2Percent = r.AvgSpO2Percent,
            MinSpO2Percent = r.MinSpO2Percent,
            MaxSpO2Percent = r.MaxSpO2Percent
        }).ToList();

        var medicalSummary = await LoadMedicalSummaryAsync(patientId, cancellationToken).ConfigureAwait(false);
        var emergencyContacts = await LoadEmergencyContactsAsync(patientId, cancellationToken).ConfigureAwait(false);
        var insuranceProfiles = await LoadInsuranceAsync(patientId, cancellationToken).ConfigureAwait(false);
        var invoices = await LoadInvoicesAsync(clinicId, patientId, cancellationToken).ConfigureAwait(false);
        var moodLogs = await LoadMoodLogsAsync(patientId, cancellationToken).ConfigureAwait(false);
        var carePlans = await LoadCarePlansAsync(clinicId, patientId, cancellationToken).ConfigureAwait(false);

        var chartVisitRows = await clinicalDbContext.Visits
            .AsNoTracking()
            .Where(v => v.ClinicId == clinicId && v.PatientId == patientId)
            .OrderByDescending(v => v.VisitStart)
            .Take(25)
            .Select(v => new { v.Id, v.VisitStart })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var visitStarts = chartVisitRows.ToDictionary(v => v.Id, v => v.VisitStart);
        var clinicalDocs = await LoadClinicalDocumentationAsync(visitStarts, cancellationToken).ConfigureAwait(false);

        return new AdminClinicPatientDetailDto
        {
            PatientId = row.patient.Id,
            FirstName = row.patient.FirstName,
            LastName = row.patient.LastName,
            DateOfBirth = row.patient.DateOfBirth,
            Gender = row.patient.Gender,
            Email = row.patient.Email,
            PhoneNumber = row.patient.PhoneNumber,
            NationalHealthId = row.patient.NationalHealthId,
            AccessType = row.access.AccessType.ToString(),
            GrantedAt = row.access.GrantedAt,
            GrantedByRule = row.access.GrantedByRule,
            Notes = row.access.Notes,
            MedicalSummary = medicalSummary,
            EmergencyContacts = emergencyContacts,
            InsuranceProfiles = insuranceProfiles,
            Invoices = invoices,
            MoodLogs = moodLogs,
            CarePlans = carePlans,
            Diagnoses = clinicalDocs.Diagnoses,
            Prescriptions = clinicalDocs.Prescriptions,
            ClinicalNotes = clinicalDocs.ClinicalNotes,
            SoapNotes = clinicalDocs.SoapNotes,
            LabResults = clinicalDocs.LabResults,
            RecentVisits = recentVisits,
            Appointments = appointmentDtos,
            RecentVitals = recentVitals,
            RecentDeviceReadings = recentDeviceReadings,
            DeviceDailyRollups = rollupDtos
        };
    }

    public async Task<AdminClinicVisitClinicalDocumentationDto> GetVisitClinicalDocumentationAsync(
        Guid visitId,
        DateTime visitStart,
        CancellationToken cancellationToken = default)
    {
        var visitStarts = new Dictionary<Guid, DateTime> { [visitId] = visitStart };
        return await LoadClinicalDocumentationAsync(visitStarts, cancellationToken).ConfigureAwait(false);
    }

    private async Task<AdminClinicPatientMedicalSummaryDto> LoadMedicalSummaryAsync(
        Guid patientId,
        CancellationToken cancellationToken)
    {
        var profile = await clinicalDbContext.Set<PatientProfile>()
            .AsNoTracking()
            .Where(p => p.PatientId == patientId)
            .Select(p => new
            {
                p.BloodType,
                p.SelfReportedAllergies,
                p.SelfReportedChronicConditions,
                p.SelfReportedMedications,
                p.PrimaryCareProviderName
            })
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        var allergies = await clinicalDbContext.Set<Allergy>()
            .AsNoTracking()
            .Where(a => a.PatientId == patientId)
            .OrderByDescending(a => a.RecordedAt)
            .Take(20)
            .Select(a => new AdminClinicPatientAllergyDto
            {
                Substance = a.Substance,
                Reaction = a.Reaction,
                Severity = a.Severity,
                RecordedAt = a.RecordedAt
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var medications = await clinicalDbContext.Set<Medication>()
            .AsNoTracking()
            .Where(m => m.PatientId == patientId)
            .OrderByDescending(m => m.StartDate)
            .Take(20)
            .Select(m => new AdminClinicPatientMedicationDto
            {
                MedicationName = m.MedicationName,
                Dosage = m.Dosage,
                Frequency = m.Frequency,
                StartDate = m.StartDate,
                EndDate = m.EndDate
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new AdminClinicPatientMedicalSummaryDto
        {
            BloodType = profile?.BloodType,
            Allergies = profile?.SelfReportedAllergies,
            ChronicConditions = profile?.SelfReportedChronicConditions,
            Medications = profile?.SelfReportedMedications,
            PrimaryDoctor = profile?.PrimaryCareProviderName,
            RecordedAllergies = allergies,
            RecordedMedications = medications
        };
    }

    private async Task<IReadOnlyList<AdminClinicPatientEmergencyContactDto>> LoadEmergencyContactsAsync(
        Guid patientId,
        CancellationToken cancellationToken)
    {
        return await clinicalDbContext.Set<EmergencyContact>()
            .AsNoTracking()
            .Where(c => c.PatientId == patientId)
            .OrderBy(c => c.Name)
            .Select(c => new AdminClinicPatientEmergencyContactDto
            {
                Name = c.Name,
                Relationship = c.Relationship,
                PhoneNumber = c.PhoneNumber,
                Email = c.Email
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task<IReadOnlyList<AdminClinicPatientInsuranceDto>> LoadInsuranceAsync(
        Guid patientId,
        CancellationToken cancellationToken)
    {
        var profiles = await insuranceDbContext.InsuranceProfiles
            .AsNoTracking()
            .Where(p => p.PatientId == patientId)
            .OrderByDescending(p => p.IsActive)
            .ThenByDescending(p => p.StartDate)
            .Select(p => new
            {
                p.InsurancePlanId,
                p.MembershipNumber,
                p.StartDate,
                p.EndDate,
                p.IsActive
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (profiles.Count == 0)
            return Array.Empty<AdminClinicPatientInsuranceDto>();

        var planIds = profiles.Select(p => p.InsurancePlanId).Distinct().ToList();
        var plans = await insuranceDbContext.InsurancePlans
            .AsNoTracking()
            .Where(p => planIds.Contains(p.Id))
            .Select(p => new { p.Id, p.Name })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var planNames = plans.ToDictionary(p => p.Id, p => p.Name);

        return profiles.Select(p =>
        {
            planNames.TryGetValue(p.InsurancePlanId, out var name);
            return new AdminClinicPatientInsuranceDto
            {
                PlanName = string.IsNullOrWhiteSpace(name) ? "Insurance plan" : name,
                MembershipNumber = p.MembershipNumber,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                IsActive = p.IsActive
            };
        }).ToList();
    }

    private async Task<IReadOnlyList<AdminClinicPatientInvoiceDto>> LoadInvoicesAsync(
        Guid clinicId,
        Guid patientId,
        CancellationToken cancellationToken)
    {
        return await billingDbContext.Invoices
            .AsNoTracking()
            .Where(i => i.ClinicId == clinicId && i.PatientId == patientId)
            .OrderByDescending(i => i.DueDate)
            .Take(20)
            .Select(i => new AdminClinicPatientInvoiceDto
            {
                Id = i.Id,
                Amount = i.Amount,
                Currency = i.Currency,
                Status = i.Status,
                DueDate = i.DueDate,
                PaidAt = i.PaidAt,
                VisitId = i.VisitId
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task<IReadOnlyList<AdminClinicPatientMoodLogDto>> LoadMoodLogsAsync(
        Guid patientId,
        CancellationToken cancellationToken)
    {
        return await clinicalDbContext.MoodLogs
            .AsNoTracking()
            .Where(m => m.PatientId == patientId)
            .OrderByDescending(m => m.LoggedAt)
            .Take(20)
            .Select(m => new AdminClinicPatientMoodLogDto
            {
                LoggedAt = m.LoggedAt,
                MoodScore = m.MoodScore,
                Notes = m.Notes,
                IsFlagged = m.IsFlagged
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task<IReadOnlyList<AdminClinicPatientCarePlanDto>> LoadCarePlansAsync(
        Guid clinicId,
        Guid patientId,
        CancellationToken cancellationToken)
    {
        return await clinicalDbContext.CarePlans
            .AsNoTracking()
            .Where(p => p.ClinicId == clinicId && p.PatientId == patientId)
            .OrderByDescending(p => p.StartDate)
            .Take(10)
            .Select(p => new AdminClinicPatientCarePlanDto
            {
                Title = p.Title,
                Description = p.Description,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                Status = p.Status
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task<AdminClinicVisitClinicalDocumentationDto> LoadClinicalDocumentationAsync(
        IReadOnlyDictionary<Guid, DateTime> visitStarts,
        CancellationToken cancellationToken)
    {
        if (visitStarts.Count == 0)
            return new AdminClinicVisitClinicalDocumentationDto();

        var visitIds = visitStarts.Keys.ToList();

        var diagnoses = await clinicalDbContext.Set<Diagnosis>()
            .AsNoTracking()
            .Where(d => visitIds.Contains(d.VisitId))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var notes = await clinicalDbContext.Set<ClinicalNote>()
            .AsNoTracking()
            .Where(n => visitIds.Contains(n.VisitId))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var soapNotes = await clinicalDbContext.Set<SOAPNote>()
            .AsNoTracking()
            .Where(s => visitIds.Contains(s.VisitId))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var prescriptions = await clinicalDbContext.Set<Prescription>()
            .AsNoTracking()
            .Where(p => visitIds.Contains(p.VisitId))
            .Include(p => p.PrescriptionItems)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var labRequests = await clinicalDbContext.Set<LabRequest>()
            .AsNoTracking()
            .Where(l => visitIds.Contains(l.VisitId))
            .Include(l => l.LabResults)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        DateTime Start(Guid visitId) => visitStarts.TryGetValue(visitId, out var start) ? start : default;

        return new AdminClinicVisitClinicalDocumentationDto
        {
            Diagnoses = diagnoses
                .OrderByDescending(d => Start(d.VisitId))
                .Select(d => new AdminClinicVisitDiagnosisDto
                {
                    VisitId = d.VisitId,
                    VisitStart = Start(d.VisitId),
                    Code = d.Code,
                    Description = d.Description,
                    Severity = d.Severity
                })
                .ToList(),
            Prescriptions = prescriptions
                .OrderByDescending(p => p.IssuedAt)
                .Select(p => new AdminClinicVisitPrescriptionDto
                {
                    Id = p.Id,
                    VisitId = p.VisitId,
                    VisitStart = Start(p.VisitId),
                    IssuedAt = p.IssuedAt,
                    Status = p.Status,
                    PickupCode = p.PickupCode,
                    DispensedAt = p.DispensedAt,
                    Notes = p.Notes,
                    Items = p.PrescriptionItems
                        .Select(i => new AdminClinicVisitPrescriptionItemDto
                        {
                            MedicationName = i.MedicationName,
                            Dosage = i.Dosage,
                            Frequency = i.Frequency,
                            DurationDays = i.DurationDays
                        })
                        .ToList()
                })
                .ToList(),
            ClinicalNotes = notes
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new AdminClinicVisitNoteDto
                {
                    VisitId = n.VisitId,
                    VisitStart = Start(n.VisitId),
                    Notes = n.Notes,
                    Category = n.Category
                })
                .ToList(),
            SoapNotes = soapNotes
                .OrderByDescending(s => Start(s.VisitId))
                .Select(s => new AdminClinicVisitSoapNoteDto
                {
                    VisitId = s.VisitId,
                    VisitStart = Start(s.VisitId),
                    Subjective = s.Subjective,
                    Objective = s.Objective,
                    Assessment = s.Assessment,
                    Plan = s.Plan
                })
                .ToList(),
            LabResults = labRequests
                .OrderByDescending(l => l.RequestedAt)
                .Select(l =>
                {
                    var result = l.LabResults.OrderByDescending(r => r.ReportedAt).FirstOrDefault();
                    return new AdminClinicVisitLabResultDto
                    {
                        Id = l.Id,
                        VisitId = l.VisitId,
                        VisitStart = Start(l.VisitId),
                        TestName = l.TestName,
                        Status = l.Status,
                        PickupCode = l.PickupCode,
                        RequestedAt = l.RequestedAt,
                        ResultValue = result?.ResultValue,
                        Unit = result?.Unit,
                        ReferenceRange = result?.ReferenceRange,
                        ReportedAt = result?.ReportedAt
                    };
                })
                .ToList()
        };
    }

    private static AdminClinicPatientDeviceReadingDto MapDeviceReading(DeviceReading r)
    {
        var dto = new AdminClinicPatientDeviceReadingDto
        {
            Kind = r.ReadingType,
            ReadingType = r.ReadingType,
            PrimaryValue = r.PrimaryValue,
            Unit = r.Unit,
            RecordedAt = r.RecordedAt
        };

        switch (r)
        {
            case HeartRateReading hr:
                dto.Kind = nameof(HeartRateReading);
                dto.HeartRateBpm = (int)hr.HeartRate;
                break;
            case PulseOximeterReading po:
                dto.Kind = nameof(PulseOximeterReading);
                dto.SpO2Percent = po.SpO2;
                break;
        }

        return dto;
    }
}
