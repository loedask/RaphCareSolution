using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Patients;

namespace RaphCare.Application.Features.PatientInsurance.Commands.UpdateMyInsuranceProfile;

public class UpdateMyInsuranceProfileHandler : IRequestHandler<UpdateMyInsuranceProfileCommand, Unit>
{
    private readonly IRepository<InsuranceProfile> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public UpdateMyInsuranceProfileHandler(
        IRepository<InsuranceProfile> repository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(UpdateMyInsuranceProfileCommand request, CancellationToken cancellationToken)
    {
        var patientId = _currentUser.CurrentPatientId
            ?? throw new ForbiddenAccessException("A patient profile is required to update insurance.");

        var profile = await _repository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
        if (profile is null || profile.PatientId != patientId)
            throw new NotFoundException(nameof(InsuranceProfile), request.Id);

        if (request.EndDate.HasValue)
            profile.EndDate = request.EndDate.Value;

        if (request.IsActive.HasValue)
            profile.IsActive = request.IsActive.Value;

        await _repository.UpdateAsync(profile, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Unit.Value;
    }
}
