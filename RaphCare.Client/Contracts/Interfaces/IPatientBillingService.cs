using RaphCare.Client.Contracts;
using RaphCare.Client.Models.Billing;

namespace RaphCare.Client.Contracts.Interfaces;

/// <summary>Patient billing vertical (plans, payment methods, invoices). Implemented by <see cref="RaphCare.Client.Services.PatientBillingService"/> wrapping generated <c>IClient</c>.</summary>
public interface IPatientBillingService
{
    Task<Response<IReadOnlyList<PatientBillingPlanOptionViewModel>>> GetPlanOptionsAsync(CancellationToken cancellationToken = default);

    Task<Response<PatientCarePlanViewModel>> GetMyCarePlanAsync(CancellationToken cancellationToken = default);

    Task<Response<PagedPatientInvoicesViewModel>> GetMyInvoicesAsync(int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default);

    Task<Response<IReadOnlyList<PatientPaymentMethodViewModel>>> GetMyPaymentMethodsAsync(CancellationToken cancellationToken = default);

    Task<Response<Guid>> AddPaymentMethodAsync(
        string methodType,
        string providerName,
        string maskedDetails,
        bool setAsDefault,
        CancellationToken cancellationToken = default);

    Task<Response<bool>> SetDefaultPaymentMethodAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Response<bool>> RemovePaymentMethodAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Response<bool>> UpgradePlanAsync(string planCode, CancellationToken cancellationToken = default);
}
