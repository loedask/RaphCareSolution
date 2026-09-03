using System.Linq.Expressions;
using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.Queries.GetAdminClinics;
using RaphCare.Domain.Identity;
using RaphCare.Domain.Organization;
using Xunit;

namespace RaphCare.Application.Tests.Features.Organization.Queries.GetAdminClinics;

/// <summary>
/// Regression: demo.admin (Administrator + Demo Clinic membership) must not see unrelated hospitals
/// such as Daskana. Ops (Administrator, no membership) still sees every active hospital.
/// </summary>
public sealed class GetAdminClinicsHandlerTests
{
    private static readonly Guid DemoClinicId = Guid.Parse("11111111-1111-1111-1111-111111111101");
    private static readonly Guid DaskanaClinicId = Guid.Parse("11111111-1111-1111-1111-111111111208");
    private static readonly Guid OtherActiveClinicId = Guid.Parse("11111111-1111-1111-1111-111111111202");
    private static readonly Guid InactiveClinicId = Guid.Parse("11111111-1111-1111-1111-111111111203");

    [Fact]
    public async Task DemoHospitalAdmin_DoesNotSeeUnrelatedHospitals_EvenWithAdministratorRole()
    {
        // Mirrors staging: demo.admin@raphcare.com is Administrator and a member of Demo Clinic only.
        var demoAdminUserId = Guid.Parse("11111111-1111-1111-1111-111111111207");

        var membership = new FakeMembership([DemoClinicId]);
        var clinics = new FakeClinicRepository(
        [
            CreateClinic(DemoClinicId, "RaphCare Demo Clinic", isActive: true),
            CreateClinic(DaskanaClinicId, "Daskana Hospital", isActive: true),
            CreateClinic(OtherActiveClinicId, "Other Hospital", isActive: true)
        ]);

        var handler = new GetAdminClinicsHandler(
            clinics,
            new FakeCurrentUser(demoAdminUserId),
            membership,
            new FakeRoles(demoAdminUserId, [RaphCareRoles.Administrator]));

        var result = await handler.Handle(new GetAdminClinicsQuery { PageNumber = 1, PageSize = 50 }, CancellationToken.None);

        Assert.Equal(1, result.TotalCount);
        Assert.Equal(DemoClinicId, Assert.Single(result.Items).Id);
        Assert.DoesNotContain(result.Items, c => c.Id == DaskanaClinicId);
        Assert.DoesNotContain(result.Items, c => c.Name.Contains("Daskana", StringComparison.OrdinalIgnoreCase));
        Assert.Equal("membership", clinics.LastFilterKind);
    }

    [Fact]
    public async Task DemoOpsAdmin_WithoutMembership_SeesAllActiveHospitals_IncludingUnrelated()
    {
        // Mirrors staging: demo.ops@raphcare.com is Administrator with no clinic membership.
        var demoOpsUserId = Guid.Parse("11111111-1111-1111-1111-111111111201");

        var membership = new FakeMembership([]);
        var clinics = new FakeClinicRepository(
        [
            CreateClinic(DemoClinicId, "RaphCare Demo Clinic", isActive: true),
            CreateClinic(DaskanaClinicId, "Daskana Hospital", isActive: true),
            CreateClinic(InactiveClinicId, "Closed Hospital", isActive: false)
        ]);

        var handler = new GetAdminClinicsHandler(
            clinics,
            new FakeCurrentUser(demoOpsUserId),
            membership,
            new FakeRoles(demoOpsUserId, [RaphCareRoles.Administrator]));

        var result = await handler.Handle(new GetAdminClinicsQuery { PageNumber = 1, PageSize = 50 }, CancellationToken.None);

        Assert.Equal(2, result.TotalCount);
        Assert.Contains(result.Items, c => c.Id == DemoClinicId);
        Assert.Contains(result.Items, c => c.Id == DaskanaClinicId);
        Assert.DoesNotContain(result.Items, c => c.Id == InactiveClinicId);
        Assert.Equal("all-active", clinics.LastFilterKind);
    }

    [Fact]
    public async Task StaffWithoutAdministrator_SeesOnlyMembershipClinics()
    {
        var userId = Guid.Parse("11111111-1111-1111-1111-111111111204");
        var memberClinicId = Guid.Parse("11111111-1111-1111-1111-111111111205");
        var otherClinicId = Guid.Parse("11111111-1111-1111-1111-111111111206");

        var membership = new FakeMembership([memberClinicId]);
        var clinics = new FakeClinicRepository(
        [
            CreateClinic(memberClinicId, "My Clinic", isActive: true),
            CreateClinic(otherClinicId, "Other Clinic", isActive: true)
        ]);

        var handler = new GetAdminClinicsHandler(
            clinics,
            new FakeCurrentUser(userId),
            membership,
            new FakeRoles(userId, [RaphCareRoles.Doctor]));

        var result = await handler.Handle(new GetAdminClinicsQuery { PageNumber = 1, PageSize = 50 }, CancellationToken.None);

        Assert.Equal(1, result.TotalCount);
        Assert.Equal(memberClinicId, result.Items[0].Id);
        Assert.Equal("membership", clinics.LastFilterKind);
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
    public string? UserName => "demo";
    public Guid? CurrentPatientId => null;
    public bool IsAuthenticated => true;
    public string? IpAddress => null;
    public string? UserAgent => null;
}

file sealed class FakeMembership(IReadOnlyList<Guid> membershipClinicIds) : IClinicStaffMembershipService
{
    public IReadOnlyList<Guid> ClinicIds { get; } = membershipClinicIds;

    public Task EnsureMembershipAsync(Guid applicationUserId, Guid clinicId, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task<IReadOnlyList<Guid>> GetClinicIdsForUserAsync(Guid applicationUserId, CancellationToken cancellationToken = default) =>
        Task.FromResult(ClinicIds);

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
/// Executes the handler query shaper after stripping EF <c>Include</c> calls so LINQ-to-Objects works.
/// Records whether the filter was all-active or membership-scoped (so buggy "admin sees everything" fails).
/// </summary>
file sealed class FakeClinicRepository(IEnumerable<Clinic> clinics) : IRepository<Clinic>
{
    private readonly List<Clinic> _items = clinics.ToList();

    public string? LastFilterKind { get; private set; }

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
        IQueryable<Clinic> query = _items.AsQueryable();
        if (queryShaper is not null)
            query = IncludeStrippingQueryRewriter.Apply(queryShaper, query);

        var ordered = query.ToList();
        LastFilterKind = ClassifyFilter(ordered);

        return Task.FromResult(new PagedResult<Clinic>
        {
            Items = ordered.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList(),
            TotalCount = ordered.Count,
            PageNumber = pageNumber,
            PageSize = pageSize
        });
    }

    private string ClassifyFilter(IReadOnlyList<Clinic> ordered)
    {
        var activeIds = _items.Where(c => c.IsActive).Select(c => c.Id).OrderBy(id => id).ToList();
        var resultIds = ordered.Select(c => c.Id).OrderBy(id => id).ToList();
        if (resultIds.SequenceEqual(activeIds))
            return "all-active";

        return "membership";
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

/// <summary>
/// Removes EF Core <c>Include</c> / <c>ThenInclude</c> method calls so handler shapes run on LINQ-to-Objects.
/// </summary>
file static class IncludeStrippingQueryRewriter
{
    public static IQueryable<T> Apply<T>(Func<IQueryable<T>, IQueryable<T>> shaper, IQueryable<T> source)
    {
        var rewritten = new RemoveIncludeVisitor().Visit(shaper(source).Expression)
                        ?? throw new InvalidOperationException("Query rewrite produced a null expression.");
        return source.Provider.CreateQuery<T>(rewritten);
    }

    private sealed class RemoveIncludeVisitor : ExpressionVisitor
    {
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.DeclaringType?.FullName == "Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions"
                && (node.Method.Name is "Include" or "ThenInclude"))
            {
                return Visit(node.Arguments[0]);
            }

            return base.VisitMethodCall(node);
        }
    }
}
