using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Appointments.Queries.GetBookableProviders;
using RaphCare.Application.Features.Organization.DTOs;
using Xunit;

namespace RaphCare.Application.Tests.Features.Appointments.Queries.GetBookableProviders;

public sealed class GetBookableProvidersHandlerTests
{
    [Fact]
    public async Task HandleReturnsOnlyActiveProvidersOrderedByDisplayName()
    {
        var clinicId = Guid.Parse("11111111-1111-1111-1111-111111111101");
        var inactive = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var activeB = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        var activeA = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

        var handler = new GetBookableProvidersHandler(
            new FakeClinicContext(clinicId),
            new FakeProviderQuery(
            [
                new AdminClinicProviderListItemDto
                {
                    ProviderId = inactive,
                    DisplayName = "Dr Zebra",
                    IsActive = false
                },
                new AdminClinicProviderListItemDto
                {
                    ProviderId = activeB,
                    DisplayName = "Dr Beta",
                    IsActive = true
                },
                new AdminClinicProviderListItemDto
                {
                    ProviderId = activeA,
                    DisplayName = "Dr Alpha",
                    IsActive = true
                }
            ]));

        var result = await handler.Handle(new GetBookableProvidersQuery(), CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal("Dr Alpha", result[0].DisplayName);
        Assert.Equal(activeA, result[0].Id);
        Assert.Equal("Dr Beta", result[1].DisplayName);
        Assert.DoesNotContain(result, p => p.Id == inactive);
    }

    [Fact]
    public async Task HandleThrowsWhenClinicContextMissing()
    {
        var handler = new GetBookableProvidersHandler(
            new FakeClinicContext(null),
            new FakeProviderQuery([]));

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            handler.Handle(new GetBookableProvidersQuery(), CancellationToken.None));
    }

    private sealed class FakeClinicContext(Guid? clinicId) : IClinicContext
    {
        public Guid? ClinicId { get; } = clinicId;
    }

    private sealed class FakeProviderQuery(IReadOnlyList<AdminClinicProviderListItemDto> providers)
        : IAdminClinicProviderQueryService
    {
        public Task<IReadOnlyList<AdminClinicProviderListItemDto>> GetProvidersAsync(
            Guid clinicId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(providers);

        public Task<AdminClinicProviderDetailDto?> GetProviderDetailAsync(
            Guid clinicId,
            Guid providerId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<AdminClinicProviderDetailDto?>(null);
    }
}
