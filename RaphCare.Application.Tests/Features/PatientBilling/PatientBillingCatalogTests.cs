using RaphCare.Application.Features.PatientBilling;
using RaphCare.Domain.Billing;
using Xunit;

namespace RaphCare.Application.Tests.Features.PatientBilling;

public sealed class PatientBillingCatalogTests
{
    [Fact]
    public void FromCatalogUsesCareSkuZarAmountsWhenPresent()
    {
        var options = PatientBillingCatalog.FromCatalog(
        [
            new PriceCatalogItem
            {
                SkuCode = PriceCatalogSku.CareEssential,
                DisplayName = "Essential Care",
                Category = PriceCatalogSku.Categories.PatientCare,
                AmountZar = 160m,
                AmountUsd = 7m,
                IsActive = true
            },
            new PriceCatalogItem
            {
                SkuCode = PriceCatalogSku.CareComplete,
                DisplayName = "Complete Care",
                Category = PriceCatalogSku.Categories.PatientCare,
                AmountZar = 310m,
                AmountUsd = 13m,
                IsActive = true
            }
        ]);

        Assert.Equal(160m, options.Single(o => o.PlanCode == PatientBillingCatalog.EssentialCode).MonthlyPrice);
        Assert.Equal(310m, options.Single(o => o.PlanCode == PatientBillingCatalog.CompleteCode).MonthlyPrice);
    }

    [Fact]
    public void FromCatalogFallsBackWhenCareSkusMissing()
    {
        var options = PatientBillingCatalog.FromCatalog(
        [
            new PriceCatalogItem
            {
                SkuCode = PriceCatalogSku.SiteClinic,
                DisplayName = "Clinic",
                Category = PriceCatalogSku.Categories.Site,
                AmountZar = 3490m,
                AmountUsd = 89m,
                IsActive = true
            }
        ]);

        Assert.Same(PatientBillingCatalog.FallbackAll, options);
    }
}
