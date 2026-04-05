using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using RaphCare.Client.Contracts;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models.Billing;

namespace RaphCare.Client.Services;

/// <summary>JSON client for <c>api/patient/billing</c> (Bearer via named HttpClient).</summary>
public sealed class PatientBillingService(IHttpClientFactory httpClientFactory) : IPatientBillingService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    private HttpClient Client => _httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);

    public async Task<Response<IReadOnlyList<PatientBillingPlanOptionViewModel>>> GetPlanOptionsAsync(CancellationToken cancellationToken = default)
    {
        using var r = await Client.GetAsync("api/patient/billing/plans", cancellationToken).ConfigureAwait(false);
        if (!r.IsSuccessStatusCode)
            return await Failure<IReadOnlyList<PatientBillingPlanOptionViewModel>>(r, cancellationToken).ConfigureAwait(false);

        var list = await r.Content.ReadFromJsonAsync<List<PlanOptionJson>>(JsonOptions, cancellationToken).ConfigureAwait(false);
        var vm = (list ?? []).Select(x => new PatientBillingPlanOptionViewModel
        {
            PlanCode = x.PlanCode ?? string.Empty,
            DisplayName = x.DisplayName ?? string.Empty,
            Tier = x.Tier,
            MonthlyPrice = x.MonthlyPrice,
            Currency = x.Currency ?? "ZAR",
            Description = x.Description ?? string.Empty
        }).ToList();
        return Response<IReadOnlyList<PatientBillingPlanOptionViewModel>>.Success(vm);
    }

    public async Task<Response<PatientCarePlanViewModel>> GetMyCarePlanAsync(CancellationToken cancellationToken = default)
    {
        using var r = await Client.GetAsync("api/patient/billing/care-plan", cancellationToken).ConfigureAwait(false);
        if (!r.IsSuccessStatusCode)
            return await Failure<PatientCarePlanViewModel>(r, cancellationToken).ConfigureAwait(false);

        var x = await r.Content.ReadFromJsonAsync<CarePlanJson>(JsonOptions, cancellationToken).ConfigureAwait(false);
        if (x is null)
            return Response<PatientCarePlanViewModel>.Failure("Empty response.", (int)r.StatusCode);

        return Response<PatientCarePlanViewModel>.Success(new PatientCarePlanViewModel
        {
            PlanCode = x.PlanCode ?? string.Empty,
            PlanDisplayName = x.PlanDisplayName ?? string.Empty,
            Tier = x.Tier,
            EffectiveFrom = x.EffectiveFrom,
            RenewsOn = x.RenewsOn,
            Status = x.Status ?? string.Empty
        });
    }

    public async Task<Response<PagedPatientInvoicesViewModel>> GetMyInvoicesAsync(
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var url = $"api/patient/billing/invoices?pageNumber={pageNumber}&pageSize={pageSize}";
        using var r = await Client.GetAsync(url, cancellationToken).ConfigureAwait(false);
        if (!r.IsSuccessStatusCode)
            return await Failure<PagedPatientInvoicesViewModel>(r, cancellationToken).ConfigureAwait(false);

        var p = await r.Content.ReadFromJsonAsync<PagedInvoicesJson>(JsonOptions, cancellationToken).ConfigureAwait(false);
        if (p is null)
            return Response<PagedPatientInvoicesViewModel>.Failure("Empty response.", (int)r.StatusCode);

        var items = (p.Items ?? []).Select(i => new PatientInvoiceHistoryItemViewModel
        {
            Id = i.Id,
            Amount = i.Amount,
            Currency = i.Currency ?? string.Empty,
            Status = i.Status ?? string.Empty,
            DueDate = i.DueDate,
            PaidAt = i.PaidAt
        }).ToList();

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
        using var r = await Client.GetAsync("api/patient/billing/payment-methods", cancellationToken).ConfigureAwait(false);
        if (!r.IsSuccessStatusCode)
            return await Failure<IReadOnlyList<PatientPaymentMethodViewModel>>(r, cancellationToken).ConfigureAwait(false);

        var list = await r.Content.ReadFromJsonAsync<List<PaymentMethodJson>>(JsonOptions, cancellationToken).ConfigureAwait(false);
        var vm = (list ?? []).Select(m => new PatientPaymentMethodViewModel
        {
            Id = m.Id,
            MethodType = m.MethodType ?? string.Empty,
            ProviderName = m.ProviderName ?? string.Empty,
            MaskedDetails = m.MaskedDetails ?? string.Empty,
            IsDefault = m.IsDefault
        }).ToList();
        return Response<IReadOnlyList<PatientPaymentMethodViewModel>>.Success(vm);
    }

    public async Task<Response<Guid>> AddPaymentMethodAsync(
        string methodType,
        string providerName,
        string maskedDetails,
        bool setAsDefault,
        CancellationToken cancellationToken = default)
    {
        using var r = await Client.PostAsJsonAsync(
            "api/patient/billing/payment-methods",
            new
            {
                methodType,
                providerName,
                maskedDetails,
                setAsDefault
            },
            JsonOptions,
            cancellationToken).ConfigureAwait(false);

        if (r.StatusCode != HttpStatusCode.Created)
            return await Failure<Guid>(r, cancellationToken).ConfigureAwait(false);

        var body = await r.Content.ReadFromJsonAsync<CreatedIdJson>(JsonOptions, cancellationToken).ConfigureAwait(false);
        return body is null
            ? Response<Guid>.Failure("Empty response.", (int)r.StatusCode)
            : Response<Guid>.Success(body.Id);
    }

    public async Task<Response<bool>> SetDefaultPaymentMethodAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var r = await Client.PutAsync($"api/patient/billing/payment-methods/{id:N}/default", null, cancellationToken).ConfigureAwait(false);
        if (!r.IsSuccessStatusCode)
            return await Failure<bool>(r, cancellationToken).ConfigureAwait(false);
        return Response<bool>.Success(true);
    }

    public async Task<Response<bool>> RemovePaymentMethodAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var r = await Client.DeleteAsync($"api/patient/billing/payment-methods/{id:N}", cancellationToken).ConfigureAwait(false);
        if (!r.IsSuccessStatusCode)
            return await Failure<bool>(r, cancellationToken).ConfigureAwait(false);
        return Response<bool>.Success(true);
    }

    public async Task<Response<bool>> UpgradePlanAsync(string planCode, CancellationToken cancellationToken = default)
    {
        using var r = await Client.PostAsJsonAsync(
            "api/patient/billing/upgrade",
            new { planCode },
            JsonOptions,
            cancellationToken).ConfigureAwait(false);
        if (r.StatusCode != HttpStatusCode.NoContent)
            return await Failure<bool>(r, cancellationToken).ConfigureAwait(false);
        return Response<bool>.Success(true);
    }

    private static async Task<Response<T>> Failure<T>(HttpResponseMessage r, CancellationToken cancellationToken)
    {
        var text = r.Content is null ? string.Empty : await r.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return Response<T>.Failure(string.IsNullOrWhiteSpace(text) ? r.ReasonPhrase ?? "Request failed." : text, (int)r.StatusCode);
    }

    private sealed class PlanOptionJson
    {
        public string? PlanCode { get; set; }
        public string? DisplayName { get; set; }
        public int Tier { get; set; }
        public decimal MonthlyPrice { get; set; }
        public string? Currency { get; set; }
        public string? Description { get; set; }
    }

    private sealed class CarePlanJson
    {
        public string? PlanCode { get; set; }
        public string? PlanDisplayName { get; set; }
        public int Tier { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime? RenewsOn { get; set; }
        public string? Status { get; set; }
    }

    private sealed class PagedInvoicesJson
    {
        public List<InvoiceItemJson>? Items { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    private sealed class InvoiceItemJson
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
        public string? Currency { get; set; }
        public string? Status { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? PaidAt { get; set; }
    }

    private sealed class PaymentMethodJson
    {
        public Guid Id { get; set; }
        public string? MethodType { get; set; }
        public string? ProviderName { get; set; }
        public string? MaskedDetails { get; set; }
        public bool IsDefault { get; set; }
    }

    private sealed class CreatedIdJson
    {
        public Guid Id { get; set; }
    }
}
