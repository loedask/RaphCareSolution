using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Ops.Queries.GetPlatformOpsStats;
using RaphCare.Domain.Clinical;
using RaphCare.Domain.Devices;
using RaphCare.Domain.Identity;
using RaphCare.Domain.Organization;
using RaphCare.Domain.Patients;
using Xunit;

namespace RaphCare.Application.Tests.Features.Ops.Queries.GetPlatformOpsStats;

/// <summary>
/// Platform Ops home needs real totals. Non-admins must not receive the payload.
/// </summary>
public sealed class GetPlatformOpsStatsHandlerTests
{
    [Fact]
    public async Task NonAdministrator_ReturnsNull()
    {
        var userId = Guid.Parse("aaaaaaaa-1111-1111-1111-111111111101");
        var handler = CreateHandler(userId, roles: [RaphCareRoles.Doctor], clinicCount: 3);

        var result = await handler.Handle(new GetPlatformOpsStatsQuery(), CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task PlatformAdministrator_ReturnsHospitalAndPatientCounts()
    {
        var userId = Guid.Parse("aaaaaaaa-1111-1111-1111-111111111102");
        var handler = CreateHandler(
            userId,
            roles: [RaphCareRoles.Administrator],
            clinicCount: 2,
            patientCount: 5,
            doctorCount: 3,
            deviceCount: 4,
            unassignedDeviceCount: 1);

        var result = await handler.Handle(new GetPlatformOpsStatsQuery(), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(2, result!.HospitalCount);
        Assert.Equal(5, result.PatientCount);
        Assert.Equal(3, result.DoctorCount);
        Assert.Equal(4, result.DeviceCount);
        Assert.Equal(1, result.UnassignedDeviceCount);
        Assert.Equal(3, result.AssignedDeviceCount);
    }

    private static GetPlatformOpsStatsHandler CreateHandler(
        Guid userId,
        IReadOnlyList<string> roles,
        int clinicCount = 0,
        int patientCount = 0,
        int doctorCount = 0,
        int deviceCount = 0,
        int unassignedDeviceCount = 0)
    {
        var assigned = Math.Max(0, deviceCount - unassignedDeviceCount);
        return new GetPlatformOpsStatsHandler(
            new FakeCurrentUser(userId),
            new FakeRoles(userId, roles),
            new CountingRepository<Clinic>(clinicCount),
            new CountingRepository<Patient>(patientCount),
            new CountingRepository<Provider>(doctorCount),
            new CountingRepository<ClinicStaffMembership>(0),
            new CountingRepository<Facility>(0),
            new DeviceCountingRepository(deviceCount, unassignedDeviceCount, assigned),
            new CountingRepository<Appointment>(0),
            new CountingRepository<InpatientAdmission>(0),
            new CountingRepository<ClinicStaffInvitation>(0),
            new CountingRepository<DeviceEmergencyEvent>(0));
    }
}

file sealed class FakeCurrentUser(Guid userId) : ICurrentUserService
{
    public string? UserId { get; } = userId.ToString();
    public Guid? CurrentUserId { get; } = userId;
    public string? UserName => "ops";
    public Guid? CurrentPatientId => null;
    public bool IsAuthenticated => true;
    public string? IpAddress => null;
    public string? UserAgent => null;
}

file sealed class FakeRoles(Guid userId, IReadOnlyList<string> roles) : IUserRoleAssignmentService
{
    public Task<IReadOnlyList<string>> GetRoleNamesAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(id == userId ? roles : Array.Empty<string>());

    public Task AssignRoleIfMissingAsync(Guid id, string roleName, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task RemoveRoleAsync(Guid id, string roleName, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task SetStaffJobRoleAsync(Guid id, string jobRole, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}

file sealed class CountingRepository<T>(int totalCount) : IRepository<T>
    where T : class
{
    public Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult<T?>(null);

    public Task<IReadOnlyList<T>> ListAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<T>>([]);

    public Task<PagedResult<T>> SearchAsync(
        Func<IQueryable<T>, IQueryable<T>>? queryShaper,
        int pageNumber,
        int pageSize,
        bool applyDefaultIdOrdering = true,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(new PagedResult<T>
        {
            Items = [],
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        });

    public Task AddAsync(T entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task UpdateAsync(T entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task DeleteAsync(T entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
}

/// <summary>
/// Returns different totals depending on whether the shaper filtered to assigned/unassigned devices.
/// </summary>
file sealed class DeviceCountingRepository(int total, int unassigned, int assigned) : IRepository<Device>
{
    public Task<Device?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult<Device?>(null);

    public Task<IReadOnlyList<Device>> ListAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Device>>([]);

    public Task<PagedResult<Device>> SearchAsync(
        Func<IQueryable<Device>, IQueryable<Device>>? queryShaper,
        int pageNumber,
        int pageSize,
        bool applyDefaultIdOrdering = true,
        CancellationToken cancellationToken = default)
    {
        var probe = new[]
        {
            new Device { IsAssigned = false },
            new Device { IsAssigned = true }
        }.AsQueryable();

        var shaped = queryShaper?.Invoke(probe) ?? probe;
        var list = shaped.ToList();
        var count = list.Count switch
        {
            0 => 0,
            1 when list[0].IsAssigned => assigned,
            1 when !list[0].IsAssigned => unassigned,
            _ => total
        };

        return Task.FromResult(new PagedResult<Device>
        {
            Items = [],
            TotalCount = count,
            PageNumber = pageNumber,
            PageSize = pageSize
        });
    }

    public Task AddAsync(Device entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task UpdateAsync(Device entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task DeleteAsync(Device entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
}
