using MediatR;
using Microsoft.EntityFrameworkCore;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.PatientHealthRecords.DTOs;
using RaphCare.Domain.Clinical;
using RaphCare.Domain.Organization;

namespace RaphCare.Application.Features.PatientHealthRecords.Queries.GetMyCollectionOrders;

public sealed class GetMyCollectionOrdersHandler(
    ICurrentUserService currentUser,
    IRepository<Visit> visitRepository,
    IRepository<Prescription> prescriptionRepository,
    IRepository<LabRequest> labRequestRepository,
    IRepository<Clinic> clinicRepository)
    : IRequestHandler<GetMyCollectionOrdersQuery, PatientCollectionOrdersDto>
{
    public async Task<PatientCollectionOrdersDto> Handle(
        GetMyCollectionOrdersQuery request,
        CancellationToken cancellationToken)
    {
        var patientId = currentUser.CurrentPatientId
            ?? throw new ForbiddenAccessException("A patient profile is required to view collection orders.");

        var visitsPage = await visitRepository.SearchAsync(
            q => q.Where(v => v.PatientId == patientId),
            1,
            200,
            cancellationToken: cancellationToken).ConfigureAwait(false);
        var visits = visitsPage.Items;
        if (visits.Count == 0)
            return new PatientCollectionOrdersDto();

        var visitIds = visits.Select(v => v.Id).ToList();
        var visitById = visits.ToDictionary(v => v.Id);

        var clinicIds = visits.Select(v => v.ClinicId).Distinct().ToList();
        var clinicsPage = await clinicRepository.SearchAsync(
            q => q.Where(c => clinicIds.Contains(c.Id)),
            1,
            200,
            cancellationToken: cancellationToken).ConfigureAwait(false);
        var clinicNames = clinicsPage.Items.ToDictionary(c => c.Id, c => c.Name);

        var prescriptions = await prescriptionRepository.SearchAsync(
            q => q.Where(p => visitIds.Contains(p.VisitId) && p.Status == "Pending")
                .Include(p => p.PrescriptionItems)
                .OrderBy(p => p.IssuedAt),
            1,
            100,
            applyDefaultIdOrdering: false,
            cancellationToken).ConfigureAwait(false);

        var labs = await labRequestRepository.SearchAsync(
            q => q.Where(l => visitIds.Contains(l.VisitId) && l.Status == "Pending")
                .Include(l => l.LabResults)
                .OrderBy(l => l.RequestedAt),
            1,
            100,
            applyDefaultIdOrdering: false,
            cancellationToken).ConfigureAwait(false);

        Guid ClinicId(Guid visitId) =>
            visitById.TryGetValue(visitId, out var visit) ? visit.ClinicId : Guid.Empty;

        string ClinicName(Guid visitId)
        {
            if (!visitById.TryGetValue(visitId, out var visit))
                return string.Empty;
            return clinicNames.TryGetValue(visit.ClinicId, out var name) ? name : string.Empty;
        }

        return new PatientCollectionOrdersDto
        {
            Prescriptions = prescriptions.Items.Select(p => new PatientCollectionPrescriptionDto
            {
                Id = p.Id,
                VisitId = p.VisitId,
                ClinicId = ClinicId(p.VisitId),
                ClinicName = ClinicName(p.VisitId),
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
                    ClinicId = ClinicId(l.VisitId),
                    ClinicName = ClinicName(l.VisitId),
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
