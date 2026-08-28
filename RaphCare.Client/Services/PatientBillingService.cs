using RaphCare.Client.Contracts;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models.Api;
using RaphCare.Client.Models.Appointments;
using RaphCare.Client.Models.Billing;
using RaphCare.Client.Services.Base;

namespace RaphCare.Client.Services;

public sealed class PatientBillingService(HttpClient httpClient) : BaseHttpService(httpClient), IPatientBillingService
{
    public async Task<Response<IReadOnlyList<PatientBillingPlanOptionViewModel>>> GetPlanOptionsAsync(CancellationToken cancellationToken = default)
    {
        var result = await GetAsync<IReadOnlyList<BillingPlanOptionDto>>("api/patient/billing/plans", cancellationToken).ConfigureAwait(false);
        if (!result.IsSuccess)
            return Response<IReadOnlyList<PatientBillingPlanOptionViewModel>>.Failure(result.ErrorMessage ?? "Could not load plans.", result.StatusCode);

        var list = (result.Data ?? Array.Empty<BillingPlanOptionDto>())
            .Select(d => new PatientBillingPlanOptionViewModel
            {
                PlanCode = d.PlanCode ?? string.Empty,
                DisplayName = d.DisplayName ?? string.Empty,
                Tier = d.Tier,
                MonthlyPrice = (decimal)d.MonthlyPrice,
                Currency = string.IsNullOrEmpty(d.Currency) ? "ZAR" : d.Currency,
                Description = d.Description ?? string.Empty
            })
            .ToList();
        return Response<IReadOnlyList<PatientBillingPlanOptionViewModel>>.Success(list);
    }

    public async Task<Response<PatientCarePlanViewModel>> GetMyCarePlanAsync(CancellationToken cancellationToken = default)
    {
        var result = await GetAsync<CarePlanDto>("api/patient/billing/care-plan", cancellationToken).ConfigureAwait(false);
        if (!result.IsSuccess || result.Data is null)
            return Response<PatientCarePlanViewModel>.Failure(result.ErrorMessage ?? "Could not load care plan.", result.StatusCode);

        var d = result.Data;
        return Response<PatientCarePlanViewModel>.Success(new PatientCarePlanViewModel
        {
            PlanCode = d.PlanCode ?? string.Empty,
            PlanDisplayName = d.PlanDisplayName ?? string.Empty,
            Tier = d.Tier,
            EffectiveFrom = d.EffectiveFrom,
            RenewsOn = d.RenewsOn,
            Status = d.Status ?? string.Empty
        });
    }

    public async Task<Response<PagedPatientInvoicesViewModel>> GetMyInvoicesAsync(
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await GetAsync<PagedApiResult<InvoiceHistoryItemDto>>(
                $"api/patient/billing/invoices?pageNumber={pageNumber}&pageSize={pageSize}",
                cancellationToken)
            .ConfigureAwait(false);

        if (!result.IsSuccess || result.Data is null)
            return Response<PagedPatientInvoicesViewModel>.Failure(result.ErrorMessage ?? "Could not load invoices.", result.StatusCode);

        var p = result.Data;
        var items = (p.Items ?? Array.Empty<InvoiceHistoryItemDto>())
            .Select(i => new PatientInvoiceHistoryItemViewModel
            {
                Id = i.Id,
                Amount = (decimal)i.Amount,
                Currency = i.Currency ?? string.Empty,
                Status = i.Status ?? string.Empty,
                DueDate = i.DueDate,
                PaidAt = i.PaidAt
            })
            .ToList();

        return Response<PagedPatientInvoicesViewModel>.Success(new PagedPatientInvoicesViewModel
        {
            Items = items,
            TotalCount = p.TotalCount,
            PageNumber = p.PageNumber,
            PageSize = p.PageSize
        });
    }

    public async Task<Response<IReadOnlyList<PatientPaymentMethodViewModel>>> GetMyPaymentMethodsAsync(CancellationToken cancellationToken = default)
    {
        var result = await GetAsync<IReadOnlyList<PaymentMethodDto>>("api/patient/billing/payment-methods", cancellationToken).ConfigureAwait(false);
        if (!result.IsSuccess)
            return Response<IReadOnlyList<PatientPaymentMethodViewModel>>.Failure(result.ErrorMessage ?? "Could not load payment methods.", result.StatusCode);

        var list = (result.Data ?? Array.Empty<PaymentMethodDto>())
            .Select(m => new PatientPaymentMethodViewModel
            {
                Id = m.Id,
                MethodType = m.MethodType ?? string.Empty,
                ProviderName = m.ProviderName ?? string.Empty,
                MaskedDetails = m.MaskedDetails ?? string.Empty,
                IsDefault = m.IsDefault
            })
            .ToList();
        return Response<IReadOnlyList<PatientPaymentMethodViewModel>>.Success(list);
    }

    public async Task<Response<Guid>> AddPaymentMethodAsync(
        string methodType,
        string providerName,
        string maskedDetails,
        bool setAsDefault,
        CancellationToken cancellationToken = default)
    {
        var body = new { methodType, providerName, maskedDetails, setAsDefault };
        var result = await PostAsync<CreatedGuidApiResponse>("api/patient/billing/payment-methods", body, cancellationToken).ConfigureAwait(false);
        if (!result.IsSuccess || result.Data is null)
            return Response<Guid>.Failure(result.ErrorMessage ?? "Add failed.", result.StatusCode);
        return Response<Guid>.Success(result.Data.Id);
    }

    public async Task<Response<bool>> SetDefaultPaymentMethodAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await PutNoContentAsync($"api/patient/billing/payment-methods/{id}/default", body: null, cancellationToken).ConfigureAwait(false);
        return result.IsSuccess
            ? Response<bool>.Success(true)
            : Response<bool>.Failure(result.ErrorMessage ?? "Update failed.", result.StatusCode);
    }

    public async Task<Response<bool>> RemovePaymentMethodAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await DeleteAsync($"api/patient/billing/payment-methods/{id}", cancellationToken).ConfigureAwait(false);
        return result.IsSuccess
            ? Response<bool>.Success(true)
            : Response<bool>.Failure(result.ErrorMessage ?? "Remove failed.", result.StatusCode);
    }

    public async Task<Response<bool>> UpgradePlanAsync(string planCode, CancellationToken cancellationToken = default)
    {
        var result = await PostNoContentAsync("api/patient/billing/upgrade", new { planCode }, cancellationToken).ConfigureAwait(false);
        return result.IsSuccess
            ? Response<bool>.Success(true)
            : Response<bool>.Failure(result.ErrorMessage ?? "Upgrade failed.", result.StatusCode);
    }

    private sealed class BillingPlanOptionDto
    {
        public string? PlanCode { get; set; }
        public string? DisplayName { get; set; }
        public int Tier { get; set; }
        public double MonthlyPrice { get; set; }
        public string? Currency { get; set; }
        public string? Description { get; set; }
    }

    private sealed class CarePlanDto
    {
        public string? PlanCode { get; set; }
        public string? PlanDisplayName { get; set; }
        public int Tier { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime? RenewsOn { get; set; }
        public string? Status { get; set; }
    }

    private sealed class InvoiceHistoryItemDto
    {
        public Guid Id { get; set; }
        public double Amount { get; set; }
        public string? Currency { get; set; }
        public string? Status { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? PaidAt { get; set; }
    }

    private sealed class PaymentMethodDto
    {
        public Guid Id { get; set; }
        public string? MethodType { get; set; }
        public string? ProviderName { get; set; }
        public string? MaskedDetails { get; set; }
        public bool IsDefault { get; set; }
    }
}
