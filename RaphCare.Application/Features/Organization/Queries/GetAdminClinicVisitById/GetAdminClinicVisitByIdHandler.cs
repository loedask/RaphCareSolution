using MediatR;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Clinical;
using RaphCare.Domain.Organization;
using RaphCare.Domain.Patients;

namespace RaphCare.Application.Features.Organization.Queries.GetAdminClinicVisitById;

public sealed class GetAdminClinicVisitByIdHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IRepository<Visit> visitRepository,
    IRepository<VitalSignRecord> vitalRepository,
    IRepository<Patient> patientRepository,
    IRepository<Provider> providerRepository,
    IProfessionalUserLookupService professionalUserLookupService)
    : IRequestHandler<GetAdminClinicVisitByIdQuery, AdminClinicVisitDetailDto?>
{
    public async Task<AdminClinicVisitDetailDto?> Handle(
        GetAdminClinicVisitByIdQuery request,
        CancellationToken cancellationToken)
    {
        if (!await AdminClinicAuthorization.HasClinicAccessAsync(
                currentUserService,
                clinicStaffMembershipService,
                request.ClinicId,
                cancellationToken)
            .ConfigureAwait(false))
            return null;

        var visit = await visitRepository.GetByIdAsync(request.VisitId, cancellationToken).ConfigureAwait(false);
        if (visit is null || visit.ClinicId != request.ClinicId)
            return null;

        var vitalsPage = await vitalRepository.SearchAsync(
            q => q.Where(v => v.VisitId == visit.Id).OrderByDescending(v => v.RecordedAt),
            1,
            100,
            applyDefaultIdOrdering: false,
            cancellationToken).ConfigureAwait(false);

        var patient = await patientRepository.GetByIdAsync(visit.PatientId, cancellationToken).ConfigureAwait(false);
        var provider = await providerRepository.GetByIdAsync(visit.ProviderId, cancellationToken).ConfigureAwait(false);
        var providerName = "Provider";
        if (provider is not null)
        {
            var users = await professionalUserLookupService
                .GetUsersByIdsAsync([provider.ApplicationUserId], cancellationToken)
                .ConfigureAwait(false);
            var user = users.Count > 0 ? users[0] : null;
            if (user is not null)
                providerName = string.IsNullOrWhiteSpace(user.DisplayName) ? user.Email : user.DisplayName;
        }

        return new AdminClinicVisitDetailDto
        {
            Id = visit.Id,
            ClinicId = visit.ClinicId,
            AppointmentId = visit.AppointmentId,
            PatientId = visit.PatientId,
            PatientName = patient is null ? "Patient" : $"{patient.FirstName} {patient.LastName}".Trim(),
            ProviderId = visit.ProviderId,
            ProviderName = providerName ?? "Provider",
            VisitStart = visit.VisitStart,
            VisitEnd = visit.VisitEnd,
            VisitType = visit.VisitType,
            Status = visit.Status,
            Summary = visit.Summary,
            Vitals = vitalsPage.Items.Select(v => new AdminClinicVisitVitalDto
            {
                Id = v.Id,
                Type = v.Type,
                Value = v.Value,
                Unit = v.Unit,
                RecordedAt = v.RecordedAt
            }).ToList()
        };
    }
}
