using MediatR;
using RaphCare.Application.Features.Telemedicine.DTOs;

namespace RaphCare.Application.Features.Telemedicine.Queries.GetTeleSessionById;

public class GetTeleSessionByIdQuery : IRequest<TeleSessionDto>
{
    public Guid Id { get; set; }
}

