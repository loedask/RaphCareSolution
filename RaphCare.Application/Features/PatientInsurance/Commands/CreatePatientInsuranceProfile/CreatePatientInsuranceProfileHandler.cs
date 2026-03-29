using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Insurance;
using RaphCare.Domain.Patients;

namespace RaphCare.Application.Features.PatientInsurance.Commands.CreatePatientInsuranceProfile;

public class CreatePatientInsuranceProfileHandler : IRequestHandler<CreatePatientInsuranceProfileCommand, Guid>
{
    private readonly IRepository<InsuranceProfile> _profileRepository;
    private readonly IRepository<InsurancePlan> _planRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public CreatePatientInsuranceProfileHandler(
        IRepository<InsuranceProfile> profileRepository,
        IRepository<InsurancePlan> planRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _profileRepository = profileRepository;
        _planRepository = planRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreatePatientInsuranceProfileCommand request, CancellationToken cancellationToken)
    {
        var patientId = _currentUser.CurrentPatientId
            ?? throw new ForbiddenAccessException("A patient profile is required to add insurance.");

        var plan = await _planRepository.GetByIdAsync(request.InsurancePlanId, cancellationToken).ConfigureAwait(false);
        if (plan is null || !plan.IsActive)
            throw new NotFoundException(nameof(InsurancePlan), request.InsurancePlanId);

        var profile = new InsuranceProfile
        {
            PatientId = patientId,
            InsurancePlanId = request.InsurancePlanId,
            MembershipNumber = request.MembershipNumber,
            StartDate = request.StartDate,
            IsActive = true
        };

        await _profileRepository.AddAsync(profile, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return profile.Id;
    }
}
