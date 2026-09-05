using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.PatientClinics.DTOs;
using RaphCare.Domain.Organization;
using RaphCare.Domain.Patients;
using RaphCare.Domain.Patients.Enums;

namespace RaphCare.Application.Features.PatientClinics.Queries.GetMyLinkedClinics;

public sealed class GetMyLinkedClinicsHandler : IRequestHandler<GetMyLinkedClinicsQuery, IReadOnlyList<PatientLinkedClinicDto>>
{
    private readonly IPatientClinicAccessService _access;
    private readonly IRepository<Clinic> _clinics;
    private readonly IRepository<PatientClinicAccess> _accessRows;
    private readonly ICurrentUserService _currentUser;

    public GetMyLinkedClinicsHandler(
        IPatientClinicAccessService access,
        IRepository<Clinic> clinics,
        IRepository<PatientClinicAccess> accessRows,
        ICurrentUserService currentUser)
    {
        _access = access;
        _clinics = clinics;
        _accessRows = accessRows;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<PatientLinkedClinicDto>> Handle(
        GetMyLinkedClinicsQuery request,
        CancellationToken cancellationToken)
    {
        var patientId = _currentUser.CurrentPatientId
            ?? throw new ForbiddenAccessException("A patient profile is required.");

        var clinicIds = await _access.GetAccessibleClinicIdsAsync(patientId, cancellationToken).ConfigureAwait(false);
        if (clinicIds.Length == 0)
            return [];

        var idSet = clinicIds.ToHashSet();
        var clinicsPaged = await _clinics.SearchAsync(
            q => q
                .Where(c => idSet.Contains(c.Id) && c.IsActive && !c.IsDeleted)
                .OrderBy(c => c.Name),
            1,
            Math.Max(clinicIds.Length, 1),
            applyDefaultIdOrdering: false,
            cancellationToken).ConfigureAwait(false);

        var accessPaged = await _accessRows.SearchAsync(
            q => q.Where(a => a.PatientId == patientId && a.IsActive && idSet.Contains(a.ClinicId)),
            1,
            Math.Max(clinicIds.Length, 1),
            applyDefaultIdOrdering: false,
            cancellationToken).ConfigureAwait(false);

        var accessByClinic = accessPaged.Items
            .GroupBy(a => a.ClinicId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(a => a.GrantedAt).First());

        return clinicsPaged.Items
            .Select(c =>
            {
                accessByClinic.TryGetValue(c.Id, out var row);
                return new PatientLinkedClinicDto
                {
                    ClinicId = c.Id,
                    Name = c.Name,
                    ReferenceCode = c.ReferenceCode ?? string.Empty,
                    AccessKind = row is null ? "CareHistory" : MapAccessType(row.AccessType),
                    GrantedAt = row?.GrantedAt
                };
            })
            .ToList();
    }

    private static string MapAccessType(PatientClinicAccessType type) => type switch
    {
        PatientClinicAccessType.EncounterBased => "EncounterBased",
        PatientClinicAccessType.Registered => "Registered",
        PatientClinicAccessType.InsuranceLinked => "InsuranceLinked",
        PatientClinicAccessType.ManualGrant => "ManualGrant",
        _ => "CareHistory"
    };
}
