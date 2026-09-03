using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.Queries.GetAdminClinics;
using RaphCare.Domain.Identity;
using RaphCare.Domain.Organization;
using Xunit;

namespace RaphCare.Application.Tests.Features.Organization.Queries.GetAdminClinics;

/// <summary>
/// Ops fleet needs platform administrators (no clinic membership) to see hospitals.
/// Regression: demo.ops previously got an empty list and then a Portal clinic-header error on devices.
/// </summary>
public sealed class GetAdminClinicsHandlerTests
{
    [Fact]
    public async Task PlatformAdminWithoutMembershipSeesAllActiveClinicsAndSkipsMembershipLookup()
    {
        var userId = Guid.Parse("11111111-1111-1111-1111-111111111201");
        var demoClinicId = Guid.Parse("11111111-1111-1111-1111-111111111101");
        var otherClinicId = Guid.Parse("11111111-1111-1111-1111-111111111202");
        var inactiveClinicId = Guid.Parse("11111111-1111-1111-1111-111111111203");

        var membership = new FakeMembership([]);
        var clinics = new FakeClinicRepository(
            [
                CreateClinic(demoClinicId, "RaphCare Demo Clinic", isActive: true),
                CreateClinic(otherClinicId, "Other Hospital", isActive: true),
                CreateClinic(inactiveClinicId, "Closed Hospital", isActive: false)
            ],
            membership);

        var handler = new GetAdminClinicsHandler(
            clinics,
            new FakeCurrentUser(userId),
            membership,
            new FakeRoles(userId, [RaphCareRoles.Administrator]));

        var result = await handler.Handle(new GetAdminClinicsQuery { PageNumber = 1, PageSize = 50 }, CancellationToken.None);

        Assert.Equal(0, membership.GetClinicIdsCallCount);
        Assert.Equal(2, result.TotalCount);
        Assert.Contains(result.Items, c => c.Id == demoClinicId);
        Assert.Contains(result.Items, c => c.Id == otherClinicId);
        Assert.DoesNotContain(result.Items, c => c.Id == inactiveClinicId);
    }

    [Fact]
    public async Task StaffWithoutAdministratorSeesOnlyMembershipClinics()
    {
        var userId = Guid.Parse("11111111-1111-1111-1111-111111111204");
        var memberClinicId = Guid.Parse("11111111-1111-1111-1111-111111111205");
        var otherClinicId = Guid.Parse("11111111-1111-1111-1111-111111111206");

        var membership = new FakeMembership([memberClinicId]);
        var clinics = new FakeClinicRepository(
            [
                CreateClinic(memberClinicId, "My Clinic", isActive: true),
                CreateClinic(otherClinicId, "Other Clinic", isActive: true)
            ],
            membership);

        var handler = new GetAdminClinicsHandler(
            clinics,
            new FakeCurrentUser(userId),
            membership,
            new FakeRoles(userId, [RaphCareRoles.Doctor]));

        var result = await handler.Handle(new GetAdminClinicsQuery { PageNumber = 1, PageSize = 50 }, CancellationToken.None);

        Assert.Equal(1, membership.GetClinicIdsCallCount);
        Assert.Equal(1, result.TotalCount);
        Assert.Equal(memberClinicId, result.Items[0].Id);
    }

    private static Clinic CreateClinic(Guid id, string name, bool isActive)
    {
        var clinic = new Clinic
        {
            Name = name,
            RegistrationNumber = "REG",
            ReferenceCode = "RC",
            Country = "ZA",
            TimeZone = "UTC",
            IsActive = isActive,
            RegisteredByApplicationUserId = Guid.NewGuid()
        };
        typeof(Clinic).GetProperty(nameof(Clinic.Id))!.SetValue(clinic, id);
        return clinic;
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

file sealed class FakeMembership(IReadOnlyList<Guid> membershipClinicIds) : IClinicStaffMembershipService
{
    public IReadOnlyList<Guid> ClinicIds { get; } = membershipClinicIds;
    public int GetClinicIdsCallCount { get; private set; }

    public Task EnsureMembershipAsync(Guid applicationUserId, Guid clinicId, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task<IReadOnlyList<Guid>> GetClinicIdsForUserAsync(Guid applicationUserId, CancellationToken cancellationToken = default)
    {
        GetClinicIdsCallCount++;
        return Task.FromResult(ClinicIds);
    }

    public Task<bool> HasMembershipAsync(Guid applicationUserId, Guid clinicId, CancellationToken cancellationToken = default) =>
        Task.FromResult(ClinicIds.Contains(clinicId));

    public Task<int> GetActiveStaffCountAsync(Guid clinicId, CancellationToken cancellationToken = default) =>
        Task.FromResult(0);

    public Task<IReadOnlyList<ClinicStaffMembershipEntry>> GetStaffMembershipsAsync(
        Guid clinicId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<ClinicStaffMembershipEntry>>([]);

    public Task<bool> DeactivateMembershipAsync(
        Guid applicationUserId,
        Guid clinicId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(false);

    public Task RecordInvitationSentAsync(Guid applicationUserId, Guid clinicId, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
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

/// <summary>
/// Applies handler filters without EF <c>Include</c> (unsupported on LINQ-to-Objects).
/// Uses membership call count to mirror platform-admin vs staff branches.
/// </summary>
file sealed class FakeClinicRepository(IEnumerable<Clinic> clinics, FakeMembership membership) : IRepository<Clinic>
{
    private readonly List<Clinic> _items = clinics.ToList();

    public Task<Clinic?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_items.FirstOrDefault(c => c.Id == id));

    public Task<IReadOnlyList<Clinic>> ListAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Clinic>>(_items);

    public Task<PagedResult<Clinic>> SearchAsync(
        Func<IQueryable<Clinic>, IQueryable<Clinic>>? queryShaper,
        int pageNumber,
        int pageSize,
        bool applyDefaultIdOrdering = true,
        CancellationToken cancellationToken = default)
    {
        IEnumerable<Clinic> filtered = membership.GetClinicIdsCallCount > 0
            ? _items.Where(c => membership.ClinicIds.Contains(c.Id))
            : _items.Where(c => c.IsActive);

        var ordered = filtered.OrderByDescending(c => c.CreatedAt).ToList();
        return Task.FromResult(new PagedResult<Clinic>
        {
            Items = ordered.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList(),
            TotalCount = ordered.Count,
            PageNumber = pageNumber,
            PageSize = pageSize
        });
    }

    public Task AddAsync(Clinic entity, CancellationToken cancellationToken = default)
    {
        _items.Add(entity);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Clinic entity, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task DeleteAsync(Clinic entity, CancellationToken cancellationToken = default)
    {
        _items.Remove(entity);
        return Task.CompletedTask;
    }
}
