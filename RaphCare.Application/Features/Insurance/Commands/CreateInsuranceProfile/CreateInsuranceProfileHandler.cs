using MediatR;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Patients;

namespace RaphCare.Application.Features.Insurance.Commands.CreateInsuranceProfile;

public class CreateInsuranceProfileHandler : IRequestHandler<CreateInsuranceProfileCommand, Guid>
{
    private readonly IRepository<InsuranceProfile> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CreateInsuranceProfileHandler(
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

    public async Task<Guid> Handle(CreateInsuranceProfileCommand request, CancellationToken cancellationToken)
    {
        var profile = new InsuranceProfile
        {
            PatientId = request.PatientId,
            InsurancePlanId = request.InsurancePlanId,
            MembershipNumber = request.MembershipNumber,
            StartDate = request.StartDate,
            IsActive = true
        };

        await _repository.AddAsync(profile, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return profile.Id;
    }
}

