using MediatR;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Clinical;

namespace RaphCare.Application.Features.Organization.Queries.GetAdminClinicAdmissionObservations;

public sealed class GetAdminClinicAdmissionObservationsHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IRepository<InpatientAdmission> admissionRepository,
    IRepository<InpatientObservation> observationRepository)
    : IRequestHandler<GetAdminClinicAdmissionObservationsQuery, IReadOnlyList<AdminClinicAdmissionObservationDto>?>
{
    public async Task<IReadOnlyList<AdminClinicAdmissionObservationDto>?> Handle(
        GetAdminClinicAdmissionObservationsQuery request,
        CancellationToken cancellationToken)
    {
        if (!await AdminClinicAuthorization.HasClinicAccessAsync(
                currentUserService, clinicStaffMembershipService, request.ClinicId, cancellationToken)
            .ConfigureAwait(false))
            return null;

        var admission = await admissionRepository.GetByIdAsync(request.AdmissionId, cancellationToken).ConfigureAwait(false);
        if (admission is null || admission.ClinicId != request.ClinicId)
            return null;

        var page = await observationRepository.SearchAsync(
            q => q.Where(o => o.AdmissionId == admission.Id).OrderByDescending(o => o.RecordedAt),
            1,
            100,
            applyDefaultIdOrdering: false,
            cancellationToken).ConfigureAwait(false);

        return page.Items.Select(o => new AdminClinicAdmissionObservationDto
        {
            Id = o.Id,
            AdmissionId = o.AdmissionId,
            RecordedAt = o.RecordedAt,
            Note = o.Note,
            HeartRate = o.HeartRate,
            TemperatureCelsius = o.TemperatureCelsius,
            OxygenSaturation = o.OxygenSaturation,
            SystolicBp = o.SystolicBp,
            DiastolicBp = o.DiastolicBp
        }).ToList();
    }
}
