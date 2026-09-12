using RaphCare.Client.Contracts;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models.Ops;
using RaphCare.Client.Services.Base;

namespace RaphCare.Client.Services;

public sealed class PlatformPriceCatalogService(HttpClient httpClient)
    : BaseHttpService(httpClient), IPlatformPriceCatalogService
{
    public async Task<Response<IReadOnlyList<PriceCatalogItem>>> GetCatalogAsync(
        bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var path = activeOnly ? "api/ops/price-catalog" : "api/ops/price-catalog?activeOnly=false";
        var result = await GetAsync<List<PriceCatalogItemDto>>(path, cancellationToken)
            .ConfigureAwait(false);
        if (!result.IsSuccess || result.Data is null)
            return Response<IReadOnlyList<PriceCatalogItem>>.Failure(
                result.ErrorMessage ?? "Could not load the price catalog.",
                result.StatusCode);

        return Response<IReadOnlyList<PriceCatalogItem>>.Success(
            result.Data.Select(Map).ToList());
    }

    public async Task<Response<PriceCatalogItem>> UpdateItemAsync(
        Guid id,
        decimal amountZar,
        decimal amountUsd,
        CancellationToken cancellationToken = default)
    {
        var result = await PutAsync<PriceCatalogItemDto>(
                $"api/ops/price-catalog/{id}",
                new { amountZar, amountUsd },
                cancellationToken)
            .ConfigureAwait(false);
        if (!result.IsSuccess || result.Data is null)
            return Response<PriceCatalogItem>.Failure(
                result.ErrorMessage ?? "Could not save that price.",
                result.StatusCode);

        return Response<PriceCatalogItem>.Success(Map(result.Data));
    }

    public async Task<Response<ClinicCommercialPlanInfo>> GetClinicCommercialPlanAsync(
        Guid clinicId,
        CancellationToken cancellationToken = default)
    {
        var result = await GetAsync<ClinicCommercialPlanDto>(
                $"api/ops/clinics/{clinicId}/commercial-plan",
                cancellationToken)
            .ConfigureAwait(false);
        if (!result.IsSuccess || result.Data is null)
            return Response<ClinicCommercialPlanInfo>.Failure(
                result.ErrorMessage ?? "Could not load that clinic plan.",
                result.StatusCode);

        return Response<ClinicCommercialPlanInfo>.Success(MapPlan(result.Data));
    }

    public async Task<Response<ClinicCommercialPlanInfo>> UpdateClinicCommercialPlanAsync(
        Guid clinicId,
        string commercialPlan,
        CancellationToken cancellationToken = default)
    {
        var result = await PutAsync<ClinicCommercialPlanDto>(
                $"api/ops/clinics/{clinicId}/commercial-plan",
                new { commercialPlan },
                cancellationToken)
            .ConfigureAwait(false);
        if (!result.IsSuccess || result.Data is null)
            return Response<ClinicCommercialPlanInfo>.Failure(
                result.ErrorMessage ?? "Could not save that clinic plan.",
                result.StatusCode);

        return Response<ClinicCommercialPlanInfo>.Success(MapPlan(result.Data));
    }

    private static PriceCatalogItem Map(PriceCatalogItemDto dto) => new()
    {
        Id = dto.Id,
        SkuCode = dto.SkuCode ?? string.Empty,
        DisplayName = dto.DisplayName ?? string.Empty,
        Category = dto.Category ?? string.Empty,
        AmountZar = dto.AmountZar,
        AmountUsd = dto.AmountUsd,
        IsActive = dto.IsActive
    };

    private static ClinicCommercialPlanInfo MapPlan(ClinicCommercialPlanDto dto) => new()
    {
        ClinicId = dto.ClinicId,
        CommercialPlan = dto.CommercialPlan ?? string.Empty,
        HasInpatient = dto.HasInpatient,
        HasCollection = dto.HasCollection,
        HasCasualty = dto.HasCasualty,
        HasTheatre = dto.HasTheatre,
        HasConsultWaiting = dto.HasConsultWaiting
    };

    private sealed class PriceCatalogItemDto
    {
        public Guid Id { get; set; }
        public string? SkuCode { get; set; }
        public string? DisplayName { get; set; }
        public string? Category { get; set; }
        public decimal AmountZar { get; set; }
        public decimal AmountUsd { get; set; }
        public bool IsActive { get; set; }
    }

    private sealed class ClinicCommercialPlanDto
    {
        public Guid ClinicId { get; set; }
        public string? CommercialPlan { get; set; }
        public bool HasInpatient { get; set; }
        public bool HasCollection { get; set; }
        public bool HasCasualty { get; set; }
        public bool HasTheatre { get; set; }
        public bool HasConsultWaiting { get; set; }
    }
}

