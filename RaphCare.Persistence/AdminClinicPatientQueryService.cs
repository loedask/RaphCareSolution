using Microsoft.EntityFrameworkCore;
using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Persistence;

public sealed class AdminClinicPatientQueryService(ClinicalDbContext clinicalDbContext)
    : IAdminClinicPatientQueryService
{
    public async Task<PagedResult<AdminClinicPatientListItemDto>> GetPatientsAsync(
        Guid clinicId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = clinicalDbContext.PatientClinicAccesses
            .AsNoTracking()
            .Where(a => a.ClinicId == clinicId && a.IsActive)
            .Join(
                clinicalDbContext.Patients.AsNoTracking().Where(p => !p.IsDeleted),
                access => access.PatientId,
                patient => patient.Id,
                (access, patient) => new { access, patient })
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
}
