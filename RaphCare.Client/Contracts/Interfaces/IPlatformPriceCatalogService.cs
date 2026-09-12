using RaphCare.Client.Contracts;
using RaphCare.Client.Models.Ops;

namespace RaphCare.Client.Contracts.Interfaces;

public interface IPlatformPriceCatalogService
{
    Task<Response<IReadOnlyList<PriceCatalogItem>>> GetCatalogAsync(
        bool activeOnly = true,
        CancellationToken cancellationToken = default);

    Task<Response<PriceCatalogItem>> UpdateItemAsync(
        Guid id,
        decimal amountZar,
        decimal amountUsd,
        CancellationToken cancellationToken = default);

    Task<Response<ClinicCommercialPlanInfo>> GetClinicCommercialPlanAsync(
        Guid clinicId,
        CancellationToken cancellationToken = default);

    Task<Response<ClinicCommercialPlanInfo>> UpdateClinicCommercialPlanAsync(
        Guid clinicId,
        string commercialPlan,
        CancellationToken cancellationToken = default);
}

