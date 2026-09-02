using System.Linq;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.PatientHealthRecords.DTOs;
using RaphCare.Domain.Clinical;
using RaphCare.Domain.Organization;

namespace RaphCare.Application.Features.PatientHealthRecords.Queries.GetMyHealthRecordById;

public class GetMyHealthRecordByIdHandler : IRequestHandler<GetMyHealthRecordByIdQuery, PatientHealthRecordDetailDto>
{
    private readonly IRepository<Visit> _visitRepository;
    private readonly IRepository<InpatientAdmission> _admissionRepository;
    private readonly IRepository<VitalSignRecord> _vitalRepository;
    private readonly IRepository<Prescription> _prescriptionRepository;
    private readonly IRepository<LabRequest> _labRequestRepository;
    private readonly IRepository<Clinic> _clinicRepository;
    private readonly ICurrentUserService _currentUser;

    public GetMyHealthRecordByIdHandler(
        IRepository<Visit> visitRepository,
        IRepository<InpatientAdmission> admissionRepository,
        IRepository<VitalSignRecord> vitalRepository,
        IRepository<Prescription> prescriptionRepository,
        IRepository<LabRequest> labRequestRepository,
        IRepository<Clinic> clinicRepository,
        ICurrentUserService currentUser)
    {
        _visitRepository = visitRepository;
        _admissionRepository = admissionRepository;
        _vitalRepository = vitalRepository;
        _prescriptionRepository = prescriptionRepository;
        _labRequestRepository = labRequestRepository;
        _clinicRepository = clinicRepository;
        _currentUser = currentUser;
    }

    public async Task<PatientHealthRecordDetailDto> Handle(GetMyHealthRecordByIdQuery request, CancellationToken cancellationToken)
    {
        var patientId = _currentUser.CurrentPatientId
            ?? throw new ForbiddenAccessException("A patient profile is required to view health records.");

        var visit = await _visitRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
        if (visit is not null && visit.PatientId == patientId)
            return await MapVisitAsync(visit, cancellationToken).ConfigureAwait(false);

        var admission = await _admissionRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
        if (admission is not null && admission.PatientId == patientId && admission.Status == "Discharged")
        {
            return new PatientHealthRecordDetailDto
            {
                Id = admission.Id,
                VisitStart = admission.AdmittedAt,
                VisitEnd = admission.DischargedAt,
                VisitType = "Stay",
                Status = admission.Status,
                Summary = admission.DischargeSummary,
                RecordKind = "Discharge"
            };
        }

        throw new NotFoundException(nameof(Visit), request.Id);
    }

    private async Task<PatientHealthRecordDetailDto> MapVisitAsync(Visit visit, CancellationToken cancellationToken)
    {
        var vitalsPage = await _vitalRepository.SearchAsync(
            q => q.Where(v => v.VisitId == visit.Id).OrderBy(v => v.RecordedAt),
            1,
            500,
            applyDefaultIdOrdering: false,
            cancellationToken).ConfigureAwait(false);

        var clinic = await _clinicRepository.GetByIdAsync(visit.ClinicId, cancellationToken).ConfigureAwait(false);
        var clinicName = clinic?.Name ?? string.Empty;

        var prescriptions = await _prescriptionRepository.SearchAsync(
            q => q.Where(p => p.VisitId == visit.Id).Include(p => p.PrescriptionItems).OrderByDescending(p => p.IssuedAt),
            1,
            50,
            applyDefaultIdOrdering: false,
            cancellationToken).ConfigureAwait(false);

        var labs = await _labRequestRepository.SearchAsync(
            q => q.Where(l => l.VisitId == visit.Id).Include(l => l.LabResults).OrderByDescending(l => l.RequestedAt),
            1,
            50,
            applyDefaultIdOrdering: false,
            cancellationToken).ConfigureAwait(false);

        var vitalDtos = vitalsPage.Items.Select(v => new PatientVitalSignDto
        {
            Type = v.Type,
            Value = v.Value,
            Unit = v.Unit,
            RecordedAt = v.RecordedAt
        }).ToList();

        return new PatientHealthRecordDetailDto
        {
            Id = visit.Id,
            VisitStart = visit.VisitStart,
            VisitEnd = visit.VisitEnd,
            VisitType = visit.VisitType,
            Status = visit.Status,
            Summary = visit.Summary,
            RecordKind = "Visit",
            VitalSigns = vitalDtos,
            Prescriptions = prescriptions.Items.Select(p => new PatientCollectionPrescriptionDto
            {
                Id = p.Id,
                VisitId = p.VisitId,
                ClinicId = visit.ClinicId,
                ClinicName = clinicName,
                PickupCode = p.PickupCode,
                IssuedAt = p.IssuedAt,
                Status = p.Status,
                CalledAt = p.CalledAt,
                Notes = p.Notes,
                Items = p.PrescriptionItems.Select(i => new PatientCollectionPrescriptionItemDto
                {
                    MedicationName = i.MedicationName,
                    Dosage = i.Dosage,
                    Frequency = i.Frequency,
                    DurationDays = i.DurationDays
                }).ToList()
            }).ToList(),
            LabOrders = labs.Items.Select(l =>
            {
                var result = l.LabResults.OrderByDescending(r => r.ReportedAt).FirstOrDefault();
                return new PatientCollectionLabOrderDto
                {
                    Id = l.Id,
                    VisitId = l.VisitId,
                    ClinicId = visit.ClinicId,
                    ClinicName = clinicName,
                    PickupCode = l.PickupCode,
                    TestName = l.TestName,
                    Status = l.Status,
                    RequestedAt = l.RequestedAt,
                    CalledAt = l.CalledAt,
                    ResultValue = result?.ResultValue,
                    Unit = result?.Unit,
                    ReferenceRange = result?.ReferenceRange,
                    ReportedAt = result?.ReportedAt
                };
            }).ToList()
        };
    }
}
