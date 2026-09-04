using MediatR;
using RaphCare.Application.Common.Interfaces;

namespace RaphCare.Application.Features.Organization.Queries.GetAdminClinicPatientPhoto;

public sealed class GetAdminClinicPatientPhotoQuery : IRequest<PatientProfilePhotoReadResult?>
{
    public Guid ClinicId { get; init; }
    public Guid PatientId { get; init; }
}

public sealed class GetAdminClinicPatientPhotoHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IAdminClinicPatientQueryService adminClinicPatientQueryService,
    IPatientProfilePhotoStorage photoStorage)
    : IRequestHandler<GetAdminClinicPatientPhotoQuery, PatientProfilePhotoReadResult?>
{
    public async Task<PatientProfilePhotoReadResult?> Handle(
        GetAdminClinicPatientPhotoQuery request,
        CancellationToken cancellationToken)
    {
        if (!await AdminClinicAuthorization.HasClinicAccessAsync(
                currentUserService, clinicStaffMembershipService, request.ClinicId, cancellationToken)
            .ConfigureAwait(false))
            return null;

        var patient = await adminClinicPatientQueryService
            .GetPatientDetailAsync(request.ClinicId, request.PatientId, cancellationToken)
            .ConfigureAwait(false);
        if (patient is null)
            return null;

        return await photoStorage.OpenReadAsync(request.PatientId, cancellationToken).ConfigureAwait(false);
    }
}