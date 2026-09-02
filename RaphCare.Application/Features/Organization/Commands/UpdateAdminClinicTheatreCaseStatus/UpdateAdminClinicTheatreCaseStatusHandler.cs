using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Clinical;
using RaphCare.Domain.Patients;

namespace RaphCare.Application.Features.Organization.Commands.UpdateAdminClinicTheatreCaseStatus;

public sealed class UpdateAdminClinicTheatreCaseStatusHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IRepository<TheatreCase> theatreCaseRepository,
    IRepository<Patient> patientRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateAdminClinicTheatreCaseStatusCommand, AdminClinicTheatreCaseDto?>
{
    public async Task<AdminClinicTheatreCaseDto?> Handle(
        UpdateAdminClinicTheatreCaseStatusCommand request,
        CancellationToken cancellationToken)
    {
        await AdminClinicAuthorization.EnsureClinicStaffAsync(
                currentUserService,
                clinicStaffMembershipService,
                request.ClinicId,
                "Only hospital staff can update a theatre case.",
                cancellationToken)
            .ConfigureAwait(false);

        var theatreCase = await theatreCaseRepository.GetByIdAsync(request.CaseId, cancellationToken)
            .ConfigureAwait(false);
        if (theatreCase is null || theatreCase.ClinicId != request.ClinicId)
            return null;

        var status = Normalize(request.Status);
        if (theatreCase.Status is "Completed" or "Cancelled")
            throw new BusinessRuleException("This theatre case is already closed.");

        if (status == "InProgress" && theatreCase.Status != "Scheduled")
            throw new BusinessRuleException("Only a scheduled case can be started.");

        if (status == "Completed" && theatreCase.Status is not ("Scheduled" or "InProgress"))
            throw new BusinessRuleException("Only an open case can be completed.");

        if (status == "Cancelled" && theatreCase.Status is not ("Scheduled" or "InProgress"))
            throw new BusinessRuleException("Only an open case can be cancelled.");

        if (status == "Scheduled")
            throw new BusinessRuleException("Cannot move a case back to Scheduled.");

        theatreCase.Status = status;
        await theatreCaseRepository.UpdateAsync(theatreCase, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var patient = await patientRepository.GetByIdAsync(theatreCase.PatientId, cancellationToken)
            .ConfigureAwait(false);
        return new AdminClinicTheatreCaseDto
        {
            Id = theatreCase.Id,
            PatientId = theatreCase.PatientId,
            PatientName = patient is null
                ? "Patient"
                : $"{patient.FirstName} {patient.LastName}".Trim(),
            ScheduledStart = theatreCase.ScheduledStart,
            ScheduledEnd = theatreCase.ScheduledEnd,
            ProcedureName = theatreCase.ProcedureName,
            TheatreName = theatreCase.TheatreName,
            SurgeonName = theatreCase.SurgeonName,
            Status = theatreCase.Status,
            Notes = theatreCase.Notes
        };
    }

    private static string Normalize(string status) => status.Trim() switch
    {
        var value when value.Equals("InProgress", StringComparison.OrdinalIgnoreCase) => "InProgress",
        var value when value.Equals("Completed", StringComparison.OrdinalIgnoreCase) => "Completed",
        var value when value.Equals("Cancelled", StringComparison.OrdinalIgnoreCase) => "Cancelled",
        var value when value.Equals("Scheduled", StringComparison.OrdinalIgnoreCase) => "Scheduled",
        _ => status.Trim()
    };
}
