using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.PatientBilling;
using RaphCare.Application.Features.PatientBilling.Commands.UpgradeMyBillingPlan;
using RaphCare.Domain.Billing;
using Xunit;

namespace RaphCare.Application.Tests.Features.PatientBilling;

public sealed class UpgradeMyBillingPlanPaystackGateTests
{
    [Fact]
    public async Task UpgradeToEssentialWhenPaystackConfiguredRequiresCheckout()
    {
        var handler = new UpgradeMyBillingPlanHandler(
            new FakePlanRepository(),
            new FakeCatalogRepository(),
            new FakeUnitOfWork(),
            new FakeCurrentUser(Guid.NewGuid()),
            new FakeClock(),
            new ConfiguredGateway());

        var ex = await Assert.ThrowsAsync<RaphCare.Application.Common.Exceptions.ValidationException>(() =>
            handler.Handle(new UpgradeMyBillingPlanCommand { PlanCode = PatientBillingCatalog.EssentialCode }, default));

        Assert.Contains(
            ex.Errors.SelectMany(kv => kv.Value),
            msg => msg.Contains("Paystack", StringComparison.OrdinalIgnoreCase));
    }

    private sealed class ConfiguredGateway : IPaymentGatewayService
    {
        public bool IsConfigured => true;

        public Task<string> ChargeAsync(Guid patientId, decimal amount, string currency, CancellationToken cancellationToken = default) =>
            Task.FromResult("x");

        public Task<PaymentCheckoutSession?> InitializeCheckoutAsync(
            Guid patientId,
            string customerEmail,
            decimal amount,
            string currency,
            string planCode,
            string? callbackUrl,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<PaymentCheckoutSession?>(null);

        public Task<PaymentChargeVerification> VerifyCheckoutAsync(string reference, CancellationToken cancellationToken = default) =>
            Task.FromResult(new PaymentChargeVerification { Succeeded = false, Reference = reference });
    }

    private sealed class FakeCurrentUser(Guid patientId) : ICurrentUserService
    {
        public string? UserId => patientId.ToString("D");
        public Guid? CurrentUserId => patientId;
        public string? UserName => "demo";
        public Guid? CurrentPatientId => patientId;
        public bool IsAuthenticated => true;
        public string? IpAddress => null;
        public string? UserAgent => null;
    }

    private sealed class FakeClock : IDateTimeProvider
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);
    }

    private sealed class FakeCatalogRepository : IRepository<PriceCatalogItem>
    {
        public Task<PriceCatalogItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult<PriceCatalogItem?>(null);

        public Task<IReadOnlyList<PriceCatalogItem>> ListAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<PriceCatalogItem>>([]);

        public Task AddAsync(PriceCatalogItem entity, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task UpdateAsync(PriceCatalogItem entity, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task DeleteAsync(PriceCatalogItem entity, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<PagedResult<PriceCatalogItem>> SearchAsync(
            Func<IQueryable<PriceCatalogItem>, IQueryable<PriceCatalogItem>>? queryShaper,
            int pageNumber,
            int pageSize,
            bool applyDefaultIdOrdering = true,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new PagedResult<PriceCatalogItem>
            {
                Items = [],
                TotalCount = 0,
                PageNumber = pageNumber,
                PageSize = pageSize
            });
    }

    private sealed class FakePlanRepository : IRepository<PatientCarePlan>
    {
        public Task<PatientCarePlan?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult<PatientCarePlan?>(null);

        public Task<IReadOnlyList<PatientCarePlan>> ListAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<PatientCarePlan>>([]);

        public Task AddAsync(PatientCarePlan entity, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task UpdateAsync(PatientCarePlan entity, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task DeleteAsync(PatientCarePlan entity, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<PagedResult<PatientCarePlan>> SearchAsync(
            Func<IQueryable<PatientCarePlan>, IQueryable<PatientCarePlan>>? queryShaper,
            int pageNumber,
            int pageSize,
            bool applyDefaultIdOrdering = true,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new PagedResult<PatientCarePlan>
            {
                Items = [],
                TotalCount = 0,
                PageNumber = pageNumber,
                PageSize = pageSize
            });
    }
}
