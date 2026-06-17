using MediatR;
using RaphCare.Application.Features.PatientTelehealth.DTOs;

namespace RaphCare.Application.Features.PatientTelehealth.Queries.GetTelehealthJoinInfo;

public class GetTelehealthJoinInfoQuery : IRequest<TelehealthJoinInfoDto>
{
    public Guid TeleSessionId { get; set; }
    public uint? Uid { get; set; }
}
