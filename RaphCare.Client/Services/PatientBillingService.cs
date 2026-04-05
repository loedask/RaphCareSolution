using RaphCare.Client.Contracts;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models.Billing;
using RaphCare.Client.Services.Base;

namespace RaphCare.Client.Services;

/// <summary>Wraps generated <see cref="IClient"/> patient billing operations and maps to feature view models.</summary>
public sealed class PatientBillingService(IClient client) : IPatientBillingService
{
    public async Task<Response<IReadOnlyList<PatientBillingPlanOptionViewModel>>> GetPlanOptionsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var dtos = await client.GetPatientBillingPlanOptionsAsync(cancellationToken).ConfigureAwait(false);
            var list = (dtos ?? Array.Empty<PatientBillingPlanOptionDto>())
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
        catch (global::RaphCare.Client.Services.Base.ApiException ex)
        {
            return Response<IReadOnlyList<PatientBillingPlanOptionViewModel>>.Failure(ex.Message, ex.StatusCode);
        }
    }

    public async Task<Response<PatientCarePlanViewModel>> GetMyCarePlanAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var d = await client.GetMyPatientCarePlanAsync(cancellationToken).ConfigureAwait(false);
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
        catch (global::RaphCare.Client.Services.Base.ApiException ex)
        {
            return Response<PatientCarePlanViewModel>.Failure(ex.Message, ex.StatusCode);
        }
    }

    public async Task<Response<PagedPatientInvoicesViewModel>> GetMyInvoicesAsync(
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var p = await client.GetMyPatientInvoicesAsync(pageNumber, pageSize, cancellationToken).ConfigureAwait(false);
            var items = (p.Items ?? Array.Empty<PatientInvoiceHistoryItemDto>())
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
        catch (global::RaphCare.Client.Services.Base.ApiException ex)
        {
            return Response<PagedPatientInvoicesViewModel>.Failure(ex.Message, ex.StatusCode);
        }
    }

    public async Task<Response<IReadOnlyList<PatientPaymentMethodViewModel>>> GetMyPaymentMethodsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var dtos = await client.GetMyPaymentMethodsAsync(cancellationToken).ConfigureAwait(false);
            var list = (dtos ?? Array.Empty<PatientPaymentMethodDto>())
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
        catch (global::RaphCare.Client.Services.Base.ApiException ex)
        {
            return Response<IReadOnlyList<PatientPaymentMethodViewModel>>.Failure(ex.Message, ex.StatusCode);
        }
    }

    public async Task<Response<Guid>> AddPaymentMethodAsync(
        string methodType,
        string providerName,
        string maskedDetails,
        bool setAsDefault,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var cmd = new AddMyPaymentMethodCommand
            {
                MethodType = methodType,
                ProviderName = providerName,
                MaskedDetails = maskedDetails,
                SetAsDefault = setAsDefault
            };
            var res = await client.AddMyPaymentMethodAsync(cmd, cancellationToken).ConfigureAwait(false);
            return Response<Guid>.Success(res.Id);
        }
        catch (global::RaphCare.Client.Services.Base.ApiException ex)
        {
            return Response<Guid>.Failure(ex.Message, ex.StatusCode);
        }
    }

    public async Task<Response<bool>> SetDefaultPaymentMethodAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            await client.SetDefaultMyPaymentMethodAsync(id, cancellationToken).ConfigureAwait(false);
            return Response<bool>.Success(true);
        }
        catch (global::RaphCare.Client.Services.Base.ApiException ex)
        {
            return Response<bool>.Failure(ex.Message, ex.StatusCode);
        }
    }

    public async Task<Response<bool>> RemovePaymentMethodAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            await client.RemoveMyPaymentMethodAsync(id, cancellationToken).ConfigureAwait(false);
            return Response<bool>.Success(true);
        }
        catch (global::RaphCare.Client.Services.Base.ApiException ex)
        {
            return Response<bool>.Failure(ex.Message, ex.StatusCode);
        }
    }

    public async Task<Response<bool>> UpgradePlanAsync(string planCode, CancellationToken cancellationToken = default)
    {
        try
        {
            await client.UpgradeMyBillingPlanAsync(new UpgradeMyBillingPlanCommand { PlanCode = planCode }, cancellationToken)
                .ConfigureAwait(false);
            return Response<bool>.Success(true);
        }
        catch (global::RaphCare.Client.Services.Base.ApiException ex)
        {
            return Response<bool>.Failure(ex.Message, ex.StatusCode);
        }
    }
}
