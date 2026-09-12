using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Organization;

namespace RaphCare.Application.Features.Organization;

/// <summary>Ensures hospital-only boards are blocked for Practice and Clinic commercial plans (HTTP 403).</summary>
internal static class AdminClinicCommercialPlanGuard
{
    public static async Task EnsureFeatureAsync(
        IRepository<Clinic> clinicRepository,
        Guid clinicId,
        Func<string?, bool> hasFeature,
        string featureLabel,
        CancellationToken cancellationToken)
    {
        var clinic = await clinicRepository.GetByIdAsync(clinicId, cancellationToken).ConfigureAwait(false);
        if (clinic is null)
            throw new NotFoundException(nameof(Clinic), clinicId);

        if (!hasFeature(clinic.CommercialPlan))
        {
            throw new ForbiddenAccessException(
                $"{featureLabel} is not included on the {ClinicCommercialPlan.Normalize(clinic.CommercialPlan)} plan.");
        }
    }
}
