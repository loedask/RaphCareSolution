using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Telemedicine.DTOs;
using RaphCare.Domain.Telemedicine;

namespace RaphCare.Application.Features.Telemedicine.Queries.GetTeleSessionById;

public class GetTeleSessionByIdHandler : IRequestHandler<GetTeleSessionByIdQuery, TeleSessionDto>
{
    private readonly IRepository<TeleSession> _repository;

    public GetTeleSessionByIdHandler(IRepository<TeleSession> repository)
    {
        _repository = repository;
    }

    public async Task<TeleSessionDto> Handle(GetTeleSessionByIdQuery request, CancellationToken cancellationToken)
    {
        var session = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (session is null)
        {
            throw new NotFoundException(nameof(TeleSession), request.Id);
        }

        return new TeleSessionDto
        {
            Id = session.Id,
            ClinicId = session.ClinicId,
            AppointmentId = session.AppointmentId,
            VisitId = session.VisitId,
            PatientId = session.PatientId,
            ProviderId = session.ProviderId,
            ScheduledStart = session.ScheduledStart,
            ActualStart = session.ActualStart,
            ActualEnd = session.ActualEnd,
            Status = session.Status
        };
    }
}

