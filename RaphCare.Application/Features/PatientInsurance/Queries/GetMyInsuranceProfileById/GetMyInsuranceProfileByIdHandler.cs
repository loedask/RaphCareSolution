using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.PatientInsurance.DTOs;
using RaphCare.Domain.Insurance;
using RaphCare.Domain.Patients;

namespace RaphCare.Application.Features.PatientInsurance.Queries.GetMyInsuranceProfileById;

public class GetMyInsuranceProfileByIdHandler : IRequestHandler<GetMyInsuranceProfileByIdQuery, PatientInsuranceProfileDto>
{
    private readonly IRepository<InsuranceProfile> _profileRepository;
    private readonly IRepository<InsurancePlan> _planRepository;
    private readonly ICurrentUserService _currentUser;

    public GetMyInsuranceProfileByIdHandler(
        IRepository<InsuranceProfile> profileRepository,
        IRepository<InsurancePlan> planRepository,
        ICurrentUserService currentUser)
    {
        _profileRepository = profileRepository;
        _planRepository = planRepository;
        _currentUser = currentUser;
    }

    public async Task<PatientInsuranceProfileDto> Handle(GetMyInsuranceProfileByIdQuery request, CancellationToken cancellationToken)
    {
        var patientId = _currentUser.CurrentPatientId
            ?? throw new ForbiddenAccessException("A patient profile is required to view insurance.");

        var profile = await _profileRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
        if (profile is null || profile.PatientId != patientId)
            throw new NotFoundException(nameof(InsuranceProfile), request.Id);

        var plan = await _planRepository.GetByIdAsync(profile.InsurancePlanId, cancellationToken).ConfigureAwait(false);

        return new PatientInsuranceProfileDto
        {
            Id = profile.Id,
            InsurancePlanId = profile.InsurancePlanId,
            PlanName = plan?.Name ?? string.Empty,
            PlanCode = plan?.Code ?? string.Empty,
            MembershipNumber = profile.MembershipNumber,
            StartDate = profile.StartDate,
            EndDate = profile.EndDate,
            IsActive = profile.IsActive
        };
    }
}
