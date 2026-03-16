using MediatR;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Patients;

namespace RaphCare.Application.Features.Patients.Commands.CreatePatient;

public class CreatePatientHandler(
    IMasterPatientIndexService mpi,
    IRepository<Patient> repository,
    IRepository<PatientExternalId> externalIdRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    IDateTimeProvider dateTimeProvider) : IRequestHandler<CreatePatientCommand, Guid>
{
    private readonly IMasterPatientIndexService _mpi = mpi;
    private readonly IRepository<Patient> _repository = repository;
    private readonly IRepository<PatientExternalId> _externalIdRepository = externalIdRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

    public async Task<Guid> Handle(CreatePatientCommand request, CancellationToken cancellationToken)
    {
        var sourceSystem = request.SourceSystem ?? (request.ClinicId != Guid.Empty ? request.ClinicId.ToString() : null);

        var existing = await _mpi.FindMatchAsync(
            request.NationalHealthId,
            sourceSystem,
            request.ExternalId,
            request.FirstName,
            request.LastName,
            request.DateOfBirth,
            request.PhoneNumber,
            cancellationToken);

        if (existing != null)
            return existing.Id;

        var patient = new Patient
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            DateOfBirth = request.DateOfBirth,
            IsActive = true
        };

        if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
            patient.PhoneNumber = request.PhoneNumber.Trim();

        if (!string.IsNullOrWhiteSpace(request.NationalHealthId))
            patient.SetNationalHealthId(request.NationalHealthId.Trim());

        await _repository.AddAsync(patient, cancellationToken);

        if (request.ClinicId != Guid.Empty)
        {
            var externalIdValue = !string.IsNullOrWhiteSpace(request.ExternalId)
                ? request.ExternalId.Trim()
                : patient.Id.ToString();
            await _externalIdRepository.AddAsync(new PatientExternalId
            {
                PatientId = patient.Id,
                SourceSystem = request.ClinicId.ToString(),
                ExternalId = externalIdValue
            }, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return patient.Id;
    }
}

