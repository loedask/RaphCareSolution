using Microsoft.EntityFrameworkCore;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Persistence;

public sealed class AdminClinicProviderQueryService(
    ClinicalDbContext clinicalDbContext,
    IProfessionalUserLookupService professionalUserLookupService)
    : IAdminClinicProviderQueryService
{
    public async Task<IReadOnlyList<AdminClinicProviderListItemDto>> GetProvidersAsync(
        Guid clinicId,
        CancellationToken cancellationToken = default)
    {
        var providers = await clinicalDbContext.Set<Domain.Organization.Provider>()
            .AsNoTracking()
            .Where(p => p.ClinicId == clinicId && !p.IsDeleted)
            .Include(p => p.ProviderSchedules)
            .OrderByDescending(p => p.IsActive)
            .ThenBy(p => p.LicenseNumber)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (providers.Count == 0)
            return [];

        var userIds = providers.Select(p => p.ApplicationUserId).Distinct().ToList();
        var users = await professionalUserLookupService
            .GetUsersByIdsAsync(userIds, cancellationToken)
            .ConfigureAwait(false);
        var usersById = users.ToDictionary(u => u.Id);

        return providers.Select(p =>
        {
            usersById.TryGetValue(p.ApplicationUserId, out var user);
            return new AdminClinicProviderListItemDto
            {
                ProviderId = p.Id,
                ApplicationUserId = p.ApplicationUserId,
                DisplayName = user?.DisplayName ?? user?.Email ?? "Provider",
                Email = user?.Email ?? string.Empty,
                LicenseNumber = p.LicenseNumber,
                IsActive = p.IsActive,
                ScheduleSlotCount = p.ProviderSchedules.Count
            };
        }).ToList();
    }

    public async Task<AdminClinicProviderDetailDto?> GetProviderDetailAsync(
        Guid clinicId,
        Guid providerId,
        CancellationToken cancellationToken = default)
    {
        var provider = await clinicalDbContext.Set<Domain.Organization.Provider>()
            .AsNoTracking()
            .Include(p => p.ProviderSchedules)
            .FirstOrDefaultAsync(p => p.Id == providerId && p.ClinicId == clinicId && !p.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (provider is null)
            return null;

        var users = await professionalUserLookupService
            .GetUsersByIdsAsync([provider.ApplicationUserId], cancellationToken)
            .ConfigureAwait(false);
        var user = users.Count > 0 ? users[0] : null;

        return new AdminClinicProviderDetailDto
        {
            ProviderId = provider.Id,
            ApplicationUserId = provider.ApplicationUserId,
            DisplayName = user?.DisplayName ?? user?.Email ?? "Provider",
            Email = user?.Email ?? string.Empty,
            LicenseNumber = provider.LicenseNumber,
            IsActive = provider.IsActive,
            Schedules = provider.ProviderSchedules
                .OrderBy(s => s.Day)
                .ThenBy(s => s.StartTime)
                .Select(s => new AdminClinicProviderScheduleDto
                {
                    Id = s.Id,
                    Day = s.Day,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    IsRecurring = s.IsRecurring
                })
                .ToList()
        };
    }
}
