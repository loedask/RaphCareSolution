using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Billing.Commands.UpdatePriceCatalogItem;
using RaphCare.Domain.Billing;
using RaphCare.Domain.Identity;
using Xunit;

namespace RaphCare.Application.Tests.Features.Billing.Commands.UpdatePriceCatalogItem;

public sealed class UpdatePriceCatalogItemHandlerTests
{
    [Fact]
    public async Task PlatformAdminCanUpdateZarAndUsdAmounts()
    {
        var item = new PriceCatalogItem
        {
            SkuCode = PriceCatalogSku.SiteClinic,
            DisplayName = "Clinic",
            Category = PriceCatalogSku.Categories.Site,
            AmountZar = 3490m,
            AmountUsd = 89m,
            IsActive = true
        };
        var catalog = new FakeCatalogRepository(item);
        var uow = new FakeUnitOfWork();
        var userId = Guid.Parse("bbbbbbbb-1111-1111-1111-111111111101");
        var handler = new UpdatePriceCatalogItemHandler(
            catalog,
            uow,
            new FakeCurrentUser(userId),
            new FakeRoles(userId, [RaphCareRoles.Administrator]));

        var result = await handler.Handle(
            new UpdatePriceCatalogItemCommand
            {
                Id = item.Id,
                AmountZar = 3600m,
                AmountUsd = 95m
            },
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(3600m, result!.AmountZar);
        Assert.Equal(95m, result.AmountUsd);
        Assert.Equal(3600m, item.AmountZar);
        Assert.Equal(95m, item.AmountUsd);
        Assert.Equal(1, uow.SaveCount);
        Assert.True(catalog.Updated);
    }

    [Fact]
    public async Task NonAdminIsForbidden()
    {
        var userId = Guid.Parse("bbbbbbbb-1111-1111-1111-111111111102");
        var handler = new UpdatePriceCatalogItemHandler(
            new FakeCatalogRepository(null),
            new FakeUnitOfWork(),
            new FakeCurrentUser(userId),
            new FakeRoles(userId, [RaphCareRoles.Doctor]));

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            handler.Handle(
                new UpdatePriceCatalogItemCommand { Id = Guid.NewGuid(), AmountZar = 1, AmountUsd = 1 },
                CancellationToken.None));
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

file sealed class FakeUnitOfWork : IUnitOfWork
{
    public int SaveCount { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveCount++;
        return Task.FromResult(1);
    }
}

file sealed class FakeCatalogRepository(PriceCatalogItem? item) : IRepository<PriceCatalogItem>
{
    public bool Updated { get; private set; }

    public Task<PriceCatalogItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(item is not null && item.Id == id ? item : null);

    public Task<IReadOnlyList<PriceCatalogItem>> ListAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<PriceCatalogItem>>(item is null ? [] : [item]);

    public Task<PagedResult<PriceCatalogItem>> SearchAsync(
        Func<IQueryable<PriceCatalogItem>, IQueryable<PriceCatalogItem>>? queryShaper,
        int pageNumber,
        int pageSize,
        bool applyDefaultIdOrdering = true,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(new PagedResult<PriceCatalogItem>
        {
            Items = item is null ? [] : [item],
            TotalCount = item is null ? 0 : 1,
            PageNumber = pageNumber,
            PageSize = pageSize
        });

    public Task AddAsync(PriceCatalogItem entity, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task UpdateAsync(PriceCatalogItem entity, CancellationToken cancellationToken = default)
    {
        Updated = true;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(PriceCatalogItem entity, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}
