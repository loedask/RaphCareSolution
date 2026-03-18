using MediatR;
using Microsoft.EntityFrameworkCore;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Patients;

namespace RaphCare.Application.Features.Patients.Commands.CreatePatient;

/// <summary>
/// Handles patient registration by resolving potential duplicates via MPI and persisting the patient record.
/// </summary>
public class CreatePatientHandler(
    IMasterPatientIndexService mpi,
    IRepository<Patient> repository,
    IRepository<PatientExternalId> externalIdRepository,
    IUnitOfWork unitOfWork,
    IPatientUniqueConflictResolver conflictResolver,
    IUniqueConstraintViolationDetector uniqueConstraintDetector,
    ICurrentUserService currentUserService,
    IDateTimeProvider dateTimeProvider) : IRequestHandler<CreatePatientCommand, Guid>
{
    private readonly IMasterPatientIndexService _mpi = mpi;
    private readonly IRepository<Patient> _repository = repository;
    private readonly IRepository<PatientExternalId> _externalIdRepository = externalIdRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IPatientUniqueConflictResolver _conflictResolver = conflictResolver;
    private readonly IUniqueConstraintViolationDetector _uniqueConstraintDetector = uniqueConstraintDetector;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

    /// <summary>
    /// Processes the create patient command, returning the id of the newly created patient
    /// or an existing patient resolved through duplicate/conflict resolution.
    /// </summary>
    /// <param name="request">The create patient command.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The id of the created or resolved patient.</returns>
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

        var externalIdValue = (string?)null;
        if (request.ClinicId != Guid.Empty)
        {
            externalIdValue = !string.IsNullOrWhiteSpace(request.ExternalId)
                ? request.ExternalId.Trim()
                : patient.Id.ToString();
            await _externalIdRepository.AddAsync(new PatientExternalId
            {
                PatientId = patient.Id,
                SourceSystem = request.ClinicId.ToString(),
                ExternalId = externalIdValue
            }, cancellationToken);
        }

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return patient.Id;
        }
        catch (DbUpdateException ex)
        {
            if (!_uniqueConstraintDetector.IsUniqueConstraintViolation(ex))
                throw;

            var resolved = await _conflictResolver.ResolveExistingPatientIdAsync(
                request.NationalHealthId,
                sourceSystem,
                externalIdValue,
                cancellationToken);
            if (resolved.HasValue)
                return resolved.Value;
            throw;
        }
    }
}

