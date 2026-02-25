using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Patients;

namespace RaphCare.Application.Features.Patients.Commands.UpdatePatient;

public class UpdatePatientHandler(
    IRepository<Patient> repository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    IDateTimeProvider dateTimeProvider) : IRequestHandler<UpdatePatientCommand, Unit>
{
    private readonly IRepository<Patient> _repository = repository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

    public async Task<Unit> Handle(UpdatePatientCommand request, CancellationToken cancellationToken)
    {
        var patient = await _repository.GetByIdAsync(request.Id, cancellationToken) ?? throw new NotFoundException(nameof(Patient), request.Id);

        if (!string.IsNullOrWhiteSpace(request.FirstName))
        {
            patient.FirstName = request.FirstName;
        }

        if (!string.IsNullOrWhiteSpace(request.LastName))
        {
            patient.LastName = request.LastName;
        }

        await _repository.UpdateAsync(patient, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}

