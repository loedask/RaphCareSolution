using MediatR;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Clinical;
using RaphCare.Domain.Patients;

namespace RaphCare.Application.Features.Organization.Commands.CreateAdminClinicTheatreCase;

public sealed class CreateAdminClinicTheatreCaseHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IRepository<TheatreCase> theatreCaseRepository,
    IRepository<Patient> patientRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateAdminClinicTheatreCaseCommand, AdminClinicTheatreCaseDto?>
{
    public async Task<AdminClinicTheatreCaseDto?> Handle(
        CreateAdminClinicTheatreCaseCommand request,
        CancellationToken cancellationToken)
    {
        await AdminClinicAuthorization.EnsureClinicStaffAsync(
                currentUserService,
                clinicStaffMembershipService,
                request.ClinicId,
                "Only hospital staff can schedule a theatre case.",
                cancellationToken)
            .ConfigureAwait(false);

        var patient = await patientRepository.GetByIdAsync(request.PatientId, cancellationToken)
            .ConfigureAwait(false);
        if (patient is null)
            return null;

        var theatreCase = new TheatreCase
        {
            ClinicId = request.ClinicId,
            PatientId = request.PatientId,
            ScheduledStart = DateTime.SpecifyKind(request.ScheduledStart, DateTimeKind.Utc),
            ScheduledEnd = request.ScheduledEnd is null
                ? null
                : DateTime.SpecifyKind(request.ScheduledEnd.Value, DateTimeKind.Utc),
            ProcedureName = request.ProcedureName.Trim(),
            TheatreName = string.IsNullOrWhiteSpace(request.TheatreName) ? null : request.TheatreName.Trim(),
            SurgeonName = string.IsNullOrWhiteSpace(request.SurgeonName) ? null : request.SurgeonName.Trim(),
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            Status = "Scheduled",
            CreatedByApplicationUserId = currentUserService.CurrentUserId
        };

        await theatreCaseRepository.AddAsync(theatreCase, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new AdminClinicTheatreCaseDto
        {
            Id = theatreCase.Id,
            PatientId = theatreCase.PatientId,
            PatientName = $"{patient.FirstName} {patient.LastName}".Trim(),
            ScheduledStart = theatreCase.ScheduledStart,
            ScheduledEnd = theatreCase.ScheduledEnd,
            ProcedureName = theatreCase.ProcedureName,
            TheatreName = theatreCase.TheatreName,
            SurgeonName = theatreCase.SurgeonName,
            Status = theatreCase.Status,
            Notes = theatreCase.Notes
        };
    }
}
