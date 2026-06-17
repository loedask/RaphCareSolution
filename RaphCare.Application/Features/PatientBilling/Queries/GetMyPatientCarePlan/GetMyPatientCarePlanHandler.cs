using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.PatientBilling.DTOs;
using RaphCare.Application.Features.PatientBilling;
using RaphCare.Domain.Billing;

namespace RaphCare.Application.Features.PatientBilling.Queries.GetMyPatientCarePlan;

public sealed class GetMyPatientCarePlanHandler(
    IRepository<PatientCarePlan> plans,
    ICurrentUserService currentUser) : IRequestHandler<GetMyPatientCarePlanQuery, PatientCarePlanDto>
{
    private readonly IRepository<PatientCarePlan> _plans = plans;
    private readonly ICurrentUserService _currentUser = currentUser;

    public async Task<PatientCarePlanDto> Handle(GetMyPatientCarePlanQuery request, CancellationToken cancellationToken)
    {
        var patientId = _currentUser.CurrentPatientId
            ?? throw new ForbiddenAccessException("A patient profile is required.");

        var paged = await _plans.SearchAsync(
            q => q.Where(p => p.PatientId == patientId),
            1,
            1,
            true,
            cancellationToken).ConfigureAwait(false);

        if (paged.Items.Count == 0)
        {
            var free = PatientBillingCatalog.FindByCode(PatientBillingCatalog.FreeCode)!;
            return new PatientCarePlanDto
            {
                PlanCode = free.PlanCode,
                PlanDisplayName = free.DisplayName,
                Tier = free.Tier,
                EffectiveFrom = DateTime.UtcNow.Date,
                RenewsOn = null,
                Status = "Active"
            };
        }

        var row = paged.Items[0];
        return new PatientCarePlanDto
        {
            PlanCode = row.PlanCode,
            PlanDisplayName = row.PlanDisplayName,
            Tier = row.Tier,
            EffectiveFrom = row.EffectiveFrom,
            RenewsOn = row.RenewsOn,
            Status = row.Status
        };
    }
}
