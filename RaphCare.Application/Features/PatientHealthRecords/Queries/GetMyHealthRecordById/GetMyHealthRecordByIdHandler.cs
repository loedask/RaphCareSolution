using System.Linq;
using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.PatientHealthRecords.DTOs;
using RaphCare.Domain.Clinical;

namespace RaphCare.Application.Features.PatientHealthRecords.Queries.GetMyHealthRecordById;

public class GetMyHealthRecordByIdHandler : IRequestHandler<GetMyHealthRecordByIdQuery, PatientHealthRecordDetailDto>
{
    private readonly IRepository<Visit> _visitRepository;
    private readonly IRepository<VitalSignRecord> _vitalRepository;
    private readonly ICurrentUserService _currentUser;

    public GetMyHealthRecordByIdHandler(
        IRepository<Visit> visitRepository,
        IRepository<VitalSignRecord> vitalRepository,
        ICurrentUserService currentUser)
    {
        _visitRepository = visitRepository;
        _vitalRepository = vitalRepository;
        _currentUser = currentUser;
    }

    public async Task<PatientHealthRecordDetailDto> Handle(GetMyHealthRecordByIdQuery request, CancellationToken cancellationToken)
    {
        var patientId = _currentUser.CurrentPatientId
            ?? throw new ForbiddenAccessException("A patient profile is required to view health records.");

        var visit = await _visitRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
        if (visit is null || visit.PatientId != patientId)
            throw new NotFoundException(nameof(Visit), request.Id);

        var vitalsPage = await _vitalRepository.SearchAsync(
            q => q.Where(v => v.VisitId == visit.Id).OrderBy(v => v.RecordedAt),
            1,
            500,
            applyDefaultIdOrdering: false,
            cancellationToken).ConfigureAwait(false);

        var vitalDtos = vitalsPage.Items.Select(v => new PatientVitalSignDto
        {
            Type = v.Type,
            Value = v.Value,
            Unit = v.Unit,
            RecordedAt = v.RecordedAt
        }).ToList();

        return new PatientHealthRecordDetailDto
        {
            Id = visit.Id,
            VisitStart = visit.VisitStart,
            VisitEnd = visit.VisitEnd,
            VisitType = visit.VisitType,
            Status = visit.Status,
            Summary = visit.Summary,
            VitalSigns = vitalDtos
        };
    }
}
