using MediatR;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Devices;
using RaphCare.Domain.Patients;

namespace RaphCare.Application.Features.Organization.Queries.GetAdminClinicDevices;

public sealed class GetAdminClinicDevicesHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IRepository<Device> deviceRepository,
    IRepository<DeviceAssignment> assignmentRepository,
    IRepository<Patient> patientRepository)
    : IRequestHandler<GetAdminClinicDevicesQuery, IReadOnlyList<AdminClinicDeviceListItemDto>?>
{
    public async Task<IReadOnlyList<AdminClinicDeviceListItemDto>?> Handle(
        GetAdminClinicDevicesQuery request,
        CancellationToken cancellationToken)
    {
        if (!await AdminClinicAuthorization.HasClinicAccessAsync(
                currentUserService,
                clinicStaffMembershipService,
                request.ClinicId,
                cancellationToken)
            .ConfigureAwait(false))
            return null;

        var devicesPage = await deviceRepository.SearchAsync(
            q => q.Where(d => d.ClinicId == request.ClinicId).OrderBy(d => d.SerialNumber),
            1,
            200,
            applyDefaultIdOrdering: false,
            cancellationToken).ConfigureAwait(false);

        var devices = devicesPage.Items;
        if (devices.Count == 0)
            return Array.Empty<AdminClinicDeviceListItemDto>();

        var deviceIds = devices.Select(d => d.Id).ToList();
        var assignmentsPage = await assignmentRepository.SearchAsync(
            q => q.Where(a => deviceIds.Contains(a.DeviceId) && a.IsActive),
            1,
            500,
            applyDefaultIdOrdering: false,
            cancellationToken).ConfigureAwait(false);

        var assignmentByDevice = assignmentsPage.Items
            .GroupBy(a => a.DeviceId)
            .ToDictionary(g => g.Key, g => g.First());

        var patientIds = assignmentByDevice.Values.Select(a => a.PatientId).Distinct().ToList();
        var patients = new Dictionary<Guid, Patient>();
        foreach (var patientId in patientIds)
        {
            var patient = await patientRepository.GetByIdAsync(patientId, cancellationToken).ConfigureAwait(false);
            if (patient is not null)
                patients[patientId] = patient;
        }

        return devices.Select(d =>
        {
            assignmentByDevice.TryGetValue(d.Id, out var assignment);
            string? patientName = null;
            Guid? patientId = null;
            if (assignment is not null)
            {
                patientId = assignment.PatientId;
                if (patients.TryGetValue(assignment.PatientId, out var p))
                    patientName = $"{p.FirstName} {p.LastName}".Trim();
            }

            return new AdminClinicDeviceListItemDto
            {
                Id = d.Id,
                SerialNumber = d.SerialNumber,
                Model = d.Model,
                IsActive = d.IsActive,
                IsAssigned = d.IsAssigned,
                Status = d.Status,
                AssignedPatientId = patientId,
                AssignedPatientName = patientName
            };
        }).ToList();
    }
}
