using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.Commands.CreateAdminClinicRosterEntry;
using RaphCare.Application.Features.Organization.Commands.DeleteAdminClinicRosterEntry;
using RaphCare.Domain.Identity;
using RaphCare.Domain.Organization;
using Xunit;

namespace RaphCare.Application.Tests.Features.Organization.Commands.Roster;

public sealed class ClinicRosterHandlerTests
{
    [Fact]
    public async Task HandleAddsStaffToMorningShift()
    {
        var clinicId = Guid.Parse("11111111-1111-1111-1111-111111111301");
        var userId = Guid.Parse("22222222-2222-2222-2222-222222222301");
        var day = new DateTime(2026, 9, 2, 0, 0, 0, DateTimeKind.Utc);
        var entries = new FakeRepository<ClinicRosterEntry>();
        var users = new FakeProfessionalUsers(
        [
            new ApplicationUser { Id = userId, Email = "nurse@example.com", DisplayName = "Nurse Ada" }
        ]);

        var handler = new CreateAdminClinicRosterEntryHandler(
            new FakeCurrentUser(Guid.NewGuid()),
            new FakeMembership(true, [new ClinicStaffMembershipEntry
            {
                ApplicationUserId = userId,
                IsActive = true,
                JoinedAt = day.AddDays(-30)
            }]),
            users,
            new FakeRoles(["Nurse"]),
            entries,
            new FakeUnitOfWork());

        var result = await handler.Handle(
            new CreateAdminClinicRosterEntryCommand
            {
                ClinicId = clinicId,
                ApplicationUserId = userId,
                DutyDate = day.AddHours(8),
                ShiftLabel = "morning",
                Note = "Ward A"
            },
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Morning", result!.ShiftLabel);
        Assert.Equal(day, result.DutyDate);
        Assert.Equal("Nurse Ada", result.DisplayName);
        Assert.Equal("Ward A", result.Note);
        Assert.Single(entries.Items);
    }

    [Fact]
    public async Task HandleRejectsPersonWhoIsNotActiveStaff()
    {
        var handler = new CreateAdminClinicRosterEntryHandler(
            new FakeCurrentUser(Guid.NewGuid()),
            new FakeMembership(true, []),
            new FakeProfessionalUsers([]),
            new FakeRoles([]),
            new FakeRepository<ClinicRosterEntry>(),
            new FakeUnitOfWork());

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            handler.Handle(
                new CreateAdminClinicRosterEntryCommand
                {
                    ClinicId = Guid.NewGuid(),
                    ApplicationUserId = Guid.NewGuid(),
                    DutyDate = DateTime.UtcNow.Date,
                    ShiftLabel = "Morning"
                },
                CancellationToken.None));
    }

    [Fact]
    public async Task HandleRejectsDuplicateShiftSameDay()
    {
        var clinicId = Guid.Parse("11111111-1111-1111-1111-111111111302");
        var userId = Guid.Parse("22222222-2222-2222-2222-222222222302");
        var day = new DateTime(2026, 9, 2, 0, 0, 0, DateTimeKind.Utc);
        var existing = new ClinicRosterEntry
        {
            ClinicId = clinicId,
            ApplicationUserId = userId,
            DutyDate = day,
            ShiftLabel = "Afternoon"
        };
        var handler = new CreateAdminClinicRosterEntryHandler(
            new FakeCurrentUser(Guid.NewGuid()),
            new FakeMembership(true, [new ClinicStaffMembershipEntry
            {
                ApplicationUserId = userId,
                IsActive = true,
                JoinedAt = day
            }]),
            new FakeProfessionalUsers([new ApplicationUser { Id = userId, Email = "a@b.c", DisplayName = "A" }]),
            new FakeRoles(["Doctor"]),
            new FakeRepository<ClinicRosterEntry>([existing]),
            new FakeUnitOfWork());

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            handler.Handle(
                new CreateAdminClinicRosterEntryCommand
                {
                    ClinicId = clinicId,
                    ApplicationUserId = userId,
                    DutyDate = day,
                    ShiftLabel = "Afternoon"
                },
                CancellationToken.None));
    }

    [Fact]
    public async Task DeleteRemovesEntryForClinic()
    {
        var clinicId = Guid.Parse("11111111-1111-1111-1111-111111111303");
        var entry = new ClinicRosterEntry
        {
            ClinicId = clinicId,
            ApplicationUserId = Guid.NewGuid(),
            DutyDate = DateTime.UtcNow.Date,
            ShiftLabel = "Night"
        };
        var entries = new FakeRepository<ClinicRosterEntry>([entry]);
        var handler = new DeleteAdminClinicRosterEntryHandler(
            new FakeCurrentUser(Guid.NewGuid()),
            new FakeMembership(true),
            entries,
            new FakeUnitOfWork());

        var deleted = await handler.Handle(
            new DeleteAdminClinicRosterEntryCommand { ClinicId = clinicId, EntryId = entry.Id },
            CancellationToken.None);

        Assert.True(deleted);
        Assert.Empty(entries.Items);
    }

    [Fact]
    public async Task DeleteReturnsFalseForWrongClinic()
    {
        var entry = new ClinicRosterEntry
        {
            ClinicId = Guid.Parse("11111111-1111-1111-1111-111111111304"),
            ApplicationUserId = Guid.NewGuid(),
            DutyDate = DateTime.UtcNow.Date,
            ShiftLabel = "Morning"
        };
        var handler = new DeleteAdminClinicRosterEntryHandler(
            new FakeCurrentUser(Guid.NewGuid()),
            new FakeMembership(true),
            new FakeRepository<ClinicRosterEntry>([entry]),
            new FakeUnitOfWork());

        var deleted = await handler.Handle(
            new DeleteAdminClinicRosterEntryCommand
            {
                ClinicId = Guid.Parse("11111111-1111-1111-1111-111111111399"),
                EntryId = entry.Id
            },
            CancellationToken.None);

        Assert.False(deleted);
    }
}

file sealed class FakeUnitOfWork : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);
}

file sealed class FakeCurrentUser(Guid userId) : ICurrentUserService
{
    public string? UserId { get; } = userId.ToString();
    public Guid? CurrentUserId { get; } = userId;
    public string? UserName => "tester";
    public Guid? CurrentPatientId => null;
    public bool IsAuthenticated => true;
    public string? IpAddress => null;
    public string? UserAgent => null;
}

file sealed class FakeMembership(
    bool hasMembership,
    IReadOnlyList<ClinicStaffMembershipEntry>? memberships = null) : IClinicStaffMembershipService
{
    public Task EnsureMembershipAsync(Guid applicationUserId, Guid clinicId, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task<IReadOnlyList<Guid>> GetClinicIdsForUserAsync(Guid applicationUserId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Guid>>([]);

    public Task<bool> HasMembershipAsync(Guid applicationUserId, Guid clinicId, CancellationToken cancellationToken = default) =>
        Task.FromResult(hasMembership);

    public Task<int> GetActiveStaffCountAsync(Guid clinicId, CancellationToken cancellationToken = default) =>
        Task.FromResult(1);

    public Task<IReadOnlyList<ClinicStaffMembershipEntry>> GetStaffMembershipsAsync(
        Guid clinicId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(memberships ?? []);

    public Task<bool> DeactivateMembershipAsync(
        Guid applicationUserId,
        Guid clinicId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(true);

    public Task RecordInvitationSentAsync(Guid applicationUserId, Guid clinicId, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}

file sealed class FakeRoles(IReadOnlyList<string> roles) : IUserRoleAssignmentService
{
    public Task<IReadOnlyList<string>> GetRoleNamesAsync(Guid userId, CancellationToken cancellationToken = default) =>
        Task.FromResult(roles);

    public Task AssignRoleIfMissingAsync(Guid userId, string roleName, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task RemoveRoleAsync(Guid userId, string roleName, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task SetStaffJobRoleAsync(Guid userId, string jobRole, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}

file sealed class FakeProfessionalUsers(IReadOnlyList<ApplicationUser> users) : IProfessionalUserLookupService
{
    public Task<ApplicationUser?> FindByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        Task.FromResult(users.FirstOrDefault(u => u.Email == email));

    public Task<IReadOnlyList<ApplicationUser>> GetUsersByIdsAsync(
        IReadOnlyCollection<Guid> userIds,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<ApplicationUser>>(users.Where(u => userIds.Contains(u.Id)).ToList());

    public Task<bool> IsProfessionalAsync(Guid userId, CancellationToken cancellationToken = default) =>
        Task.FromResult(true);

    public Task<bool> HasSuccessfulLoginAsync(Guid userId, CancellationToken cancellationToken = default) =>
        Task.FromResult(true);
}

file sealed class FakeRepository<T> : IRepository<T> where T : class
{
    public List<T> Items { get; }

    public FakeRepository(IEnumerable<T>? items = null)
    {
        Items = items?.ToList() ?? [];
    }

    public Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var match = Items.FirstOrDefault(item =>
        {
            var value = typeof(T).GetProperty("Id")?.GetValue(item);
            return value is Guid guid && guid == id;
        });
        return Task.FromResult(match);
    }

    public Task<IReadOnlyList<T>> ListAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<T>>(Items);

    public Task<PagedResult<T>> SearchAsync(
        Func<IQueryable<T>, IQueryable<T>>? queryShaper,
        int pageNumber,
        int pageSize,
        bool applyDefaultIdOrdering = true,
        CancellationToken cancellationToken = default)
    {
        IQueryable<T> query = Items.AsQueryable();
        if (queryShaper is not null)
            query = queryShaper(query);
        var list = query.ToList();
        return Task.FromResult(new PagedResult<T>
        {
            Items = list.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList(),
            TotalCount = list.Count,
            PageNumber = pageNumber,
            PageSize = pageSize
        });
    }

    public Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        Items.Add(entity);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(T entity, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        Items.Remove(entity);
        return Task.CompletedTask;
    }
}
