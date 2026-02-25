using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Patients;

namespace RaphCare.Application.Features.Insurance.Commands.UpdateInsuranceProfile;

public class UpdateInsuranceProfileHandler : IRequestHandler<UpdateInsuranceProfileCommand, Unit>
{
    private readonly IRepository<InsuranceProfile> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public UpdateInsuranceProfileHandler(
        IRepository<InsuranceProfile> repository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Unit> Handle(UpdateInsuranceProfileCommand request, CancellationToken cancellationToken)
    {
        var profile = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (profile is null)
        {
            throw new NotFoundException(nameof(InsuranceProfile), request.Id);
        }

        if (request.EndDate.HasValue)
        {
            profile.EndDate = request.EndDate.Value;
        }

        if (request.IsActive.HasValue)
        {
            profile.IsActive = request.IsActive.Value;
        }

        await _repository.UpdateAsync(profile, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}

